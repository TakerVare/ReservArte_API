namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con el estado de los consentimientos RGPD del cliente.
/// 
/// FINALIDAD:
/// Este DTO muestra el estado actual de cada tipo de consentimiento del cliente,
/// incluyendo cuándo fue otorgado o revocado. Permite verificar qué operaciones
/// están autorizadas para cada cliente.
/// 
/// USO:
/// - GET /api/customer/{id}/consents → Obtener todos los consentimientos
/// - PUT /api/customer/{id}/consents → Respuesta tras actualizar un consentimiento
/// 
/// CAMPOS IMPORTANTES:
/// - IsGranted: Estado actual del consentimiento (true = otorgado)
/// - GrantedAt: Fecha/hora en que se otorgó (null si nunca se otorgó)
/// - RevokedAt: Fecha/hora en que se revocó (null si está vigente)
/// 
/// VERIFICACIÓN ANTES DE OPERACIONES:
/// - Guardar tarjeta → Verificar SavedCards = true
/// - Enviar promociones → Verificar Marketing = true
/// - Publicar fotos → Verificar Photos = true
/// </summary>
public class CustomerConsentDtoOut
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public string? GrantedAt { get; set; }
    public string? RevokedAt { get; set; }
}
