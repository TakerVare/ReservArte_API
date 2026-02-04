namespace ReservArte_API.Models;

/// <summary>
/// Tipos de transacción de Redsys (DS_MERCHANT_TRANSACTIONTYPE)
/// </summary>
public static class RedsysTransactionType
{
    /// <summary>
    /// Autorización estándar (cobro directo)
    /// </summary>
    public const string Authorization = "0";
    
    /// <summary>
    /// Pre-autorización (bloquea el importe sin cobrar)
    /// Válida por 7 días
    /// </summary>
    public const string PreAuthorization = "1";
    
    /// <summary>
    /// Confirmación de pre-autorización (captura el importe bloqueado)
    /// </summary>
    public const string Confirmation = "2";
    
    /// <summary>
    /// Devolución automática
    /// </summary>
    public const string AutomaticRefund = "3";
    
    /// <summary>
    /// Cancelación de pre-autorización (libera el importe bloqueado)
    /// </summary>
    public const string Cancellation = "9";
    
    public static readonly string[] All = { 
        Authorization, PreAuthorization, Confirmation, AutomaticRefund, Cancellation 
    };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
    
    /// <summary>
    /// Obtiene descripción legible del tipo de transacción
    /// </summary>
    public static string GetDescription(string? type)
    {
        return type switch
        {
            Authorization => "Autorización",
            PreAuthorization => "Pre-autorización",
            Confirmation => "Confirmación",
            AutomaticRefund => "Devolución",
            Cancellation => "Cancelación",
            _ => "Desconocido"
        };
    }
}
