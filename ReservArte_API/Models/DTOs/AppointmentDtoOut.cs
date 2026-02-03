// =============================================================================
// AppointmentDtoOut.cs
// =============================================================================
// Este archivo contiene los DTOs de salida para mostrar información de citas.
//
// Uso principal:
// - AppointmentDtoOut: Se utiliza como respuesta en endpoints que devuelven
//   información detallada de una cita (GET /api/appointment/{id}, POST, PUT).
//   Incluye todos los datos de la cita, información del cliente y empleado,
//   detalles de pago, información de cancelación y los servicios incluidos.
// - AppointmentServiceItemDtoOut: Representa cada servicio dentro de la cita
//   con su nombre, variación, precio y duración.
//
// Este DTO proporciona una vista completa de la cita para:
// - Mostrar detalles en la interfaz de usuario
// - Confirmar la creación/modificación de citas
// - Generar recibos o confirmaciones
// =============================================================================

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información completa de la cita
/// </summary>
public class AppointmentDtoOut
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    public string AppointmentDate { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal DepositAmount { get; set; }
    
    public string? RedsysOrderNumber { get; set; }
    public int? PaymentMethodId { get; set; }
    public string? PaymentMethodLast4 { get; set; }
    
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledById { get; set; }
    public string? CancelledByType { get; set; }
    public string? CancelledByName { get; set; }
    
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Servicios incluidos en la cita
    /// </summary>
    public List<AppointmentServiceItemDtoOut> Services { get; set; } = new();
}

/// <summary>
/// DTO para los servicios dentro de una cita (salida)
/// </summary>
public class AppointmentServiceItemDtoOut
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int? ServiceVariationId { get; set; }
    public string? VariationName { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public int Order { get; set; }
}
