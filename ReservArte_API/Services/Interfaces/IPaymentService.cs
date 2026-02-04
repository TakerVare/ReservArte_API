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
    
    #region Pagos Manuales
    
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
    /// Procesa un reembolso (manual)
    /// </summary>
    Task<PaymentDtoOut?> ProcessRefundAsync(int id, decimal amount, string? notes = null);
    
    #endregion
    
    #region Estadísticas
    
    /// <summary>
    /// Obtiene estadísticas de pagos de un cliente
    /// </summary>
    Task<CustomerPaymentStatsDto> GetCustomerStatsAsync(int customerId);
    
    #endregion
    
    #region Redsys - Pre-autorización
    
    /// <summary>
    /// Crea una pre-autorización para una cita (bloquea importe sin cobrar)
    /// </summary>
    Task<RedsysPreAuthResponseDto> CreatePreAuthorizationAsync(RedsysPreAuthRequestDto dto);
    
    #endregion
    
    #region Redsys - Confirmación/Captura
    
    /// <summary>
    /// Confirma (captura) una pre-autorización
    /// </summary>
    Task<RedsysConfirmResponseDto> ConfirmPaymentAsync(int paymentId, RedsysConfirmRequestDto? dto = null);
    
    #endregion
    
    #region Redsys - Cancelación
    
    /// <summary>
    /// Cancela una pre-autorización (libera el importe bloqueado)
    /// </summary>
    Task<RedsysCancelResponseDto> CancelPreAuthorizationAsync(int paymentId, RedsysCancelRequestDto? dto = null);
    
    #endregion
    
    #region Redsys - Reembolso
    
    /// <summary>
    /// Procesa un reembolso vía Redsys
    /// </summary>
    Task<RedsysRefundResponseDto> ProcessRedsysRefundAsync(int paymentId, RedsysRefundRequestDto dto);
    
    #endregion
    
    #region Redsys - Webhook
    
    /// <summary>
    /// Procesa notificación webhook de Redsys
    /// </summary>
    Task<bool> ProcessWebhookAsync(RedsysWebhookDto webhook);
    
    #endregion
    
    #region Tarjetas Guardadas
    
    /// <summary>
    /// Obtiene las tarjetas guardadas de un cliente
    /// </summary>
    Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetCustomerPaymentMethodsAsync(int customerId);
    
    /// <summary>
    /// Elimina una tarjeta guardada
    /// </summary>
    Task<bool> DeletePaymentMethodAsync(int paymentMethodId, int customerId);
    
    /// <summary>
    /// Establece una tarjeta como predeterminada
    /// </summary>
    Task<bool> SetDefaultPaymentMethodAsync(int paymentMethodId, int customerId);
    
    /// <summary>
    /// Guarda una tarjeta desde los datos de una transacción exitosa
    /// </summary>
    Task<CustomerPaymentMethodDtoOut?> SaveCardFromTransactionAsync(SaveCardRequestDto dto);
    
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
