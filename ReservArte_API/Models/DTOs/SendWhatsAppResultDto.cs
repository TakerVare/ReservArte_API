namespace ReservArte_API.Models.DTOs;

/// <summary>
/// Resultado del envío de un mensaje WhatsApp (para tracking en ReminderLog).
/// </summary>
public class SendWhatsAppResultDto
{
    public bool Success { get; set; }
    /// <summary>ID del mensaje en WhatsApp Business API.</summary>
    public string? ExternalMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
