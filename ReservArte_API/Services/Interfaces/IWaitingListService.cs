using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IWaitingListService
{
    Task<WaitingListDtoOut?> GetByIdAsync(int id);
    Task<IEnumerable<WaitingListDtoOut>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<WaitingListDtoOut>> GetByOrganizationAsync(int organizationId);
    
    Task<WaitingListDtoOut?> AddToWaitingListAsync(WaitingListDtoIn dto);
    Task<bool> RemoveFromWaitingListAsync(int id);
    
    /// <summary>
    /// Notifica a los clientes en lista de espera cuando se libera un slot
    /// </summary>
    Task<IEnumerable<WaitingListDtoOut>> NotifyMatchingCustomersAsync(
        int organizationId, int serviceId, DateTime date, int? employeeId = null);
}
