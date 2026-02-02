namespace ReservArte_API.Models;

/// <summary>
/// Categorías de clientes para el sistema de categorización
/// </summary>
public static class CustomerCategory
{
    public const string New = "New";
    public const string Regular = "Regular";
    public const string VIP = "VIP";
    public const string Blocked = "Blocked";
    
    public static readonly string[] All = { New, Regular, VIP, Blocked };
    
    public static bool IsValid(string? category)
    {
        return !string.IsNullOrEmpty(category) && All.Contains(category);
    }
}
