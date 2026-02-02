namespace ReservArte_API.Models;

/// <summary>
/// Tipos de consentimiento (RGPD).
/// 
/// Estos consentimientos permiten cumplir con el Reglamento General de Protección
/// de Datos y dar control al cliente sobre cómo se utilizan sus datos y cómo
/// prefiere ser contactado.
/// </summary>
public static class ConsentType
{
    /// <summary>
    /// Consentimiento para el tratamiento de datos personales.
    /// Obligatorio para poder prestar el servicio.
    /// </summary>
    public const string DataProcessing = "DataProcessing";
    
    /// <summary>
    /// Consentimiento para recibir comunicaciones comerciales y promociones.
    /// Incluye ofertas, descuentos, newsletters, etc.
    /// </summary>
    public const string Marketing = "Marketing";
    
    /// <summary>
    /// Consentimiento para tomar y publicar fotografías antes/después.
    /// Usado para portfolio, redes sociales o materiales promocionales.
    /// </summary>
    public const string Photos = "Photos";
    
    /// <summary>
    /// Consentimiento para guardar tarjetas de pago tokenizadas.
    /// Permite guardar métodos de pago para cargos futuros.
    /// </summary>
    public const string SavedCards = "SavedCards";
    
    /// <summary>
    /// Consentimiento para recibir notificaciones por SMS.
    /// Incluye recordatorios de citas, confirmaciones y cambios.
    /// </summary>
    public const string SmsNotifications = "SmsNotifications";
    
    /// <summary>
    /// Consentimiento para recibir notificaciones por WhatsApp.
    /// Incluye recordatorios de citas, confirmaciones y cambios.
    /// </summary>
    public const string WhatsAppNotifications = "WhatsAppNotifications";
    
    public static readonly string[] All = 
    { 
        DataProcessing, 
        Marketing, 
        Photos, 
        SavedCards,
        SmsNotifications,
        WhatsAppNotifications
    };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
}
