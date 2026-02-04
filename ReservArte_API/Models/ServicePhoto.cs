namespace ReservArte_API.Models;

/// <summary>
/// Entidad para almacenar fotografías de servicios (antes/después).
/// Las fotos se almacenan en Amazon S3 y tienen expiración automática según RGPD.
/// </summary>
public class ServicePhoto
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID de la cita asociada a esta fotografía.
    /// </summary>
    public int AppointmentId { get; set; }
    
    /// <summary>
    /// Tipo de foto: Before (antes del servicio) o After (después del servicio).
    /// </summary>
    public string Type { get; set; } = PhotoType.Before;
    
    /// <summary>
    /// Clave del objeto en Amazon S3.
    /// Formato: {customerId}/{appointmentId}/{type}_{timestamp}.jpg
    /// </summary>
    public string S3Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del bucket de S3 donde se almacena la foto.
    /// </summary>
    public string S3Bucket { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del empleado que subió la fotografía.
    /// </summary>
    public int UploadedBy { get; set; }
    
    /// <summary>
    /// Fecha y hora de subida de la fotografía.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si la foto puede ser compartida públicamente (con consentimiento del cliente).
    /// </summary>
    public bool IsPublic { get; set; } = false;
    
    /// <summary>
    /// Fecha de expiración automática según RGPD (por defecto 2 años desde la subida).
    /// Después de esta fecha, la foto será eliminada automáticamente.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    // Propiedades de navegación para DTOs
    public string? EmployeeName { get; set; }
    public string? CustomerName { get; set; }
}
