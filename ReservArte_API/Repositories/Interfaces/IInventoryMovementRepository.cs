using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IInventoryMovementRepository
{
    Task<InventoryMovement?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(int productId);
    Task<IEnumerable<InventoryMovement>> GetAllAsync(
        int? productId = null,
        string? movementType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<InventoryMovement> CreateAsync(InventoryMovement movement);
}
