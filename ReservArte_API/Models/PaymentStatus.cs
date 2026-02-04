namespace ReservArte_API.Models;

/// <summary>
/// Estados posibles de un pago
/// </summary>
public static class PaymentStatus
{
    /// <summary>
    /// Pago pendiente de procesar
    /// </summary>
    public const string Pending = "pending";
    
    /// <summary>
    /// Pre-autorización realizada (dinero bloqueado pero no cobrado)
    /// </summary>
    public const string Authorized = "authorized";
    
    /// <summary>
    /// Pago capturado/cobrado exitosamente
    /// </summary>
    public const string Captured = "captured";
    
    /// <summary>
    /// Pago fallido
    /// </summary>
    public const string Failed = "failed";
    
    /// <summary>
    /// Pago reembolsado completamente
    /// </summary>
    public const string Refunded = "refunded";
    
    /// <summary>
    /// Pago reembolsado parcialmente
    /// </summary>
    public const string PartiallyRefunded = "partially_refunded";
    
    /// <summary>
    /// Pre-autorización cancelada (liberado el bloqueo)
    /// </summary>
    public const string Cancelled = "cancelled";
    
    public static readonly string[] All = { 
        Pending, Authorized, Captured, Failed, Refunded, PartiallyRefunded, Cancelled 
    };
    
    /// <summary>
    /// Estados que indican pago exitoso/completado
    /// </summary>
    public static readonly string[] Successful = { Captured };
    
    /// <summary>
    /// Estados que permiten confirmar/capturar
    /// </summary>
    public static readonly string[] Confirmable = { Authorized };
    
    /// <summary>
    /// Estados que permiten cancelar
    /// </summary>
    public static readonly string[] Cancellable = { Pending, Authorized };
    
    /// <summary>
    /// Estados que permiten reembolso
    /// </summary>
    public static readonly string[] Refundable = { Captured, PartiallyRefunded };
    
    /// <summary>
    /// Estados finales (no se pueden modificar)
    /// </summary>
    public static readonly string[] Final = { Failed, Refunded, Cancelled };
    
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrEmpty(status) && All.Contains(status);
    }
    
    public static bool IsSuccessful(string? status)
    {
        return !string.IsNullOrEmpty(status) && Successful.Contains(status);
    }
    
    public static bool IsConfirmable(string? status)
    {
        return !string.IsNullOrEmpty(status) && Confirmable.Contains(status);
    }
    
    public static bool IsCancellable(string? status)
    {
        return !string.IsNullOrEmpty(status) && Cancellable.Contains(status);
    }
    
    public static bool IsRefundable(string? status)
    {
        return !string.IsNullOrEmpty(status) && Refundable.Contains(status);
    }
    
    public static bool IsFinal(string? status)
    {
        return !string.IsNullOrEmpty(status) && Final.Contains(status);
    }
}
