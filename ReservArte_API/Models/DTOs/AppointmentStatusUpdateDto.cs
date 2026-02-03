// =============================================================================
// AppointmentStatusUpdateDto.cs
// =============================================================================
// Este archivo contiene el DTO para actualizar el estado de una cita.
//
// Uso principal:
// - Se utiliza en el endpoint PUT /api/appointment/{id}/status para cambiar
//   el estado de una cita existente.
//
// Estados permitidos y transiciones válidas:
// - pending → confirmed (confirmar cita)
// - pending → cancelled_by_customer/cancelled_by_business (cancelar)
// - confirmed → in_progress (iniciar servicio)
// - confirmed → cancelled_by_customer/cancelled_by_business (cancelar)
// - confirmed → no_show (cliente no se presenta)
// - in_progress → completed (finalizar servicio)
//
// El campo Notes permite añadir observaciones sobre el cambio de estado.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para actualizar el estado de una cita
/// </summary>
public class AppointmentStatusUpdateDto
{
    /// <summary>
    /// Nuevo estado: pending, confirmed, in_progress, completed
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Notas opcionales sobre el cambio de estado
    /// </summary>
    public string? Notes { get; set; }
}
