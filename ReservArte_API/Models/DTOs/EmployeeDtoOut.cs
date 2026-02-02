namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO for returning employee information.
/// </summary>
public class EmployeeDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? HireDate { get; set; } // Formatted as yyyy-MM-dd
    public bool IsActive { get; set; }
}
