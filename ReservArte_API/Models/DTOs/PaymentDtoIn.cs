/*
 * PaymentDtoIn.cs
 * ===============
 * 
 * PROPÓSITO:
 * DTO de entrada para registrar pagos manuales en el sistema.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: POST /api/payment/manual
 * - PaymentService.cs: CreateManualPaymentAsync()
 * 
 * PARA QUÉ SE USA:
 * - Registrar pagos en efectivo realizados en el local
 * - Registrar pagos por transferencia bancaria
 * - Registrar pagos realizados con TPV físico
 * 
 * NOTA: Los pagos con tarjeta online (Redsys) usan RedsysPaymentDtoIn
 */

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

public class PaymentDtoIn
{
    /// <summary>
    /// ID de la cita asociada (opcional)
    /// </summary>
    public int? AppointmentId { get; set; }
    
    /// <summary>
    /// ID del cliente que realiza el pago (requerido)
    /// </summary>
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Importe del pago en euros (ej: 25.50)
    /// </summary>
    [Required(ErrorMessage = "El importe es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Método de pago: cash, transfer, tpv
    /// Los métodos card y bizum requieren flujo Redsys
    /// </summary>
    [Required(ErrorMessage = "El método de pago es obligatorio")]
    public string PaymentMethodType { get; set; } = PaymentMethod.Cash;
    
    /// <summary>
    /// Notas opcionales sobre el pago
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Referencia externa (número de transferencia, ticket TPV, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? ExternalReference { get; set; }
}
