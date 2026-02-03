namespace ReservArte_API.Models;

/// <summary>
/// Tipos de entidad que pueden cancelar una cita
/// </summary>
public static class CancelledByType
{
    public const string Customer = "Customer";
    public const string Employee = "Employee";
    public const string System = "System";
    
    public static readonly string[] All = { Customer, Employee, System };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
}
