/*
 * PaymentDtoOut.cs
 * ================
 * 
 * PROPÓSITO:
 * DTO de salida con información detallada de un pago.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: GET /api/payment/{id}, POST /api/payment/manual
 * - PaymentService.cs: GetByIdAsync(), CreateManualPaymentAsync()
 * - PaymentRepository.cs: GetByIdDetailedAsync()
 * 
 * PARA QUÉ SE USA:
 * - Mostrar detalles completos de un pago individual
 * - Respuesta tras crear/modificar un pago
 * - Incluye datos de Redsys para pagos con tarjeta
 */

namespace ReservArte_API.Models.DTOs;

public class PaymentDtoOut
{
    public int Id { get; set; }
    
    // Relaciones
    public int? AppointmentId { get; set; }
    public string? AppointmentDate { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    // Datos del pago
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string PaymentMethodType { get; set; } = string.Empty;
    public string PaymentMethodDescription => GetPaymentMethodDescription();
    public string Status { get; set; } = string.Empty;
    public string StatusDescription => GetStatusDescription();
    
    // Campos Redsys (solo para pagos con tarjeta)
    public string? RedsysOrderNumber { get; set; }
    public string? RedsysAuthCode { get; set; }
    public string? RedsysResponse { get; set; }
    public string? RedsysTransactionType { get; set; }
    public string? RedsysCardNumber { get; set; }
    
    // Método de pago guardado
    public int? CustomerPaymentMethodId { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    
    // Reembolsos
    public decimal RefundedAmount { get; set; }
    public decimal RemainingAmount => Amount - RefundedAmount;
    public DateTime? RefundedAt { get; set; }
    
    // Metadatos
    public string? Notes { get; set; }
    public int? RegisteredById { get; set; }
    public string? RegisteredByName { get; set; }
    
    // Timestamps
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    #region Helpers
    
    private string GetPaymentMethodDescription()
    {
        return PaymentMethodType switch
        {
            Models.PaymentMethod.Card => "Tarjeta",
            Models.PaymentMethod.Cash => "Efectivo",
            Models.PaymentMethod.Transfer => "Transferencia",
            Models.PaymentMethod.Bizum => "Bizum",
            Models.PaymentMethod.TPV => "TPV físico",
            _ => PaymentMethodType
        };
    }
    
    private string GetStatusDescription()
    {
        return Status switch
        {
            PaymentStatus.Pending => "Pendiente",
            PaymentStatus.Authorized => "Pre-autorizado",
            PaymentStatus.Captured => "Cobrado",
            PaymentStatus.Failed => "Fallido",
            PaymentStatus.Refunded => "Reembolsado",
            PaymentStatus.PartiallyRefunded => "Reembolso parcial",
            PaymentStatus.Cancelled => "Cancelado",
            _ => Status
        };
    }
    
    #endregion
}
