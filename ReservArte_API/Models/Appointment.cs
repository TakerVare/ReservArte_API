namespace ReservArte_API.Models;

/// <summary>
/// Entidad principal de cita/reserva
/// </summary>
public class Appointment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    /// <summary>
    /// Estado de la cita: pending, confirmed, in_progress, completed, cancelled_by_customer, cancelled_by_business, no_show
    /// </summary>
    public string Status { get; set; } = Models.Status.Pending;
    
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    
    // Campos de pago Redsys
    public string? RedsysOrderNumber { get; set; }
    public string? RedsysPreAuthToken { get; set; }
    
    /// <summary>
    /// ID del método de pago guardado del cliente (nullable si paga en el momento)
    /// </summary>
    public int? PaymentMethodId { get; set; }
    
    // Campos de cancelación
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledById { get; set; }
    
    /// <summary>
    /// Tipo de quien canceló: Customer, Employee, System
    /// </summary>
    public string? CancelledByType { get; set; }
    
    /// <summary>
    /// Notas internas sobre la cita
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Propiedades de navegación para DTOs
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
    public List<AppointmentServiceItem>? Services { get; set; }
}   