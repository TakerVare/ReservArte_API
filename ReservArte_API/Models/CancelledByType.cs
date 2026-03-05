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
    
    /// <summary>
    /// Tipos permitidos al cancelar una cita: solo Customer (→ cancelled_by_customer) o Employee (→ cancelled_by_business).
    /// </summary>
    public static readonly string[] AllowedForCancellation = { Customer, Employee };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
    
    /// <summary>
    /// Comprueba si el tipo es válido para cancelar una cita (solo Customer o Employee).
    /// </summary>
    public static bool IsValidForCancellation(string? type)
    {
        return !string.IsNullOrEmpty(type) && AllowedForCancellation.Contains(type);
    }
}
