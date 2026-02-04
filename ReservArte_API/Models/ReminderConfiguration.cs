namespace ReservArte_API.Models;

/// <summary>
/// Configuración de un recordatorio (orden, horas antes, canal, plantilla, horario de envío).
/// Proyecto single-tenant: no se usa OrganizationId.
/// </summary>
public class ReminderConfiguration
{
    public Guid Id { get; set; }
    /// <summary>Orden del recordatorio (1º, 2º, 3º).</summary>
    public int ReminderOrder { get; set; }
    /// <summary>Horas antes de la cita para enviar.</summary>
    public int HoursBeforeAppointment { get; set; }
    /// <summary>Canal: Email, WhatsApp o Both.</summary>
    public string Channel { get; set; } = ReminderChannel.Email;
    public bool IsActive { get; set; } = true;
    public Guid MessageTemplateId { get; set; }
    /// <summary>Hora mínima del día para enviar (ej. 08:00). No enviar antes.</summary>
    public TimeOnly? AllowedSendStartTime { get; set; }
    /// <summary>Hora máxima del día para enviar (ej. 22:00). No enviar después.</summary>
    public TimeOnly? AllowedSendEndTime { get; set; }
}
