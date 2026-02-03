// =============================================================================
// WaitingListDto.cs
// =============================================================================
// Este archivo contiene los DTOs para gestionar la lista de espera de clientes.
//
// Uso principal:
// - WaitingListDtoIn: Se utiliza en POST /api/waitinglist para añadir un cliente
//   a la lista de espera cuando no hay disponibilidad inmediata.
//
// - WaitingListDtoOut: Se devuelve en las consultas de lista de espera,
//   incluyendo información del cliente, servicio y estado de notificación.
//
// Funcionamiento de la lista de espera:
// - Los clientes especifican un servicio y rango de fechas aceptables
// - Opcionalmente pueden indicar un empleado preferido o fecha específica
// - La prioridad se calcula automáticamente según:
//   * Categoría del cliente (VIP tiene mayor prioridad)
//   * Número de servicios previos contratados
//   * Orden de registro (FIFO dentro de la misma prioridad)
// - Cuando se libera un slot, se notifica automáticamente a los clientes
//   que coincidan con el servicio, fecha y empleado
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para añadir a lista de espera
/// </summary>
public class WaitingListDtoIn
{
    [Required]
    public int OrganizationId { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int ServiceId { get; set; }
    
    /// <summary>
    /// Empleado preferido (opcional)
    /// </summary>
    public int? PreferredEmployeeId { get; set; }
    
    /// <summary>
    /// Fecha preferida específica (opcional)
    /// </summary>
    public DateTime? PreferredDate { get; set; }
    
    /// <summary>
    /// Inicio del rango de fechas aceptables
    /// </summary>
    [Required]
    public DateTime DateRangeStart { get; set; }
    
    /// <summary>
    /// Fin del rango de fechas aceptables
    /// </summary>
    [Required]
    public DateTime DateRangeEnd { get; set; }
}

/// <summary>
/// DTO de salida para lista de espera
/// </summary>
public class WaitingListDtoOut
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCategory { get; set; }
    
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    
    public int? PreferredEmployeeId { get; set; }
    public string? PreferredEmployeeName { get; set; }
    
    public DateTime? PreferredDate { get; set; }
    public DateTime DateRangeStart { get; set; }
    public DateTime DateRangeEnd { get; set; }
    
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
    
    /// <summary>
    /// Indica si ya fue notificado
    /// </summary>
    public bool WasNotified => NotifiedAt.HasValue;
}
