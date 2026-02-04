namespace ReservArte_API.Models;

/// <summary>
/// Estado del envío de un recordatorio (tracking).
/// </summary>
public static class ReminderLogStatus
{
    public const string Sent = "Sent";
    public const string Delivered = "Delivered";
    public const string Failed = "Failed";
    public const string Opened = "Opened";
    public const string Clicked = "Clicked";
    public const string Bounced = "Bounced";

    public static readonly string[] All = { Sent, Delivered, Failed, Opened, Clicked, Bounced };

    public static bool IsValid(string? status)
    {
        return !string.IsNullOrEmpty(status) && All.Contains(status);
    }
}
