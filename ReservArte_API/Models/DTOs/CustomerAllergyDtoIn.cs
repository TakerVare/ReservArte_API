using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para registrar o actualizar una alergia/condición médica de un cliente.
/// 
/// FINALIDAD:
/// Este DTO permite registrar información médica relevante del cliente, como alergias
/// a productos cosméticos, látex, tintes, etc. Esta información es crítica para
/// garantizar la seguridad durante los servicios y evitar reacciones adversas.
/// 
/// USO:
/// - POST /api/customer/{id}/allergies → Registrar nueva alergia
/// - PUT /api/customer/allergies/{allergyId} → Actualizar alergia existente
/// 
/// VALIDACIONES:
/// - AllergyDescription es obligatorio (2-500 caracteres)
/// - Severity es obligatorio (valores válidos: Low, Medium, High)
/// 
/// NIVELES DE SEVERIDAD:
/// - Low: Sensibilidad leve, precaución recomendada
/// - Medium: Reacción moderada, evitar producto/sustancia
/// - High: Reacción severa, contraindicación absoluta
/// </summary>
public class CustomerAllergyDtoIn
{
    [Required(ErrorMessage = "La descripción de la alergia es obligatoria")]
    [StringLength(500, MinimumLength = 2, ErrorMessage = "La descripción debe tener entre 2 y 500 caracteres")]
    public string AllergyDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "La severidad es obligatoria")]
    public string Severity { get; set; } = AllergySeverity.Low;
}
