using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IWaitingListRepository
{
    Task<WaitingList?> GetByIdAsync(int id);
    Task<WaitingListDtoOut?> GetByIdDetailedAsync(int id);
    Task<IEnumerable<WaitingListDtoOut>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<WaitingListDtoOut>> GetAllAsync();
    
    /// <summary>
    /// Obtiene clientes en lista de espera que coinciden con un slot liberado
    /// </summary>
    Task<IEnumerable<WaitingListDtoOut>> GetMatchingForSlotAsync(int serviceId, DateTime date, int? employeeId = null);
    
    Task<WaitingList?> CreateAsync(WaitingList waitingList);
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Marca una entrada como notificada
    /// </summary>
    Task<bool> MarkNotifiedAsync(int id);
    
    /// <summary>
    /// Calcula la prioridad basada en categoría del cliente y servicios contratados
    /// </summary>
    Task<int> CalculatePriorityAsync(int customerId);
}
