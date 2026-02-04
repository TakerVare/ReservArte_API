/*
 * RedsysCancelDto.cs
 * ==================
 * 
 * PROPÓSITO:
 * DTOs para cancelar una pre-autorización con Redsys.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/{id}/cancel-preauth
 * - PaymentService.cs: CancelPreAuthorizationAsync()
 * - RedsysService.cs: CancelPreAuthorizationAsync()
 * 
 * PARA QUÉ SE USA:
 * - Liberar el importe bloqueado cuando el cliente cancela con antelación
 * - Cancelar pre-autorización si el cliente paga en efectivo
 * - Transacción tipo "9" en Redsys
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para cancelar pre-autorización
/// </summary>
public class RedsysCancelRequestDto
{
    /// <summary>
    /// Motivo de la cancelación
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// DTO de respuesta de cancelación
/// </summary>
public class RedsysCancelResponseDto
{
    public int PaymentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public bool Success { get; set; }
    public DateTime? CancelledAt { get; set; }
}
