namespace ReservArte_API.Models;

/// <summary>
/// Plantilla de mensaje para recordatorios (email/WhatsApp) o confirmación.
/// Variables soportadas: {{customerName}}, {{appointmentDate}}, {{appointmentTime}},
/// {{serviceName}}, {{employeeName}}, {{businessAddress}}, {{confirmUrl}}, {{cancelUrl}}.
/// Proyecto single-tenant: no se usa OrganizationId.
/// </summary>
public class MessageTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    /// <summary>EmailReminder | WhatsAppReminder | Confirmation</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Asunto del email (solo para tipo EmailReminder).</summary>
    public string? Subject { get; set; }
    /// <summary>Cuerpo con variables {{variable}}.</summary>
    public string Body { get; set; } = string.Empty;
    /// <summary>Idioma, ej. es-ES.</summary>
    public string Language { get; set; } = "es-ES";
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
}
