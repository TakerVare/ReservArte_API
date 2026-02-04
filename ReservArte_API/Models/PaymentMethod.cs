namespace ReservArte_API.Models;

/// <summary>
/// Métodos de pago aceptados en el sistema
/// </summary>
public static class PaymentMethod
{
    /// <summary>
    /// Pago con tarjeta de crédito/débito (a través de Redsys)
    /// </summary>
    public const string Card = "card";
    
    /// <summary>
    /// Pago en efectivo (registro manual por el personal)
    /// </summary>
    public const string Cash = "cash";
    
    /// <summary>
    /// Transferencia bancaria
    /// </summary>
    public const string Transfer = "transfer";
    
    /// <summary>
    /// Bizum (integración con Redsys)
    /// </summary>
    public const string Bizum = "bizum";
    
    /// <summary>
    /// TPV físico (registro en sistema)
    /// </summary>
    public const string TPV = "tpv";
    
    public static readonly string[] All = { Card, Cash, Transfer, Bizum, TPV };
    
    /// <summary>
    /// Métodos que requieren integración con Redsys
    /// </summary>
    public static readonly string[] RedsysMethods = { Card, Bizum };
    
    /// <summary>
    /// Métodos de registro manual (no requieren pasarela)
    /// </summary>
    public static readonly string[] ManualMethods = { Cash, Transfer, TPV };
    
    public static bool IsValid(string? method)
    {
        return !string.IsNullOrEmpty(method) && All.Contains(method);
    }
    
    public static bool RequiresRedsys(string? method)
    {
        return !string.IsNullOrEmpty(method) && RedsysMethods.Contains(method);
    }
    
    public static bool IsManual(string? method)
    {
        return !string.IsNullOrEmpty(method) && ManualMethods.Contains(method);
    }
}
