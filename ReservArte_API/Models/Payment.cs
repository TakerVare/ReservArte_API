namespace ReservArte_API.Models;

/// <summary>
/// Entidad principal de pago
/// Registra todas las transacciones de pago del sistema
/// </summary>
public class Payment
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID de la cita asociada (nullable para pagos no vinculados a cita)
    /// </summary>
    public int? AppointmentId { get; set; }
    
    /// <summary>
    /// ID del cliente que realiza el pago
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Importe del pago en céntimos (para evitar problemas de precisión decimal)
    /// Ejemplo: 1000 = 10.00 EUR
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Código de moneda ISO 4217 (978 = EUR)
    /// </summary>
    public string Currency { get; set; } = "EUR";
    
    /// <summary>
    /// Método de pago: card, cash, transfer, bizum, tpv
    /// </summary>
    public string PaymentMethodType { get; set; } = PaymentMethod.Card;
    
    /// <summary>
    /// Estado del pago: pending, authorized, captured, failed, refunded, partially_refunded, cancelled
    /// </summary>
    public string Status { get; set; } = PaymentStatus.Pending;
    
    #region Campos Redsys
    
    /// <summary>
    /// Número de pedido único para Redsys (12 caracteres alfanuméricos)
    /// Formato sugerido: YYYYMMDD + 4 dígitos secuenciales
    /// </summary>
    public string? RedsysOrderNumber { get; set; }
    
    /// <summary>
    /// Código de autorización devuelto por Redsys
    /// </summary>
    public string? RedsysAuthCode { get; set; }
    
    /// <summary>
    /// Código de respuesta de Redsys (0000-0099 = éxito)
    /// </summary>
    public string? RedsysResponse { get; set; }
    
    /// <summary>
    /// Tipo de transacción Redsys: 0=Auth, 1=PreAuth, 2=Confirm, 9=Cancel
    /// </summary>
    public string? RedsysTransactionType { get; set; }
    
    /// <summary>
    /// PAN enmascarado de la tarjeta (ej: 454881******0004)
    /// </summary>
    public string? RedsysCardNumber { get; set; }
    
    #endregion
    
    /// <summary>
    /// Referencia al método de pago guardado del cliente (tarjeta tokenizada)
    /// </summary>
    public int? CustomerPaymentMethodId { get; set; }
    
    /// <summary>
    /// Fecha/hora en que se procesó el pago
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Importe total reembolsado
    /// </summary>
    public decimal RefundedAmount { get; set; } = 0;
    
    /// <summary>
    /// Fecha/hora del último reembolso
    /// </summary>
    public DateTime? RefundedAt { get; set; }
    
    /// <summary>
    /// Datos adicionales en formato JSON (respuestas completas de Redsys, notas, etc.)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Notas internas sobre el pago
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// ID del usuario que registró el pago (para pagos manuales)
    /// </summary>
    public int? RegisteredById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    #region Propiedades de navegación para DTOs
    
    public string? CustomerName { get; set; }
    public string? AppointmentDate { get; set; }
    
    #endregion
    
    #region Métodos de utilidad
    
    /// <summary>
    /// Verifica si el pago fue exitoso según el código de respuesta de Redsys
    /// </summary>
    public bool IsRedsysSuccessful => 
        !string.IsNullOrEmpty(RedsysResponse) && 
        int.TryParse(RedsysResponse, out int code) && 
        code >= 0 && code <= 99;
    
    /// <summary>
    /// Importe pendiente de reembolso
    /// </summary>
    public decimal RemainingAmount => Amount - RefundedAmount;
    
    /// <summary>
    /// Verifica si el pago está completamente reembolsado
    /// </summary>
    public bool IsFullyRefunded => RefundedAmount >= Amount;
    
    #endregion
}
