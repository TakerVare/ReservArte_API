using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IProductRepository
{
    // CRUD Productos
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    
    // Stock
    Task<bool> UpdateStockAsync(int productId, int quantityChange);
    Task<IEnumerable<Product>> GetLowStockAsync();
    
    // Categorías
    Task<IEnumerable<ProductCategory>> GetCategoriesAsync(bool includeInactive = false);
    Task<ProductCategory?> GetCategoryByIdAsync(int id);
    Task<ProductCategory> CreateCategoryAsync(ProductCategory category);
    Task<bool> UpdateCategoryAsync(ProductCategory category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<int> GetCategoryProductCountAsync(int categoryId);
}
