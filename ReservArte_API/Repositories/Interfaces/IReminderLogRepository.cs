using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IReminderLogRepository
{
    Task<ReminderLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReminderLogDtoOut>> GetByAppointmentIdAsync(int appointmentId);
    Task<ReminderLog> CreateAsync(ReminderLog log);
    Task<bool> UpdateStatusAsync(Guid id, string status, string? externalMessageId = null, string? errorMessage = null);
    /// <summary>Comprueba si ya se envió este recordatorio (config) para esta cita.</summary>
    Task<bool> WasReminderSentAsync(int appointmentId, Guid reminderConfigurationId, string channel);
}
