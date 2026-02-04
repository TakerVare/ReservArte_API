namespace ReservArte_API.Models;

/// <summary>
/// Configuración de Redsys cargada desde appsettings.json
/// </summary>
public class RedsysSettings
{
    public const string SectionName = "Redsys";
    
    /// <summary>
    /// Código de comercio (FUC) proporcionado por Redsys
    /// </summary>
    public string MerchantCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de terminal (normalmente "1")
    /// </summary>
    public string Terminal { get; set; } = "1";
    
    /// <summary>
    /// Clave secreta para firma HMAC SHA-256
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Código de moneda ISO 4217 (978 = EUR)
    /// </summary>
    public string Currency { get; set; } = "978";
    
    /// <summary>
    /// Entorno: "test" o "live"
    /// </summary>
    public string Environment { get; set; } = "test";
    
    /// <summary>
    /// URL de notificación (webhook) para respuestas asíncronas
    /// </summary>
    public string NotificationUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de redirección en caso de éxito
    /// </summary>
    public string UrlOK { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de redirección en caso de error
    /// </summary>
    public string UrlKO { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del endpoint REST de Redsys para InSite
    /// </summary>
    public string InSiteUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del endpoint REST de Redsys
    /// </summary>
    public string RestUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si está en modo test
    /// </summary>
    public bool IsTestEnvironment => Environment?.ToLower() == "test";
    
    /// <summary>
    /// Obtiene la URL base según el entorno
    /// </summary>
    public string GetBaseUrl()
    {
        return IsTestEnvironment
            ? "https://sis-t.redsys.es:25443"
            : "https://sis.redsys.es";
    }
    
    /// <summary>
    /// Obtiene la URL del endpoint REST según el entorno
    /// </summary>
    public string GetRestEndpoint()
    {
        if (!string.IsNullOrEmpty(RestUrl))
            return RestUrl;
            
        return IsTestEnvironment
            ? "https://sis-t.redsys.es:25443/sis/rest/trataPeticionREST"
            : "https://sis.redsys.es/sis/rest/trataPeticionREST";
    }
    
    /// <summary>
    /// Valida que la configuración esté completa
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(MerchantCode) &&
               !string.IsNullOrEmpty(Terminal) &&
               !string.IsNullOrEmpty(SecretKey);
    }
}
