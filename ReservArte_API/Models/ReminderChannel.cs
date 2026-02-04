namespace ReservArte_API.Models;

/// <summary>
/// Canal de envío del recordatorio (Email, WhatsApp o ambos).
/// </summary>
public static class ReminderChannel
{
    public const string Email = "Email";
    public const string WhatsApp = "WhatsApp";
    public const string Both = "Both";

    public static readonly string[] All = { Email, WhatsApp, Both };

    public static bool IsValid(string? channel)
    {
        return !string.IsNullOrEmpty(channel) && All.Contains(channel);
    }
}
