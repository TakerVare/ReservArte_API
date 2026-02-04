using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IReminderConfigurationRepository
{
    Task<ReminderConfiguration?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReminderConfigurationDtoOut>> GetAllAsync();
    Task<ReminderConfiguration?> CreateAsync(ReminderConfiguration config);
    Task<ReminderConfiguration?> UpdateAsync(Guid id, ReminderConfiguration config);
    Task<bool> DeleteAsync(Guid id);
    /// <summary>Configuraciones activas ordenadas por ReminderOrder (1º, 2º, 3º...).</summary>
    Task<IEnumerable<ReminderConfiguration>> GetActiveOrderedAsync();
}
