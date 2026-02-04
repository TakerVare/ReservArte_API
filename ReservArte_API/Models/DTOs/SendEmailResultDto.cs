namespace ReservArte_API.Models.DTOs;

/// <summary>
/// Resultado del envío de un email (para tracking en ReminderLog).
/// </summary>
public class SendEmailResultDto
{
    public bool Success { get; set; }
    /// <summary>ID del mensaje en el proveedor (ej. Amazon SES MessageId).</summary>
    public string? ExternalMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
