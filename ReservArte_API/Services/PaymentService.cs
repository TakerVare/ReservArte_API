using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ICustomerRepository customerRepository,
        IAppointmentRepository appointmentRepository)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _appointmentRepository = appointmentRepository;
    }

    #region CRUD Básico

    public async Task<IEnumerable<PaymentListDto>> GetAllAsync()
    {
        return await _paymentRepository.GetAllAsync();
    }

    public async Task<PaymentPagedResultDto> GetFilteredAsync(PaymentFilterDto filter)
    {
        // Validar filtros
        if (filter.Page < 1)
            filter.Page = 1;
        
        if (filter.PageSize < 1 || filter.PageSize > 100)
            filter.PageSize = 20;
        
        if (!string.IsNullOrEmpty(filter.Status) && !PaymentStatus.IsValid(filter.Status))
            throw new InvalidOperationException($"Estado de pago inválido: {filter.Status}");
        
        if (!string.IsNullOrEmpty(filter.PaymentMethodType) && !PaymentMethod.IsValid(filter.PaymentMethodType))
            throw new InvalidOperationException($"Método de pago inválido: {filter.PaymentMethodType}");
        
        return await _paymentRepository.GetFilteredAsync(filter);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _paymentRepository.GetByIdAsync(id);
    }

    public async Task<PaymentDtoOut?> GetByIdDetailedAsync(int id)
    {
        return await _paymentRepository.GetByIdDetailedAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            return false;
        
        // Solo permitir eliminar pagos pendientes o fallidos
        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Failed)
            throw new InvalidOperationException("Solo se pueden eliminar pagos pendientes o fallidos");
        
        return await _paymentRepository.DeleteAsync(id);
    }

    #endregion

    #region Consultas

    public async Task<IEnumerable<PaymentListDto>> GetByCustomerIdAsync(int customerId)
    {
        return await _paymentRepository.GetByCustomerIdAsync(customerId);
    }

    public async Task<IEnumerable<PaymentListDto>> GetByAppointmentIdAsync(int appointmentId)
    {
        return await _paymentRepository.GetByAppointmentIdAsync(appointmentId);
    }

    #endregion

    #region Pagos Manuales

    public async Task<PaymentDtoOut?> CreateManualPaymentAsync(PaymentDtoIn dto, int registeredById)
    {
        // Validar que es un método de pago manual
        if (!PaymentMethod.IsManual(dto.PaymentMethodType))
            throw new InvalidOperationException(
                $"El método de pago '{dto.PaymentMethodType}' no es válido para registro manual. " +
                $"Use: {string.Join(", ", PaymentMethod.ManualMethods)}");
        
        // Validar que el cliente existe
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Cliente no encontrado");
        
        // Si hay cita asociada, validarla
        if (dto.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId.Value);
            if (appointment == null)
                throw new InvalidOperationException("Cita no encontrada");
            
            if (appointment.CustomerId != dto.CustomerId)
                throw new InvalidOperationException("La cita no pertenece al cliente especificado");
        }
        
        // Crear el pago
        var payment = new Payment
        {
            AppointmentId = dto.AppointmentId,
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            Currency = "EUR",
            PaymentMethodType = dto.PaymentMethodType,
            Status = PaymentStatus.Captured, // Los pagos manuales se marcan como capturados directamente
            ProcessedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            RegisteredById = registeredById,
            CreatedAt = DateTime.UtcNow
        };
        
        // Si hay referencia externa, guardarla en metadata
        if (!string.IsNullOrEmpty(dto.ExternalReference))
        {
            payment.Metadata = System.Text.Json.JsonSerializer.Serialize(new 
            { 
                externalReference = dto.ExternalReference 
            });
        }
        
        var created = await _paymentRepository.CreateAsync(payment);
        if (created == null)
            throw new InvalidOperationException("Error al crear el pago");
        
        return await _paymentRepository.GetByIdDetailedAsync(created.Id);
    }

    public async Task<PaymentDtoOut?> CreateManualPaymentForAppointmentAsync(int appointmentId, PaymentDtoIn dto, int registeredById)
    {
        // Obtener la cita
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
            throw new InvalidOperationException("Cita no encontrada");
        
        // Verificar que la cita está en un estado que permite pago
        if (appointment.Status != Status.Confirmed && 
            appointment.Status != Status.InProgress && 
            appointment.Status != Status.Completed)
        {
            throw new InvalidOperationException(
                $"No se puede registrar pago para una cita en estado '{appointment.Status}'");
        }
        
        // Establecer los datos de la cita
        dto.AppointmentId = appointmentId;
        dto.CustomerId = appointment.CustomerId;
        
        // Si no se especifica importe, usar el de la cita
        if (dto.Amount <= 0)
            dto.Amount = appointment.TotalPrice;
        
        return await CreateManualPaymentAsync(dto, registeredById);
    }

    #endregion

    #region Operaciones de Estado

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        if (!PaymentStatus.IsValid(status))
            throw new InvalidOperationException($"Estado de pago inválido: {status}");
        
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            throw new InvalidOperationException("Pago no encontrado");
        
        // Validar transición de estado
        if (!IsValidStatusTransition(payment.Status, status))
            throw new InvalidOperationException(
                $"No se puede cambiar el estado de '{payment.Status}' a '{status}'");
        
        return await _paymentRepository.UpdateStatusAsync(id, status);
    }

    public async Task<PaymentDtoOut?> ProcessRefundAsync(int id, decimal amount, string? notes = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
            throw new InvalidOperationException("Pago no encontrado");
        
        if (!PaymentStatus.IsRefundable(payment.Status))
            throw new InvalidOperationException(
                $"No se puede reembolsar un pago en estado '{payment.Status}'");
        
        if (amount <= 0)
            throw new InvalidOperationException("El importe de reembolso debe ser mayor que 0");
        
        if (amount > payment.RemainingAmount)
            throw new InvalidOperationException(
                $"El importe de reembolso ({amount:C}) supera el importe pendiente ({payment.RemainingAmount:C})");
        
        // Para pagos con Redsys, el reembolso real se procesará en Fase 2
        // Por ahora solo actualizamos el registro
        var success = await _paymentRepository.UpdateRefundAsync(id, amount);
        if (!success)
            throw new InvalidOperationException("Error al procesar el reembolso");
        
        // Actualizar notas si se proporcionan
        if (!string.IsNullOrEmpty(notes))
        {
            payment.Notes = string.IsNullOrEmpty(payment.Notes) 
                ? notes 
                : $"{payment.Notes}\n[Reembolso: {notes}]";
            await _paymentRepository.UpdateAsync(id, payment);
        }
        
        return await _paymentRepository.GetByIdDetailedAsync(id);
    }

    #endregion

    #region Estadísticas

    public async Task<CustomerPaymentStatsDto> GetCustomerStatsAsync(int customerId)
    {
        var totalSpent = await _paymentRepository.GetCustomerTotalSpentAsync(customerId);
        var paymentCount = await _paymentRepository.GetCustomerPaymentCountAsync(customerId);
        
        return new CustomerPaymentStatsDto
        {
            CustomerId = customerId,
            TotalSpent = totalSpent,
            PaymentCount = paymentCount
        };
    }

    #endregion

    #region Métodos Privados

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            // Desde Pending
            (PaymentStatus.Pending, PaymentStatus.Authorized) => true,
            (PaymentStatus.Pending, PaymentStatus.Captured) => true,
            (PaymentStatus.Pending, PaymentStatus.Failed) => true,
            (PaymentStatus.Pending, PaymentStatus.Cancelled) => true,
            
            // Desde Authorized (pre-autorizado)
            (PaymentStatus.Authorized, PaymentStatus.Captured) => true,
            (PaymentStatus.Authorized, PaymentStatus.Cancelled) => true,
            (PaymentStatus.Authorized, PaymentStatus.Failed) => true,
            
            // Desde Captured (cobrado)
            (PaymentStatus.Captured, PaymentStatus.PartiallyRefunded) => true,
            (PaymentStatus.Captured, PaymentStatus.Refunded) => true,
            
            // Desde PartiallyRefunded
            (PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded) => true,
            
            // No se permiten otras transiciones
            _ => false
        };
    }

    #endregion
}
