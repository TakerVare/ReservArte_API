using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO for the relationship between an employee and a service.
/// </summary>
public class EmployeeServiceDto
{
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "El ID del servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Range(1, 5, ErrorMessage = "El nivel de competencia debe estar entre 1 y 5")]
    public int ProficiencyLevel { get; set; } = 3;

    // Additional info for display purposes
    public string? ServiceName { get; set; }
    public decimal? ServicePrice { get; set; }
    public int? ServiceDurationMinutes { get; set; }
}
