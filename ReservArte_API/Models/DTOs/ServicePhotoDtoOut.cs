namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de respuesta para fotografías de servicios.
/// Incluye URL pre-firmada temporal para acceso a la imagen.
/// </summary>
public class ServicePhotoDtoOut
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    
    /// <summary>
    /// Tipo de foto: Before o After.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// URL pre-firmada temporal para acceder a la imagen.
    /// Expira según configuración (por defecto 60 minutos).
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// URL de la imagen con marca de agua (si está configurada).
    /// </summary>
    public string? WatermarkedUrl { get; set; }
    
    /// <summary>
    /// Nombre del empleado que subió la foto.
    /// </summary>
    public string? UploadedByName { get; set; }
    
    public DateTime UploadedAt { get; set; }
    public bool IsPublic { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// DTO para comparación lado a lado de fotos antes/después.
/// </summary>
public class PhotoComparisonDtoOut
{
    public int AppointmentId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime AppointmentDate { get; set; }
    
    /// <summary>
    /// Fotos tomadas antes del servicio.
    /// </summary>
    public List<ServicePhotoDtoOut> BeforePhotos { get; set; } = new();
    
    /// <summary>
    /// Fotos tomadas después del servicio.
    /// </summary>
    public List<ServicePhotoDtoOut> AfterPhotos { get; set; } = new();
}

/// <summary>
/// DTO para la galería de un cliente.
/// </summary>
public class CustomerGalleryDtoOut
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Comparaciones agrupadas por cita.
    /// </summary>
    public List<PhotoComparisonDtoOut> Appointments { get; set; } = new();
    
    /// <summary>
    /// Total de fotos en la galería.
    /// </summary>
    public int TotalPhotos { get; set; }
}

/// <summary>
/// DTO para compartir una foto públicamente.
/// </summary>
public class PhotoShareDtoOut
{
    public int PhotoId { get; set; }
    
    /// <summary>
    /// URL pública para compartir (válida por tiempo limitado).
    /// </summary>
    public string ShareUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de expiración de la URL compartida.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Indica si el cliente tiene consentimiento para compartir.
    /// </summary>
    public bool HasConsent { get; set; }
}
