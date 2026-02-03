using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface ICancellationPolicyRepository
{
    Task<CancellationPolicy?> GetByIdAsync(int id);
    Task<CancellationPolicy?> GetActiveAsync();
    Task<CancellationPolicy?> CreateAsync(CancellationPolicy policy);
    Task<CancellationPolicy?> UpdateAsync(int id, CancellationPolicy policy);
    Task<CancellationPolicy?> CreateOrUpdateAsync(CancellationPolicy policy);
}
