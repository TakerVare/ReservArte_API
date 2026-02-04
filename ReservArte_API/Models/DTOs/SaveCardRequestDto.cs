/*
 * SaveCardRequestDto.cs
 * =====================
 * 
 * PROPÓSITO:
 * DTO para guardar una nueva tarjeta tokenizada desde una transacción exitosa.
 * 
 * DÓNDE SE USA:
 * - PaymentService.cs: SaveCardFromTransactionAsync()
 * - CustomerPaymentMethodRepository.cs: CreateAsync()
 * 
 * PARA QUÉ SE USA:
 * - Guardar tarjeta tras pre-autorización exitosa cuando el cliente lo solicita
 * - Almacenar token de Redsys (DS_MERCHANT_IDENTIFIER) para pagos futuros
 * - El token se obtiene de la respuesta de Redsys cuando se incluye COF parameters
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

public class SaveCardRequestDto
{
    /// <summary>
    /// ID del cliente propietario de la tarjeta
    /// </summary>
    [Required]
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Token de Redsys (DS_MERCHANT_IDENTIFIER)
    /// </summary>
    [Required]
    public string RedsysToken { get; set; } = string.Empty;
    
    /// <summary>
    /// ID de transacción COF (DS_MERCHANT_COF_TXNID)
    /// </summary>
    public string? CofTxnId { get; set; }
    
    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string CardLast4 { get; set; } = string.Empty;
    
    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, etc.)
    /// </summary>
    [Required]
    public string CardBrand { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de expiración en formato AAMM
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string CardExpiry { get; set; } = string.Empty;
    
    /// <summary>
    /// Si debe ser el método de pago por defecto
    /// </summary>
    public bool SetAsDefault { get; set; } = false;
}
