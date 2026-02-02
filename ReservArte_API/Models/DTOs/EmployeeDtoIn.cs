using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO for creating or updating an employee.
/// </summary>
public class EmployeeDtoIn
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? Phone { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime? HireDate { get; set; }

    public bool IsActive { get; set; } = true;
}
