namespace ReservArte_API.Models;

/// <summary>
/// Relación entre citas y los servicios contratados en cada cita
/// </summary>
public class AppointmentServiceItem
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int ServiceId { get; set; }
    
    /// <summary>
    /// Variación del servicio (nullable si es el servicio base)
    /// </summary>
    public int? ServiceVariationId { get; set; }
    
    /// <summary>
    /// Precio del servicio en el momento de la reserva
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Duración del servicio en minutos
    /// </summary>
    public int DurationMinutes { get; set; }
    
    /// <summary>
    /// Orden del servicio dentro de la cita (para servicios múltiples)
    /// </summary>
    public int Order { get; set; }
    
    // Propiedades de navegación para DTOs
    public string? ServiceName { get; set; }
    public string? VariationName { get; set; }
}
