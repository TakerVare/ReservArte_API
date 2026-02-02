namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información de una alergia/condición médica registrada.
/// 
/// FINALIDAD:
/// Este DTO se utiliza para devolver las alergias y condiciones médicas
/// registradas de un cliente. Esta información debe ser consultada por el
/// personal antes de realizar cualquier servicio.
/// 
/// USO:
/// - GET /api/customer/{id}/allergies → Obtener todas las alergias de un cliente
/// - POST /api/customer/{id}/allergies → Respuesta tras registrar una alergia
/// - PUT /api/customer/allergies/{id} → Respuesta tras actualizar una alergia
/// 
/// IMPORTANTE:
/// Esta información es sensible y debe tratarse conforme al RGPD.
/// Solo debe mostrarse al personal autorizado.
/// </summary>
public class CustomerAllergyDtoOut
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string AllergyDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
