// =============================================================================
// AppointmentRescheduleDto.cs
// =============================================================================
// Este archivo contiene el DTO para reagendar citas existentes.
//
// Uso principal:
// - Se utiliza en el endpoint PUT /api/appointment/{id}/reschedule para
//   cambiar la fecha, hora o empleado de una cita existente.
//
// Características:
// - Permite cambiar a una nueva fecha y hora
// - Opcionalmente permite cambiar de empleado
// - Mantiene los servicios y precios originales de la cita
// - Registra quién realizó el reagendamiento y la razón
//
// Validaciones automáticas:
// - Verifica disponibilidad del empleado en el nuevo horario
// - Comprueba que no haya solapamiento con otras citas
// - No permite reagendar citas en estados finales (completada, cancelada, no-show)
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para reagendar una cita
/// </summary>
public class AppointmentRescheduleDto
{
    [Required]
    public DateOnly NewDate { get; set; }
    
    [Required]
    public TimeOnly NewStartTime { get; set; }
    
    /// <summary>
    /// Nuevo empleado (opcional, si no se especifica mantiene el actual)
    /// </summary>
    public int? NewEmployeeId { get; set; }
    
    /// <summary>
    /// Razón del reagendamiento
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// ID de quien reagenda
    /// </summary>
    [Required]
    public int RescheduledById { get; set; }
    
    /// <summary>
    /// Tipo de quien reagenda: Customer, Employee
    /// </summary>
    [Required]
    public string RescheduledByType { get; set; } = string.Empty;
}
