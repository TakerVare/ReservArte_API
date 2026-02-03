// =============================================================================
// AppointmentDtoIn.cs
// =============================================================================
// Este archivo contiene los DTOs de entrada para la creación de citas.
//
// Uso principal:
// - AppointmentDtoIn: Se utiliza en el endpoint POST /api/appointment para crear
//   una nueva cita. Incluye la información del cliente, empleado, fecha/hora y
//   los servicios que se van a realizar.
// - AppointmentServiceItemDtoIn: Define cada servicio individual dentro de una
//   cita, permitiendo especificar variaciones del servicio.
//
// El sistema calculará automáticamente:
// - La hora de fin basándose en la duración de los servicios
// - El precio total sumando los precios de cada servicio
// - La validación de disponibilidad del empleado
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para crear una nueva cita
/// </summary>
public class AppointmentDtoIn
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateOnly AppointmentDate { get; set; }
    
    [Required]
    public TimeOnly StartTime { get; set; }
    
    /// <summary>
    /// Lista de servicios a incluir en la cita
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un servicio")]
    public List<AppointmentServiceItemDtoIn> Services { get; set; } = new();
    
    /// <summary>
    /// ID del método de pago guardado (nullable si paga en el momento)
    /// </summary>
    public int? PaymentMethodId { get; set; }
    
    /// <summary>
    /// Notas adicionales para la cita
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para los servicios dentro de una cita (entrada)
/// </summary>
public class AppointmentServiceItemDtoIn
{
    [Required]
    public int ServiceId { get; set; }
    
    /// <summary>
    /// Variación del servicio (nullable si es el servicio base)
    /// </summary>
    public int? ServiceVariationId { get; set; }
}
