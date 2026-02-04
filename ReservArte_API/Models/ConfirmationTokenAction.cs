namespace ReservArte_API.Models;

/// <summary>
/// Acción asociada a un token de confirmación/cancelación one-click.
/// </summary>
public static class ConfirmationTokenAction
{
    public const string Confirm = "confirm";
    public const string Cancel = "cancel";

    public static readonly string[] All = { Confirm, Cancel };

    public static bool IsValid(string? action)
    {
        return !string.IsNullOrEmpty(action) && All.Contains(action);
    }
}
