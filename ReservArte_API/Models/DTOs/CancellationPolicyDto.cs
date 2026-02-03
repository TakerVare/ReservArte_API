// =============================================================================
// CancellationPolicyDto.cs
// =============================================================================
// Este archivo contiene el DTO para configurar las políticas de cancelación.
//
// Uso principal:
// - Se utiliza en GET/PUT /api/cancellationpolicy/{orgId} para consultar y
//   configurar las políticas de cancelación de una organización.
//
// Parámetros configurables:
// - MinHoursBeforeCancel: Horas mínimas de anticipación para cancelar sin
//   penalización (ej: 24 horas antes de la cita)
// - PenaltyPercentage: Porcentaje del precio que se cobra si cancela tarde
//   (ej: 50% significa que se cobra la mitad del servicio)
// - MaxNoShowsBeforeBlock: Número de no-shows permitidos antes de bloquear
//   automáticamente al cliente (ej: 3 no-shows = cliente bloqueado)
//
// Configuración especial para clientes VIP:
// - VipMinHoursBeforeCancel: Tiempo de anticipación reducido para VIP
// - VipPenaltyPercentage: Penalización reducida o nula para VIP
//
// Si la política no está activa (IsActive=false), no se aplicarán penalizaciones.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para políticas de cancelación
/// </summary>
public class CancellationPolicyDto
{
    public int Id { get; set; }
    
    [Required]
    public int OrganizationId { get; set; }
    
    /// <summary>
    /// Horas mínimas de anticipación para cancelar sin penalización
    /// </summary>
    [Required]
    [Range(1, 168, ErrorMessage = "Las horas deben estar entre 1 y 168 (1 semana)")]
    public int MinHoursBeforeCancel { get; set; } = 24;
    
    /// <summary>
    /// Porcentaje de penalización si cancela tarde (0-100)
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
    public int PenaltyPercentage { get; set; } = 50;
    
    /// <summary>
    /// Número máximo de no-shows antes de bloquear al cliente
    /// </summary>
    [Required]
    [Range(1, 10, ErrorMessage = "El máximo de no-shows debe estar entre 1 y 10")]
    public int MaxNoShowsBeforeBlock { get; set; } = 3;
    
    /// <summary>
    /// Horas mínimas para clientes VIP (opcional)
    /// </summary>
    [Range(1, 168)]
    public int? VipMinHoursBeforeCancel { get; set; }
    
    /// <summary>
    /// Porcentaje de penalización para clientes VIP (opcional)
    /// </summary>
    [Range(0, 100)]
    public int? VipPenaltyPercentage { get; set; }
    
    public bool IsActive { get; set; } = true;
}
