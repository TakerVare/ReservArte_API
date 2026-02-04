namespace ReservArte_API.Models.DTOs;

/// <summary>
/// Resultado de confirmar o cancelar cita mediante token (link one-click).
/// </summary>
public class ConfirmAppointmentByTokenResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public string? NewStatus { get; set; }
}
