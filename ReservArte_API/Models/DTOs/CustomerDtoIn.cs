using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para crear o actualizar un cliente.
/// 
/// FINALIDAD:
/// Este DTO se utiliza como cuerpo de las peticiones POST y PUT en los endpoints
/// de gestión de clientes. Contiene los datos básicos del perfil que el cliente
/// o el personal pueden proporcionar al registrarse o modificar su información.
/// 
/// USO:
/// - POST /api/customer → Crear nuevo cliente
/// - PUT /api/customer/{id} → Actualizar cliente existente
/// 
/// VALIDACIONES:
/// - FirstName y LastName son obligatorios (2-100 caracteres)
/// - Email es obligatorio y debe tener formato válido
/// - Phone es opcional pero si se proporciona debe tener formato válido
/// </summary>
public class CustomerDtoIn
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? Phone { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime? BirthDate { get; set; }

    public string PreferredContactMethod { get; set; } = ContactMethod.Email;

    public bool MarketingConsent { get; set; } = false;
}
