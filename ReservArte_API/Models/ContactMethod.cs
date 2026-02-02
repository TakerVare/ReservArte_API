namespace ReservArte_API.Models;

/// <summary>
/// Métodos de contacto preferidos por el cliente
/// </summary>
public static class ContactMethod
{
    public const string Email = "Email";
    public const string WhatsApp = "WhatsApp";
    public const string SMS = "SMS";
    
    public static readonly string[] All = { Email, WhatsApp, SMS };
    
    public static bool IsValid(string? method)
    {
        return !string.IsNullOrEmpty(method) && All.Contains(method);
    }
}
