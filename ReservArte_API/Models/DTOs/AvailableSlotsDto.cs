// =============================================================================
// AvailableSlotsDto.cs
// =============================================================================
// Este archivo contiene los DTOs para consultar y mostrar slots disponibles.
//
// Uso principal:
// - AvailableSlotsRequestDto: Se utiliza en GET /api/appointment/slots para
//   solicitar los horarios disponibles para un servicio en una fecha específica.
//
// - AvailableSlotsResponseDto: Respuesta con los slots disponibles agrupados
//   por empleado, incluyendo el nombre del servicio y duración total.
//
// - EmployeeSlotsDto: Agrupa los slots de un empleado específico.
//
// - TimeSlotDto: Representa un intervalo de tiempo disponible (inicio-fin).
//
// El sistema considera automáticamente:
// - Horario laboral de cada empleado (EmployeeAvailability)
// - Excepciones del empleado (vacaciones, días libres)
// - Citas ya reservadas (evita solapamientos)
// - Duración del servicio (incluyendo modificadores de variación)
//
// Los slots se generan cada 30 minutos dentro del horario disponible.
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para solicitar slots disponibles
/// </summary>
public class AvailableSlotsRequestDto
{
    /// <summary>
    /// Fecha para buscar disponibilidad
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }
    
    /// <summary>
    /// ID del servicio (para calcular duración necesaria)
    /// </summary>
    [Required]
    public int ServiceId { get; set; }
    
    /// <summary>
    /// Variación del servicio (opcional)
    /// </summary>
    public int? ServiceVariationId { get; set; }
    
    /// <summary>
    /// Empleado específico (opcional, si no se especifica busca en todos)
    /// </summary>
    public int? EmployeeId { get; set; }
}

/// <summary>
/// DTO de un slot disponible
/// </summary>
public class AvailableSlotDto
{
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Duración del slot en minutos
    /// </summary>
    public int DurationMinutes { get; set; }
}

/// <summary>
/// DTO de respuesta con slots agrupados por empleado
/// </summary>
public class AvailableSlotsResponseDto
{
    public DateOnly Date { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    
    /// <summary>
    /// Slots agrupados por empleado
    /// </summary>
    public List<EmployeeSlotsDto> EmployeeSlots { get; set; } = new();
    
    /// <summary>
    /// Total de slots disponibles
    /// </summary>
    public int TotalSlotsAvailable => EmployeeSlots.Sum(e => e.Slots.Count);
}

/// <summary>
/// Slots de un empleado específico
/// </summary>
public class EmployeeSlotsDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<TimeSlotDto> Slots { get; set; } = new();
}

/// <summary>
/// Slot de tiempo simple
/// </summary>
public class TimeSlotDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
