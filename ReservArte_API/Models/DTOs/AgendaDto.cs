// =============================================================================
// AgendaDto.cs
// =============================================================================
// Este archivo contiene los DTOs para la visualización de la agenda de citas.
//
// Uso principal:
// - AgendaQueryDto: Parámetros de consulta para GET /api/appointment/agenda.
//   Permite filtrar por organización, rango de fechas, empleado y estado.
//
// - AgendaResponseDto: Respuesta completa de la agenda, incluyendo días con
//   sus citas y estadísticas del período consultado.
//
// - AgendaDayViewDto: Vista de un día específico con todas sus citas.
//
// - AgendaAppointmentDto: Información resumida de una cita para mostrar en
//   la agenda, incluyendo código de color según estado/tipo de cliente.
//
// - AgendaStatsDto: Estadísticas agregadas del período (totales por estado,
//   ingresos, etc.)
//
// Tipos de vista soportados (ViewType):
// - "day": Vista diaria detallada
// - "week": Vista semanal
// - "month": Vista mensual
//
// Códigos de color por estado:
// - Pending: Naranja (#FFA500)
// - Confirmed: Verde (#4CAF50)
// - InProgress: Azul (#2196F3)
// - Completed: Gris (#9E9E9E)
// - Cancelled: Rojo (#F44336)
// - NoShow: Púrpura (#9C27B0)
// - VIP: Dorado (#FFD700)
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para filtrar citas por cliente (query params opcionales en GET /api/Appointment/customer/{customerId})
/// </summary>
public class CustomerAppointmentsQueryDto
{
    /// <summary>Fecha de inicio del rango (opcional)</summary>
    public DateOnly? StartDate { get; set; }
    /// <summary>Fecha de fin del rango (opcional)</summary>
    public DateOnly? EndDate { get; set; }
    /// <summary>Estado de la cita: pending, confirmed, completed, cancelled, etc. (opcional)</summary>
    public string? Status { get; set; }
    /// <summary>Filtrar por empleado (opcional)</summary>
    public int? EmployeeId { get; set; }
    /// <summary>Texto a buscar en la descripción de servicios (opcional)</summary>
    public string? ServicesDescription { get; set; }
}

/// <summary>
/// DTO para consultar la agenda con filtros
/// </summary>
public class AgendaQueryDto
{
    /// <summary>
    /// Fecha de inicio del rango
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Fecha de fin del rango
    /// </summary>
    [Required]
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Filtrar por empleado específico (opcional)
    /// </summary>
    public int? EmployeeId { get; set; }
    
    /// <summary>
    /// Filtrar por estado (opcional)
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Tipo de vista: day, week, month
    /// </summary>
    public string ViewType { get; set; } = "day";
}

/// <summary>
/// DTO de respuesta con la vista de agenda
/// </summary>
public class AgendaResponseDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ViewType { get; set; } = string.Empty;
    
    /// <summary>
    /// Días con sus citas
    /// </summary>
    public List<AgendaDayViewDto> Days { get; set; } = new();
    
    /// <summary>
    /// Estadísticas del período
    /// </summary>
    public AgendaStatsDto Stats { get; set; } = new();
}

/// <summary>
/// DTO de vista diaria de agenda
/// </summary>
public class AgendaDayViewDto
{
    public DateOnly Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    
    /// <summary>
    /// Citas del día
    /// </summary>
    public List<AgendaAppointmentDto> Appointments { get; set; } = new();
    
    /// <summary>
    /// Total de citas del día
    /// </summary>
    public int TotalAppointments => Appointments.Count;
}

/// <summary>
/// DTO simplificado de cita para la agenda
/// </summary>
public class AgendaAppointmentDto
{
    public int Id { get; set; }
    /// <summary>Fecha del día de la cita</summary>
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerCategory { get; set; }
    public bool IsVip { get; set; }
    
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Nombre de los servicios concatenados
    /// </summary>
    public string ServicesDescription { get; set; } = string.Empty;
    /// <summary>
    /// IDs de los servicios de la cita (orden de AppointmentServiceItems)
    /// </summary>
    public List<int> ServiceIds { get; set; } = new();
    
    /// <summary>
    /// Duración total en minutos
    /// </summary>
    public int DurationMinutes { get; set; }
    
    /// <summary>
    /// Color sugerido según estado/tipo
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
}

/// <summary>
/// Estadísticas de la agenda
/// </summary>
public class AgendaStatsDto
{
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int NoShowCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
