using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO for service information.
/// </summary>
public class ServiceDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Range(0, 10000, ErrorMessage = "El precio debe estar entre 0 y 10000")]
    public decimal Price { get; set; }

    [Range(5, 480, ErrorMessage = "La duración debe estar entre 5 y 480 minutos")]
    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}
