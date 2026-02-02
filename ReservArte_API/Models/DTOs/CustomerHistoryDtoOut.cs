namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con el historial completo de un cliente.
/// 
/// FINALIDAD:
/// Este DTO proporciona una vista completa del historial del cliente, incluyendo
/// estadísticas resumidas y el detalle de todas sus citas y pagos. Permite al
/// personal conocer el comportamiento del cliente y su valor para el negocio.
/// 
/// USO:
/// - GET /api/customer/{id}/history → Obtener historial completo
/// 
/// SECCIONES INCLUIDAS:
/// 
/// 1. RESUMEN ESTADÍSTICO:
///    - TotalAppointments: Total de citas registradas
///    - CompletedAppointments: Citas completadas satisfactoriamente
///    - CancelledAppointments: Citas canceladas por el cliente
///    - NoShows: Citas a las que no se presentó
///    - TotalSpent: Importe total gastado en servicios completados
/// 
/// 2. DETALLE DE CITAS (Appointments):
///    - Lista cronológica de todas las citas con fecha, hora, servicio,
///      empleado que atendió, estado y precio
/// 
/// 3. DETALLE DE PAGOS (Payments):
///    - Lista de todos los pagos realizados con fecha, importe,
///      método de pago y estado
/// 
/// UTILIDADES:
/// - Evaluar si el cliente merece categoría VIP
/// - Identificar patrones de no-shows para posible bloqueo
/// - Calcular valor del cliente (Customer Lifetime Value)
/// </summary>
public class CustomerHistoryDtoOut
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    // Resumen
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShows { get; set; }
    public decimal TotalSpent { get; set; }
    
    // Detalle de citas
    public List<AppointmentHistoryItem> Appointments { get; set; } = new();
    
    // Pagos realizados
    public List<PaymentHistoryItem> Payments { get; set; } = new();
}

/// <summary>
/// Item del historial de citas
/// </summary>
public class AppointmentHistoryItem
{
    public int AppointmentId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>
/// Item del historial de pagos
/// </summary>
public class PaymentHistoryItem
{
    public int PaymentId { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
