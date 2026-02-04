using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IMessageTemplateRepository
{
    Task<MessageTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<MessageTemplate>> GetAllAsync();
    Task<MessageTemplate?> CreateAsync(MessageTemplate template);
    Task<MessageTemplate?> UpdateAsync(Guid id, MessageTemplate template);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<MessageTemplate>> GetByTypeAsync(string type);
}
