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
    private readonly ICustomerPaymentMethodRepository _paymentMethodRepository;
    private readonly IRedsysService _redsysService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ICustomerRepository customerRepository,
        IAppointmentRepository appointmentRepository,
        ICustomerPaymentMethodRepository paymentMethodRepository,
        IRedsysService redsysService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _appointmentRepository = appointmentRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _redsysService = redsysService;
        _logger = logger;
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

    #region Redsys - Pre-autorización

    public async Task<RedsysPreAuthResponseDto> CreatePreAuthorizationAsync(RedsysPreAuthRequestDto dto)
    {
        // Validar que Redsys está configurado
        if (!_redsysService.IsConfigured())
            throw new InvalidOperationException("Redsys no está configurado");

        // Validar cliente
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Cliente no encontrado");

        // Validar cita
        var appointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);
        if (appointment == null)
            throw new InvalidOperationException("Cita no encontrada");

        if (appointment.CustomerId != dto.CustomerId)
            throw new InvalidOperationException("La cita no pertenece al cliente especificado");

        // Generar número de pedido único
        var orderNumber = await _paymentRepository.GenerateOrderNumberAsync();

        // Crear registro de pago en estado pending
        var payment = new Payment
        {
            AppointmentId = dto.AppointmentId,
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            Currency = "EUR",
            PaymentMethodType = PaymentMethod.Card,
            Status = PaymentStatus.Pending,
            RedsysOrderNumber = orderNumber,
            RedsysTransactionType = RedsysTransactionType.PreAuthorization,
            CustomerPaymentMethodId = dto.SavedPaymentMethodId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPayment = await _paymentRepository.CreateAsync(payment);
        if (createdPayment == null)
            throw new InvalidOperationException("Error al crear el registro de pago");

        // Llamar a Redsys
        RedsysOperationResult result;

        if (dto.SavedPaymentMethodId.HasValue)
        {
            // Usar tarjeta guardada
            var savedMethod = await _paymentMethodRepository.GetByIdAsync(dto.SavedPaymentMethodId.Value);
            if (savedMethod == null)
                throw new InvalidOperationException("Método de pago guardado no encontrado");
            
            if (savedMethod.CustomerId != dto.CustomerId)
                throw new InvalidOperationException("El método de pago no pertenece al cliente");
            
            if (savedMethod.IsExpired)
                throw new InvalidOperationException("La tarjeta guardada ha caducado");
            
            result = await _redsysService.CreatePreAuthorizationWithTokenAsync(
                orderNumber,
                dto.Amount,
                savedMethod.RedsysToken,
                savedMethod.RedsysCofTxnid,
                $"appointment:{dto.AppointmentId}");
        }
        else
        {
            // Usar idOper del frontend (InSite)
            if (string.IsNullOrEmpty(dto.IdOper))
                throw new InvalidOperationException("Se requiere idOper o SavedPaymentMethodId");
            
            result = await _redsysService.CreatePreAuthorizationAsync(
                orderNumber,
                dto.Amount,
                dto.IdOper,
                $"appointment:{dto.AppointmentId}");
        }

        // Actualizar pago con resultado
        if (result.Success)
        {
            createdPayment.Status = PaymentStatus.Authorized;
            createdPayment.RedsysAuthCode = result.AuthCode;
            createdPayment.RedsysResponse = result.ResponseCode;
            createdPayment.RedsysCardNumber = result.CardNumber;
            createdPayment.ProcessedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(result.RawResponse))
            {
                createdPayment.Metadata = result.RawResponse;
            }

            // Actualizar cita con datos de Redsys
            appointment.RedsysOrderNumber = orderNumber;
            appointment.RedsysPreAuthToken = result.AuthCode;
            await _appointmentRepository.UpdateAsync(appointment.Id, appointment);
        }
        else
        {
            createdPayment.Status = PaymentStatus.Failed;
            createdPayment.RedsysResponse = result.ResponseCode;
            createdPayment.Notes = result.ErrorMessage;
        }

        createdPayment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(createdPayment.Id, createdPayment);

        _logger.LogInformation(
            "Pre-autorización {Status}. Payment: {PaymentId}, Order: {OrderNumber}, Amount: {Amount}",
            result.Success ? "exitosa" : "fallida",
            createdPayment.Id,
            orderNumber,
            dto.Amount);

        // Si se solicitó guardar la tarjeta y la operación fue exitosa
        string? savedCardToken = null;
        if (result.Success && dto.SaveCard && !string.IsNullOrEmpty(result.CardToken))
        {
            try
            {
                var saveCardDto = new SaveCardRequestDto
                {
                    CustomerId = dto.CustomerId,
                    RedsysToken = result.CardToken,
                    CofTxnId = result.CofTxnId,
                    CardLast4 = result.CardNumber?.Length >= 4 
                        ? result.CardNumber.Substring(result.CardNumber.Length - 4) 
                        : "****",
                    CardBrand = result.CardBrand ?? "Desconocida",
                    CardExpiry = result.ExpiryDate ?? DateTime.UtcNow.AddYears(3).ToString("yyMM"),
                    SetAsDefault = !await _paymentMethodRepository.CustomerHasPaymentMethodsAsync(dto.CustomerId)
                };
                
                var savedCard = await SaveCardFromTransactionAsync(saveCardDto);
                savedCardToken = savedCard != null ? result.CardToken : null;
                
                _logger.LogInformation(
                    "Tarjeta guardada para cliente {CustomerId}. Last4: {Last4}",
                    dto.CustomerId,
                    saveCardDto.CardLast4);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar tarjeta para cliente {CustomerId}", dto.CustomerId);
                // No fallar la operación si no se pudo guardar la tarjeta
            }
        }

        return new RedsysPreAuthResponseDto
        {
            PaymentId = createdPayment.Id,
            OrderNumber = orderNumber,
            Amount = dto.Amount,
            Status = createdPayment.Status,
            AuthCode = result.AuthCode,
            ResponseCode = result.ResponseCode,
            ResponseMessage = result.ResponseMessage,
            Success = result.Success,
            CardLast4 = result.CardNumber?.Length > 4 
                ? result.CardNumber.Substring(result.CardNumber.Length - 4) 
                : null,
            CardBrand = result.CardBrand,
            CardToken = savedCardToken,
            ExpiresAt = result.Success ? DateTime.UtcNow.AddDays(7) : null
        };
    }

    #endregion

    #region Redsys - Confirmación/Captura

    public async Task<RedsysConfirmResponseDto> ConfirmPaymentAsync(int paymentId, RedsysConfirmRequestDto? dto = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException("Pago no encontrado");

        if (!PaymentStatus.IsConfirmable(payment.Status))
            throw new InvalidOperationException(
                $"No se puede confirmar un pago en estado '{payment.Status}'");

        if (string.IsNullOrEmpty(payment.RedsysOrderNumber))
            throw new InvalidOperationException("El pago no tiene número de pedido Redsys");

        // Determinar importe a capturar
        var captureAmount = dto?.Amount ?? payment.Amount;
        if (captureAmount > payment.Amount)
            throw new InvalidOperationException(
                $"El importe a capturar ({captureAmount:C}) no puede superar el pre-autorizado ({payment.Amount:C})");

        // Llamar a Redsys
        var result = await _redsysService.ConfirmPreAuthorizationAsync(
            payment.RedsysOrderNumber,
            captureAmount);

        // Actualizar pago
        if (result.Success)
        {
            payment.Status = PaymentStatus.Captured;
            payment.Amount = captureAmount; // Actualizar al importe realmente capturado
            payment.RedsysAuthCode = result.AuthCode ?? payment.RedsysAuthCode;
            payment.RedsysResponse = result.ResponseCode;
            payment.RedsysTransactionType = RedsysTransactionType.Confirmation;
            payment.ProcessedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(dto?.Notes))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes) 
                    ? dto.Notes 
                    : $"{payment.Notes}\n{dto.Notes}";
            }
        }
        else
        {
            payment.RedsysResponse = result.ResponseCode;
            payment.Notes = $"{payment.Notes}\n[Error confirmación: {result.ErrorMessage}]";
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(paymentId, payment);

        _logger.LogInformation(
            "Confirmación {Status}. Payment: {PaymentId}, Order: {OrderNumber}, Amount: {Amount}",
            result.Success ? "exitosa" : "fallida",
            paymentId,
            payment.RedsysOrderNumber,
            captureAmount);

        return new RedsysConfirmResponseDto
        {
            PaymentId = paymentId,
            OrderNumber = payment.RedsysOrderNumber,
            OriginalAmount = payment.Amount,
            CapturedAmount = captureAmount,
            Status = payment.Status,
            AuthCode = result.AuthCode,
            ResponseCode = result.ResponseCode,
            ResponseMessage = result.ResponseMessage,
            Success = result.Success,
            ProcessedAt = payment.ProcessedAt
        };
    }

    #endregion

    #region Redsys - Cancelación

    public async Task<RedsysCancelResponseDto> CancelPreAuthorizationAsync(int paymentId, RedsysCancelRequestDto? dto = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException("Pago no encontrado");

        if (!PaymentStatus.IsCancellable(payment.Status))
            throw new InvalidOperationException(
                $"No se puede cancelar un pago en estado '{payment.Status}'");

        if (string.IsNullOrEmpty(payment.RedsysOrderNumber))
            throw new InvalidOperationException("El pago no tiene número de pedido Redsys");

        // Llamar a Redsys
        var result = await _redsysService.CancelPreAuthorizationAsync(
            payment.RedsysOrderNumber,
            payment.Amount);

        // Actualizar pago
        if (result.Success)
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.RedsysResponse = result.ResponseCode;
            payment.RedsysTransactionType = RedsysTransactionType.Cancellation;
            payment.ProcessedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(dto?.Reason))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes) 
                    ? $"Cancelado: {dto.Reason}" 
                    : $"{payment.Notes}\n[Cancelado: {dto.Reason}]";
            }
        }
        else
        {
            payment.RedsysResponse = result.ResponseCode;
            payment.Notes = $"{payment.Notes}\n[Error cancelación: {result.ErrorMessage}]";
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(paymentId, payment);

        _logger.LogInformation(
            "Cancelación {Status}. Payment: {PaymentId}, Order: {OrderNumber}",
            result.Success ? "exitosa" : "fallida",
            paymentId,
            payment.RedsysOrderNumber);

        return new RedsysCancelResponseDto
        {
            PaymentId = paymentId,
            OrderNumber = payment.RedsysOrderNumber,
            Amount = payment.Amount,
            Status = payment.Status,
            ResponseCode = result.ResponseCode,
            ResponseMessage = result.ResponseMessage,
            Success = result.Success,
            CancelledAt = result.Success ? DateTime.UtcNow : null
        };
    }

    #endregion

    #region Redsys - Reembolso

    public async Task<RedsysRefundResponseDto> ProcessRedsysRefundAsync(int paymentId, RedsysRefundRequestDto dto)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException("Pago no encontrado");

        if (!PaymentStatus.IsRefundable(payment.Status))
            throw new InvalidOperationException(
                $"No se puede reembolsar un pago en estado '{payment.Status}'");

        if (string.IsNullOrEmpty(payment.RedsysOrderNumber))
            throw new InvalidOperationException("El pago no tiene número de pedido Redsys");

        if (dto.Amount > payment.RemainingAmount)
            throw new InvalidOperationException(
                $"El importe a reembolsar ({dto.Amount:C}) supera el pendiente ({payment.RemainingAmount:C})");

        // Llamar a Redsys
        var result = await _redsysService.ProcessRefundAsync(
            payment.RedsysOrderNumber,
            dto.Amount);

        // Actualizar pago
        if (result.Success)
        {
            payment.RefundedAmount += dto.Amount;
            payment.RefundedAt = DateTime.UtcNow;
            payment.Status = payment.IsFullyRefunded 
                ? PaymentStatus.Refunded 
                : PaymentStatus.PartiallyRefunded;
            payment.RedsysResponse = result.ResponseCode;
            
            if (!string.IsNullOrEmpty(dto.Reason))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes) 
                    ? $"Reembolso: {dto.Reason}" 
                    : $"{payment.Notes}\n[Reembolso {dto.Amount:C}: {dto.Reason}]";
            }
        }
        else
        {
            payment.RedsysResponse = result.ResponseCode;
            payment.Notes = $"{payment.Notes}\n[Error reembolso: {result.ErrorMessage}]";
        }

        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(paymentId, payment);

        _logger.LogInformation(
            "Reembolso {Status}. Payment: {PaymentId}, Order: {OrderNumber}, Amount: {Amount}",
            result.Success ? "exitoso" : "fallido",
            paymentId,
            payment.RedsysOrderNumber,
            dto.Amount);

        return new RedsysRefundResponseDto
        {
            PaymentId = paymentId,
            OrderNumber = payment.RedsysOrderNumber,
            OriginalAmount = payment.Amount,
            RefundedAmount = dto.Amount,
            TotalRefunded = payment.RefundedAmount,
            RemainingAmount = payment.RemainingAmount,
            Status = payment.Status,
            ResponseCode = result.ResponseCode,
            ResponseMessage = result.ResponseMessage,
            Success = result.Success,
            RefundedAt = result.Success ? DateTime.UtcNow : null
        };
    }

    #endregion

    #region Redsys - Webhook

    public async Task<bool> ProcessWebhookAsync(RedsysWebhookDto webhook)
    {
        // Validar firma
        if (!_redsysService.ValidateWebhookSignature(webhook))
        {
            _logger.LogWarning("Webhook Redsys con firma inválida");
            return false;
        }

        // Parsear datos
        var data = _redsysService.ParseWebhookData(webhook.Ds_MerchantParameters);
        if (data == null || string.IsNullOrEmpty(data.Ds_Order))
        {
            _logger.LogWarning("Webhook Redsys con datos inválidos");
            return false;
        }

        // Buscar pago por número de pedido
        var payment = await _paymentRepository.GetByRedsysOrderNumberAsync(data.Ds_Order);
        if (payment == null)
        {
            _logger.LogWarning("Webhook para pedido no encontrado: {OrderNumber}", data.Ds_Order);
            return false;
        }

        // Actualizar estado según respuesta
        var previousStatus = payment.Status;
        
        if (data.IsSuccessful)
        {
            // Determinar nuevo estado según tipo de transacción
            payment.Status = data.Ds_TransactionType switch
            {
                RedsysTransactionType.PreAuthorization => PaymentStatus.Authorized,
                RedsysTransactionType.Confirmation => PaymentStatus.Captured,
                RedsysTransactionType.Cancellation => PaymentStatus.Cancelled,
                RedsysTransactionType.AutomaticRefund => payment.IsFullyRefunded 
                    ? PaymentStatus.Refunded 
                    : PaymentStatus.PartiallyRefunded,
                _ => payment.Status
            };

            payment.RedsysAuthCode = data.Ds_AuthorisationCode ?? payment.RedsysAuthCode;
            payment.RedsysCardNumber = data.Ds_Card_Number ?? payment.RedsysCardNumber;
            payment.ProcessedAt = DateTime.UtcNow;
        }
        else
        {
            // Solo marcar como fallido si no estaba ya en un estado final
            if (!PaymentStatus.IsFinal(payment.Status))
            {
                payment.Status = PaymentStatus.Failed;
            }
        }

        payment.RedsysResponse = data.Ds_Response;
        payment.RedsysTransactionType = data.Ds_TransactionType;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment.Id, payment);

        _logger.LogInformation(
            "Webhook procesado. Order: {OrderNumber}, Status: {PrevStatus} -> {NewStatus}, Response: {Response}",
            data.Ds_Order,
            previousStatus,
            payment.Status,
            data.Ds_Response);

        return true;
    }

    #endregion

    #region Tarjetas Guardadas

    public async Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetCustomerPaymentMethodsAsync(int customerId)
    {
        return await _paymentMethodRepository.GetByCustomerIdAsync(customerId);
    }

    public async Task<bool> DeletePaymentMethodAsync(int paymentMethodId, int customerId)
    {
        var method = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (method == null)
            return false;
        
        if (method.CustomerId != customerId)
            throw new InvalidOperationException("El método de pago no pertenece al cliente");
        
        return await _paymentMethodRepository.DeleteAsync(paymentMethodId);
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(int paymentMethodId, int customerId)
    {
        var method = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
        if (method == null)
            throw new InvalidOperationException("Método de pago no encontrado");
        
        if (method.CustomerId != customerId)
            throw new InvalidOperationException("El método de pago no pertenece al cliente");
        
        return await _paymentMethodRepository.SetAsDefaultAsync(paymentMethodId, customerId);
    }

    public async Task<CustomerPaymentMethodDtoOut?> SaveCardFromTransactionAsync(SaveCardRequestDto dto)
    {
        // Validar cliente
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Cliente no encontrado");
        
        // Crear método de pago
        var paymentMethod = new CustomerPaymentMethod
        {
            CustomerId = dto.CustomerId,
            RedsysToken = dto.RedsysToken,
            RedsysCofTxnid = dto.CofTxnId,
            CardLast4 = dto.CardLast4,
            CardBrand = dto.CardBrand,
            CardExpiry = dto.CardExpiry,
            IsDefault = dto.SetAsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var created = await _paymentMethodRepository.CreateAsync(paymentMethod);
        if (created == null)
            return null;
        
        return new CustomerPaymentMethodDtoOut
        {
            Id = created.Id,
            CustomerId = created.CustomerId,
            CardLast4 = created.CardLast4,
            CardBrand = created.CardBrand,
            CardExpiry = created.FormattedExpiry,
            IsDefault = created.IsDefault,
            IsExpired = created.IsExpired,
            CreatedAt = created.CreatedAt
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
