using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

/// <summary>
/// Servicio para integración con la pasarela de pagos Redsys
/// </summary>
public interface IRedsysService
{
    #region Pre-autorización
    
    /// <summary>
    /// Crea una pre-autorización (bloquea importe sin cobrar)
    /// Transacción tipo "1"
    /// </summary>
    Task<RedsysOperationResult> CreatePreAuthorizationAsync(
        string orderNumber,
        decimal amount,
        string idOper,
        string? merchantData = null);
    
    /// <summary>
    /// Crea una pre-autorización usando token guardado
    /// </summary>
    Task<RedsysOperationResult> CreatePreAuthorizationWithTokenAsync(
        string orderNumber,
        decimal amount,
        string cardToken,
        string? cofTxnId = null,
        string? merchantData = null);
    
    #endregion
    
    #region Confirmación/Captura
    
    /// <summary>
    /// Confirma (captura) una pre-autorización
    /// Transacción tipo "2"
    /// </summary>
    Task<RedsysOperationResult> ConfirmPreAuthorizationAsync(
        string orderNumber,
        decimal amount);
    
    #endregion
    
    #region Cancelación
    
    /// <summary>
    /// Cancela una pre-autorización (libera el importe bloqueado)
    /// Transacción tipo "9"
    /// </summary>
    Task<RedsysOperationResult> CancelPreAuthorizationAsync(
        string orderNumber,
        decimal amount);
    
    #endregion
    
    #region Reembolso
    
    /// <summary>
    /// Procesa un reembolso de un pago capturado
    /// Transacción tipo "3"
    /// </summary>
    Task<RedsysOperationResult> ProcessRefundAsync(
        string orderNumber,
        decimal amount);
    
    #endregion
    
    #region Webhooks
    
    /// <summary>
    /// Valida la firma del webhook de Redsys
    /// </summary>
    bool ValidateWebhookSignature(RedsysWebhookDto webhook);
    
    /// <summary>
    /// Decodifica y parsea los datos del webhook
    /// </summary>
    RedsysWebhookDataDto? ParseWebhookData(string merchantParameters);
    
    #endregion
    
    #region Utilidades
    
    /// <summary>
    /// Verifica si la configuración de Redsys es válida
    /// </summary>
    bool IsConfigured();
    
    /// <summary>
    /// Genera firma HMAC SHA-256
    /// </summary>
    string GenerateSignature(string merchantParameters, string orderNumber);
    
    /// <summary>
    /// Convierte importe de euros a céntimos para Redsys
    /// </summary>
    string AmountToCents(decimal amount);
    
    /// <summary>
    /// Convierte importe de céntimos a euros
    /// </summary>
    decimal CentsToAmount(string cents);
    
    #endregion
}

/// <summary>
/// Resultado de una operación con Redsys
/// </summary>
public class RedsysOperationResult
{
    public bool Success { get; set; }
    public string? OrderNumber { get; set; }
    public string? AuthCode { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? CardNumber { get; set; }
    public string? CardBrand { get; set; }
    public string? CardToken { get; set; }
    public string? CofTxnId { get; set; }
    public string? ExpiryDate { get; set; }
    public string? RawResponse { get; set; }
    public string? ErrorMessage { get; set; }
    
    public static RedsysOperationResult Successful(
        string orderNumber, 
        string? authCode = null, 
        string? responseCode = null)
    {
        return new RedsysOperationResult
        {
            Success = true,
            OrderNumber = orderNumber,
            AuthCode = authCode,
            ResponseCode = responseCode,
            ResponseMessage = GetResponseMessage(responseCode)
        };
    }
    
    public static RedsysOperationResult Failed(string errorMessage, string? responseCode = null)
    {
        return new RedsysOperationResult
        {
            Success = false,
            ResponseCode = responseCode,
            ResponseMessage = GetResponseMessage(responseCode),
            ErrorMessage = errorMessage
        };
    }
    
    public static RedsysOperationResult NotConfigured()
    {
        return new RedsysOperationResult
        {
            Success = false,
            ErrorMessage = "Redsys no está configurado. Verifique las credenciales en appsettings.json"
        };
    }
    
    private static string? GetResponseMessage(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return null;
        
        if (!int.TryParse(code, out int intCode))
            return $"Código no válido: {code}";
        
        return intCode switch
        {
            >= 0 and <= 99 => "Transacción autorizada",
            101 => "Tarjeta caducada",
            102 => "Tarjeta bloqueada temporalmente",
            104 => "Operación no permitida",
            116 => "Saldo insuficiente",
            129 => "CVV2/CVC2 incorrecto",
            180 => "Tarjeta no válida",
            190 => "Denegación del emisor",
            _ => $"Error: {code}"
        };
    }
}
