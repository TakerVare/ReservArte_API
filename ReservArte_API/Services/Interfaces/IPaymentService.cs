using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IPaymentService
{
    #region CRUD Básico
    
    Task<IEnumerable<PaymentListDto>> GetAllAsync();
    Task<PaymentPagedResultDto> GetFilteredAsync(PaymentFilterDto filter);
    Task<Payment?> GetByIdAsync(int id);
    Task<PaymentDtoOut?> GetByIdDetailedAsync(int id);
    Task<bool> DeleteAsync(int id);
    
    #endregion
    
    #region Consultas
    
    Task<IEnumerable<PaymentListDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<PaymentListDto>> GetByAppointmentIdAsync(int appointmentId);
    
    #endregion
    
    #region Pagos Manuales (Fase 1)
    
    /// <summary>
    /// Registra un pago manual (efectivo, transferencia, TPV)
    /// </summary>
    Task<PaymentDtoOut?> CreateManualPaymentAsync(PaymentDtoIn dto, int registeredById);
    
    /// <summary>
    /// Registra pago manual asociado a una cita
    /// </summary>
    Task<PaymentDtoOut?> CreateManualPaymentForAppointmentAsync(int appointmentId, PaymentDtoIn dto, int registeredById);
    
    #endregion
    
    #region Operaciones de Estado
    
    /// <summary>
    /// Actualiza el estado de un pago
    /// </summary>
    Task<bool> UpdateStatusAsync(int id, string status);
    
    /// <summary>
    /// Procesa un reembolso
    /// </summary>
    Task<PaymentDtoOut?> ProcessRefundAsync(int id, decimal amount, string? notes = null);
    
    #endregion
    
    #region Estadísticas
    
    /// <summary>
    /// Obtiene estadísticas de pagos de un cliente
    /// </summary>
    Task<CustomerPaymentStatsDto> GetCustomerStatsAsync(int customerId);
    
    #endregion
    
    #region Redsys (Placeholder para Fase 2)
    
    // Los métodos de Redsys se añadirán en la Fase 2:
    // Task<PaymentDtoOut?> CreatePreAuthorizationAsync(RedsysPreAuthDto dto);
    // Task<PaymentDtoOut?> ConfirmPaymentAsync(int paymentId, decimal? amount = null);
    // Task<PaymentDtoOut?> CancelPreAuthorizationAsync(int paymentId);
    // Task<bool> ProcessWebhookAsync(RedsysWebhookDto webhook);
    
    #endregion
}

/// <summary>
/// Estadísticas de pagos de un cliente
/// </summary>
public class CustomerPaymentStatsDto
{
    public int CustomerId { get; set; }
    public decimal TotalSpent { get; set; }
    public int PaymentCount { get; set; }
    public decimal AveragePayment => PaymentCount > 0 ? TotalSpent / PaymentCount : 0;
}
