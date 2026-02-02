using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para registrar un método de pago (tarjeta tokenizada Redsys).
/// 
/// FINALIDAD:
/// Este DTO permite guardar una tarjeta de crédito/débito de forma segura mediante
/// tokenización de Redsys. NO se almacenan los datos completos de la tarjeta,
/// solo el token y datos de referencia para identificarla.
/// 
/// USO:
/// - POST /api/customer/{id}/payment-methods → Guardar nueva tarjeta
/// 
/// REQUISITOS PREVIOS:
/// - El cliente debe haber otorgado el consentimiento SavedCards = true
/// - El token debe obtenerse previamente mediante el proceso COF de Redsys
/// 
/// VALIDACIONES:
/// - RedsysToken: Token de Redsys (obligatorio)
/// - CardLast4: Últimos 4 dígitos de la tarjeta (obligatorio, exactamente 4 caracteres)
/// - CardBrand: Marca de la tarjeta - Visa, Mastercard, etc. (obligatorio)
/// - CardExpiry: Fecha de expiración en formato AAMM (obligatorio, 4 caracteres)
/// 
/// SEGURIDAD:
/// - El token de Redsys NO permite realizar cargos sin autorización adicional
/// - Los datos de tarjeta completos nunca se transmiten ni almacenan
/// </summary>
public class CustomerPaymentMethodDtoIn
{
    [Required(ErrorMessage = "El token de Redsys es obligatorio")]
    public string RedsysToken { get; set; } = string.Empty;

    public string? RedsysCofTxnid { get; set; }

    [Required(ErrorMessage = "Los últimos 4 dígitos de la tarjeta son obligatorios")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Debe tener exactamente 4 dígitos")]
    public string CardLast4 { get; set; } = string.Empty;

    [Required(ErrorMessage = "La marca de la tarjeta es obligatoria")]
    public string CardBrand { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de expiración es obligatoria")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Formato AAMM (4 dígitos)")]
    public string CardExpiry { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}
