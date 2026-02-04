using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para subida de fotografías de servicios.
/// </summary>
public class ServicePhotoDtoIn
{
    /// <summary>
    /// Tipo de foto: Before o After.
    /// </summary>
    [Required(ErrorMessage = "El tipo de foto es obligatorio")]
    public string Type { get; set; } = PhotoType.Before;
    
    /// <summary>
    /// Indica si la foto puede ser compartida públicamente.
    /// Requiere consentimiento del cliente (ConsentType.Photos).
    /// </summary>
    public bool IsPublic { get; set; } = false;
}

/// <summary>
/// DTO para actualizar la visibilidad de una foto.
/// </summary>
public class ServicePhotoUpdateDto
{
    /// <summary>
    /// Indica si la foto puede ser compartida públicamente.
    /// </summary>
    public bool IsPublic { get; set; }
}
