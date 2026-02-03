namespace ReservArte_API.Models;

/// <summary>
/// Lista de espera para clientes que desean un servicio cuando no hay disponibilidad
/// </summary>
public class WaitingList
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ServiceId { get; set; }
    
    /// <summary>
    /// Empleado preferido (nullable si no tiene preferencia)
    /// </summary>
    public int? PreferredEmployeeId { get; set; }
    
    /// <summary>
    /// Fecha preferida específica (nullable si acepta cualquier fecha en el rango)
    /// </summary>
    public DateTime? PreferredDate { get; set; }
    
    /// <summary>
    /// Inicio del rango de fechas aceptables
    /// </summary>
    public DateTime DateRangeStart { get; set; }
    
    /// <summary>
    /// Fin del rango de fechas aceptables
    /// </summary>
    public DateTime DateRangeEnd { get; set; }
    
    /// <summary>
    /// Prioridad en la lista (menor número = mayor prioridad)
    /// Se calcula según: orden de registro, categoría VIP, número de servicios contratados
    /// </summary>
    public int Priority { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha en que se notificó al cliente de disponibilidad
    /// </summary>
    public DateTime? NotifiedAt { get; set; }
    
    // Propiedades de navegación para DTOs
    public string? CustomerName { get; set; }
    public string? ServiceName { get; set; }
    public string? PreferredEmployeeName { get; set; }
}
