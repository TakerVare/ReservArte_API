// =============================================================================
// AppointmentCancelDto.cs
// =============================================================================
// Este archivo contiene los DTOs relacionados con la cancelación de citas.
//
// Uso principal:
// - AppointmentCancelDto: Se utiliza en el endpoint PUT /api/appointment/{id}/cancel
//   para cancelar una cita. Permite especificar quién cancela, la razón y si
//   es una cancelación justificada (emergencia).
//
// - AppointmentCancelResultDto: Se devuelve como respuesta tras la cancelación,
//   incluyendo información sobre si se aplicó penalización y el monto.
//
// Lógica de penalización:
// - Se aplica según la política de cancelación de la organización
// - Depende del tiempo de anticipación (minHoursBeforeCancel)
// - Los clientes VIP pueden tener condiciones especiales
// - Las cancelaciones justificadas no aplican penalización
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para cancelar una cita
/// </summary>
public class AppointmentCancelDto
{
    /// <summary>
    /// Razón de la cancelación
    /// </summary>
    [Required]
    [MinLength(5, ErrorMessage = "La razón debe tener al menos 5 caracteres")]
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// ID de quien cancela (cliente o empleado)
    /// </summary>
    [Required]
    public int CancelledById { get; set; }
    
    /// <summary>
    /// Tipo de quien cancela: Customer, Employee
    /// </summary>
    [Required]
    public string CancelledByType { get; set; } = string.Empty;
    
    /// <summary>
    /// Si es una cancelación justificada (emergencia con evidencia)
    /// </summary>
    public bool IsJustified { get; set; } = false;
}

/// <summary>
/// DTO de respuesta con información de penalización
/// </summary>
public class AppointmentCancelResultDto
{
    public int AppointmentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
    
    /// <summary>
    /// Si se aplicó penalización
    /// </summary>
    public bool PenaltyApplied { get; set; }
    
    /// <summary>
    /// Monto de la penalización (si aplica)
    /// </summary>
    public decimal PenaltyAmount { get; set; }
    
    /// <summary>
    /// Porcentaje de penalización aplicado
    /// </summary>
    public int PenaltyPercentage { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
