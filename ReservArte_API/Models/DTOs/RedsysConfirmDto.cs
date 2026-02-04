/*
 * RedsysConfirmDto.cs
 * ===================
 * 
 * PROPÓSITO:
 * DTOs para confirmar (capturar) una pre-autorización con Redsys.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/{id}/confirm
 * - PaymentService.cs: ConfirmPaymentAsync()
 * - RedsysService.cs: ConfirmPreAuthorizationAsync()
 * 
 * PARA QUÉ SE USA:
 * - Capturar el importe pre-autorizado cuando el cliente asiste a la cita
 * - Permite capturar un importe menor al pre-autorizado (ej: penalización parcial)
 * - Transacción tipo "2" en Redsys
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para confirmar pre-autorización
/// </summary>
public class RedsysConfirmRequestDto
{
    /// <summary>
    /// Importe a capturar en euros (opcional)
    /// Si no se especifica, se captura el importe total pre-autorizado
    /// Puede ser menor al pre-autorizado para capturas parciales
    /// </summary>
    [Range(0.01, 999999.99, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Notas sobre la confirmación
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO de respuesta de confirmación
/// </summary>
public class RedsysConfirmResponseDto
{
    public int PaymentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal CapturedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AuthCode { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool Success { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
