/*
 * PaymentListDto.cs
 * =================
 * 
 * PROPÓSITO:
 * DTO ligero para listados de pagos, optimizado para rendimiento.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: GET /api/payment, GET /api/payment/customer/{customerId}
 * - PaymentService.cs: GetAllAsync(), GetByCustomerIdAsync()
 * - PaymentRepository.cs: GetAllAsync(), GetByCustomerIdAsync()
 * 
 * PARA QUÉ SE USA:
 * - Mostrar lista de pagos en tablas/grids
 * - Historial de pagos de un cliente
 * - Búsqueda y filtrado de pagos
 * - Contiene solo campos esenciales para mejor rendimiento en listados
 */

namespace ReservArte_API.Models.DTOs;

public class PaymentListDto
{
    public int Id { get; set; }
    
    // Relaciones (solo IDs y nombres para listado)
    public int? AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    // Datos esenciales del pago
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string PaymentMethodType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Referencia Redsys (para identificación rápida)
    public string? RedsysOrderNumber { get; set; }
    
    // Timestamps
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Helpers para UI
    public string PaymentMethodDescription => PaymentMethodType switch
    {
        Models.PaymentMethod.Card => "Tarjeta",
        Models.PaymentMethod.Cash => "Efectivo",
        Models.PaymentMethod.Transfer => "Transferencia",
        Models.PaymentMethod.Bizum => "Bizum",
        Models.PaymentMethod.TPV => "TPV físico",
        _ => PaymentMethodType
    };
    
    public string StatusDescription => Status switch
    {
        PaymentStatus.Pending => "Pendiente",
        PaymentStatus.Authorized => "Pre-autorizado",
        PaymentStatus.Captured => "Cobrado",
        PaymentStatus.Failed => "Fallido",
        PaymentStatus.Refunded => "Reembolsado",
        PaymentStatus.PartiallyRefunded => "Reembolso parcial",
        PaymentStatus.Cancelled => "Cancelado",
        _ => Status
    };
    
    /// <summary>
    /// Color sugerido para mostrar el estado en UI
    /// </summary>
    public string StatusColor => Status switch
    {
        PaymentStatus.Pending => "#FFA500",      // Orange
        PaymentStatus.Authorized => "#2196F3",   // Blue
        PaymentStatus.Captured => "#4CAF50",     // Green
        PaymentStatus.Failed => "#F44336",       // Red
        PaymentStatus.Refunded => "#9E9E9E",     // Gray
        PaymentStatus.PartiallyRefunded => "#FF9800", // Deep Orange
        PaymentStatus.Cancelled => "#757575",    // Dark Gray
        _ => "#757575"
    };
}
