/*
 * RedsysRefundDto.cs
 * ==================
 * 
 * PROPÓSITO:
 * DTOs para procesar reembolsos a través de Redsys.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/{id}/refund
 * - PaymentService.cs: ProcessRedsysRefundAsync()
 * - RedsysService.cs: ProcessRefundAsync()
 * 
 * PARA QUÉ SE USA:
 * - Devolver dinero al cliente tras un pago capturado
 * - Reembolsos parciales o totales
 * - Transacción tipo "3" en Redsys (devolución automática)
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para reembolso vía Redsys
/// </summary>
public class RedsysRefundRequestDto
{
    /// <summary>
    /// Importe a reembolsar en euros
    /// Debe ser menor o igual al importe pendiente de reembolso
    /// </summary>
    [Required(ErrorMessage = "El importe es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Motivo del reembolso
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// DTO de respuesta de reembolso
/// </summary>
public class RedsysRefundResponseDto
{
    public int PaymentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool Success { get; set; }
    public DateTime? RefundedAt { get; set; }
}
