/*
 * RedsysPreAuthDto.cs
 * ===================
 * 
 * PROPÓSITO:
 * DTOs para el proceso de pre-autorización con Redsys.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/preauth
 * - PaymentService.cs: CreatePreAuthorizationAsync()
 * - RedsysService.cs: CreatePreAuthorizationAsync()
 * 
 * PARA QUÉ SE USA:
 * - Iniciar pre-autorización al crear cita online
 * - Bloquear importe en tarjeta del cliente sin cobrar
 * - Flujo InSite: recibe idOper del frontend tras captura de tarjeta
 * - La pre-autorización es válida por 7 días
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para crear pre-autorización
/// </summary>
public class RedsysPreAuthRequestDto
{
    /// <summary>
    /// ID de la cita para la que se hace la pre-autorización
    /// </summary>
    [Required(ErrorMessage = "La cita es obligatoria")]
    public int AppointmentId { get; set; }
    
    /// <summary>
    /// ID del cliente
    /// </summary>
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Importe a pre-autorizar en euros (ej: 25.50)
    /// </summary>
    [Required(ErrorMessage = "El importe es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// idOper obtenido del SDK InSite de Redsys en el frontend
    /// Este identificador contiene los datos de tarjeta capturados de forma segura
    /// </summary>
    [Required(ErrorMessage = "El idOper es obligatorio")]
    public string IdOper { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del método de pago guardado (opcional, alternativa a idOper)
    /// Si se proporciona, se usa el token guardado en lugar de idOper
    /// </summary>
    public int? SavedPaymentMethodId { get; set; }
    
    /// <summary>
    /// Indica si se debe guardar la tarjeta para usos futuros
    /// </summary>
    public bool SaveCard { get; set; } = false;
}

/// <summary>
/// DTO de respuesta de pre-autorización
/// </summary>
public class RedsysPreAuthResponseDto
{
    public int PaymentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AuthCode { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool Success { get; set; }
    
    /// <summary>
    /// Token de tarjeta si se solicitó guardar (DS_MERCHANT_IDENTIFIER)
    /// </summary>
    public string? CardToken { get; set; }
    
    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string? CardLast4 { get; set; }
    
    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, etc.)
    /// </summary>
    public string? CardBrand { get; set; }
    
    /// <summary>
    /// Fecha de expiración de la pre-autorización
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
