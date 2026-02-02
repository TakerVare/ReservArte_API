namespace ReservArte_API.Models;

/// <summary>
/// Niveles de severidad para alergias
/// </summary>
public static class AllergySeverity
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    
    public static readonly string[] All = { Low, Medium, High };
    
    public static bool IsValid(string? severity)
    {
        return !string.IsNullOrEmpty(severity) && All.Contains(severity);
    }
}
