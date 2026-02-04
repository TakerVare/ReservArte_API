namespace ReservArte_API.Models;

/// <summary>
/// Registro de envío de un recordatorio (tracking: enviado, entregado, abierto, etc.).
/// </summary>
public class ReminderLog
{
    public Guid Id { get; set; }
    public int AppointmentId { get; set; }
    public Guid ReminderConfigurationId { get; set; }
    /// <summary>Email o WhatsApp.</summary>
    public string Channel { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    /// <summary>Sent | Delivered | Failed | Opened | Clicked | Bounced</summary>
    public string Status { get; set; } = ReminderLogStatus.Sent;
    /// <summary>ID del mensaje en SES o WhatsApp.</summary>
    public string? ExternalMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
