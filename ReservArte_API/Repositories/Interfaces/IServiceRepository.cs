using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IServiceRepository
{
    #region Service CRUD

    Task<IEnumerable<ServiceListDtoOut>> GetAllServicesAsync(int? organizationId = null, int? categoryId = null, bool? isActive = null);
    Task<Service?> GetServiceByIdAsync(int id);
    Task<Service?> CreateServiceAsync(Service service);
    Task<Service?> UpdateServiceAsync(int id, Service service);
    Task<bool> DeleteServiceAsync(int id);

    #endregion

    #region ServiceCategory CRUD

    Task<IEnumerable<ServiceCategoryDtoOut>> GetAllCategoriesAsync(int? organizationId = null);
    Task<ServiceCategory?> GetCategoryByIdAsync(int id);
    Task<ServiceCategory?> CreateCategoryAsync(ServiceCategory category);
    Task<ServiceCategory?> UpdateCategoryAsync(int id, ServiceCategory category);
    Task<bool> DeleteCategoryAsync(int id);

    #endregion

    #region ServiceVariation

    Task<IEnumerable<ServiceVariation>> GetVariationsByServiceIdAsync(int serviceId);
    Task<ServiceVariation?> GetVariationByIdAsync(int id);
    Task<ServiceVariation?> CreateVariationAsync(ServiceVariation variation);
    Task<ServiceVariation?> UpdateVariationAsync(int id, ServiceVariation variation);
    Task<bool> DeleteVariationAsync(int id);

    #endregion

    #region ServicePricing

    Task<IEnumerable<ServicePricing>> GetPricingsByServiceIdAsync(int serviceId);
    Task<ServicePricing?> UpsertPricingAsync(ServicePricing pricing);
    Task<bool> DeletePricingAsync(int id);

    #endregion

    #region Product CRUD

    Task<IEnumerable<ProductDtoOut>> GetAllProductsAsync(int? organizationId = null);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(int id, Product product);
    Task<bool> DeleteProductAsync(int id);

    #endregion

    #region ServiceProduct

    Task<IEnumerable<ServiceProduct>> GetProductsByServiceIdAsync(int serviceId);
    Task<ServiceProduct?> CreateServiceProductAsync(ServiceProduct serviceProduct);
    Task<bool> DeleteServiceProductAsync(int id);

    #endregion

    #region ServicePackage CRUD

    Task<IEnumerable<ServicePackageListDtoOut>> GetAllPackagesAsync(int? organizationId = null, bool? isActive = null);
    Task<ServicePackage?> GetPackageByIdAsync(int id);
    Task<ServicePackage?> CreatePackageAsync(ServicePackage package);
    Task<ServicePackage?> UpdatePackageAsync(int id, ServicePackage package);
    Task<bool> DeletePackageAsync(int id);

    #endregion

    #region ServicePackageItem

    Task<IEnumerable<ServicePackageItem>> GetItemsByPackageIdAsync(int packageId);
    Task<ServicePackageItem?> CreatePackageItemAsync(ServicePackageItem item);
    Task<ServicePackageItem?> UpdatePackageItemOrderAsync(int id, int order);
    Task<bool> DeletePackageItemAsync(int id);

    #endregion

    #region ServicePromotion CRUD

    Task<IEnumerable<ServicePromotionDtoOut>> GetAllPromotionsAsync(int? organizationId = null, bool? activeOnly = null);
    Task<ServicePromotion?> GetPromotionByIdAsync(int id);
    Task<ServicePromotion?> CreatePromotionAsync(ServicePromotion promotion);
    Task<ServicePromotion?> UpdatePromotionAsync(int id, ServicePromotion promotion);
    Task<bool> DeletePromotionAsync(int id);
    Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForServiceAsync(int serviceId);
    Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForPackageAsync(int packageId);

    #endregion
}
