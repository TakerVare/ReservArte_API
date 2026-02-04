namespace ReservArte_API.Models;

/// <summary>
/// Tipo de plantilla de mensaje para recordatorios y confirmaciones.
/// </summary>
public static class MessageTemplateType
{
    public const string EmailReminder = "EmailReminder";
    public const string WhatsAppReminder = "WhatsAppReminder";
    public const string Confirmation = "Confirmation";

    public static readonly string[] All = { EmailReminder, WhatsAppReminder, Confirmation };

    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
}
