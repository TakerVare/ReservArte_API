using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para gestionar consentimientos del cliente (RGPD).
/// 
/// FINALIDAD:
/// Este DTO permite registrar o actualizar los consentimientos del cliente
/// conforme al Reglamento General de Protección de Datos (RGPD). Es obligatorio
/// obtener consentimiento explícito antes de ciertas operaciones.
/// 
/// USO:
/// - PUT /api/customer/{id}/consents → Otorgar o revocar un consentimiento
/// 
/// TIPOS DE CONSENTIMIENTO (ConsentType):
/// - DataProcessing: Tratamiento de datos personales (obligatorio para el servicio)
/// - Marketing: Recibir comunicaciones comerciales y promociones
/// - Photos: Permitir fotografías antes/después para portfolio
/// - SavedCards: Permitir guardar tarjetas de pago tokenizadas
/// 
/// VALIDACIONES:
/// - ConsentType es obligatorio y debe ser uno de los tipos válidos
/// - IsGranted indica si se otorga (true) o revoca (false) el consentimiento
/// 
/// NOTA LEGAL:
/// El sistema registra automáticamente la fecha de otorgamiento o revocación.
/// </summary>
public class CustomerConsentDtoIn
{
    [Required(ErrorMessage = "El tipo de consentimiento es obligatorio")]
    public string ConsentType { get; set; } = string.Empty;

    public bool IsGranted { get; set; } = false;
}
