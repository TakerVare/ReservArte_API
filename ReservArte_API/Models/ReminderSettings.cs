namespace ReservArte_API.Models;

/// <summary>
/// Configuración de recordatorios (URLs base, negocio). Cargar desde appsettings "Reminder".
/// </summary>
public class ReminderSettings
{
    public const string SectionName = "Reminder";

    /// <summary>URL base de la API/app (ej. https://api.tudominio.com) para generar links de confirmar/cancelar.</summary>
    public string BaseUrl { get; set; } = "https://localhost:7000";

    /// <summary>Nombre del negocio (para plantillas).</summary>
    public string BusinessName { get; set; } = "ReservArte";

    /// <summary>Dirección del local (para plantillas e instrucciones).</summary>
    public string? BusinessAddress { get; set; }
}
