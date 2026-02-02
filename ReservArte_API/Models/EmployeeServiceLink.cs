namespace ReservArte_API.Models;

/// <summary>
/// Represents the relationship between an employee and a service they can perform.
/// </summary>
public class EmployeeServiceLink
{
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }
    public int ProficiencyLevel { get; set; } // 1-5 scale indicating skill level
}
