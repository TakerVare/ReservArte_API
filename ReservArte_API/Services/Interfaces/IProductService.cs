using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IProductService
{
    // CRUD Productos
    Task<CatalogProductDtoOut?> GetByIdAsync(int id);
    Task<IEnumerable<CatalogProductListDtoOut>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<CatalogProductListDtoOut>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<CatalogProductListDtoOut>> SearchAsync(string searchTerm);
    Task<CatalogProductDtoOut> CreateAsync(CatalogProductDtoIn dto);
    Task<CatalogProductDtoOut?> UpdateAsync(int id, CatalogProductDtoIn dto);
    Task<bool> DeleteAsync(int id);
    
    // Categorías
    Task<IEnumerable<ProductCategoryDtoOut>> GetCategoriesAsync(bool includeInactive = false);
    Task<ProductCategoryDtoOut?> GetCategoryByIdAsync(int id);
    Task<ProductCategoryDtoOut> CreateCategoryAsync(ProductCategoryDtoIn dto);
    Task<ProductCategoryDtoOut?> UpdateCategoryAsync(int id, ProductCategoryDtoIn dto);
    Task<bool> DeleteCategoryAsync(int id);
    
    // Inventario
    Task<IEnumerable<InventoryMovementDtoOut>> GetMovementsAsync(InventoryMovementFilterDto filter);
    Task<InventoryMovementDtoOut> RegisterPurchaseAsync(PurchaseDtoIn dto, int employeeId);
    Task<InventoryMovementDtoOut> AdjustStockAsync(StockAdjustmentDtoIn dto, int employeeId);
    Task<InventoryMovementDtoOut> RegisterWasteAsync(int productId, int quantity, string notes, int employeeId);
    Task<IEnumerable<StockAlertDtoOut>> GetLowStockAlertsAsync();
    
    // Ventas
    Task<ProductSaleDtoOut> CreateSaleAsync(ProductSaleDtoIn dto, int employeeId);
    Task<IEnumerable<ProductSaleDtoOut>> GetSalesAsync(ProductSaleFilterDto filter);
    Task<ProductSaleDtoOut?> GetSaleByIdAsync(int id);
    Task<SalesSummaryDtoOut> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate);
    Task<bool> CancelSaleAsync(int saleId);
}
