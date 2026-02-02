namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información completa de un cliente.
/// 
/// FINALIDAD:
/// Este DTO se utiliza para devolver todos los datos de un cliente específico,
/// incluyendo su información personal, estado de cuenta, categoría, puntos de
/// fidelización y configuración de preferencias.
/// 
/// USO:
/// - GET /api/customer/{id} → Obtener detalle completo de un cliente
/// - POST /api/customer → Respuesta tras crear un cliente
/// - PUT /api/customer/{id} → Respuesta tras actualizar un cliente
/// 
/// NOTA:
/// - BirthDate se devuelve en formato yyyy-MM-dd
/// - CreatedAt se devuelve en formato ISO 8601 (yyyy-MM-ddTHH:mm:ssZ)
/// - No incluye información sensible como tokens de pago
/// </summary>
public class CustomerDtoOut
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? BirthDate { get; set; } // Formato yyyy-MM-dd
    public int OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string Category { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public string PreferredContactMethod { get; set; } = string.Empty;
    public bool MarketingConsent { get; set; }
    public string CreatedAt { get; set; } = string.Empty; // Formato ISO 8601
}
