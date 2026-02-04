using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IConfirmationTokenRepository
{
    Task<ConfirmationToken?> GetByTokenAsync(string token);
    Task<ConfirmationToken> CreateAsync(ConfirmationToken token);
    Task<bool> MarkAsUsedAsync(string token);
    Task<int> DeleteExpiredAsync();
}
