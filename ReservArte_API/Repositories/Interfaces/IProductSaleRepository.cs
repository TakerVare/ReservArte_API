using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IProductSaleRepository
{
    Task<ProductSale?> GetByIdAsync(int id);
    Task<IEnumerable<ProductSale>> GetAllAsync(
        int? customerId = null,
        int? appointmentId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<ProductSale> CreateAsync(ProductSale sale);
    Task<bool> AddSaleItemAsync(ProductSaleItem item);
    Task<IEnumerable<ProductSaleItem>> GetSaleItemsAsync(int saleId);
    Task<bool> UpdateStatusAsync(int saleId, string status);
    
    // Estadísticas
    Task<(int totalSales, decimal totalAmount, int itemsSold)> GetSalesSummaryAsync(
        DateTime? fromDate = null, DateTime? toDate = null);
}
