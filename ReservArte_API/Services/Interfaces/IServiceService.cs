using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IServiceService
{
    #region Service CRUD

    Task<IEnumerable<ServiceListDtoOut>> GetAllServicesAsync(int? organizationId = null, int? categoryId = null, bool? isActive = null);
    Task<ServiceDtoOut?> GetServiceByIdAsync(int id);
    Task<ServiceDtoOut?> CreateServiceAsync(ServiceDtoIn serviceDto);
    Task<ServiceDtoOut?> UpdateServiceAsync(int id, ServiceDtoIn serviceDto);
    Task<bool> DeleteServiceAsync(int id);

    #endregion

    #region ServiceCategory CRUD

    Task<IEnumerable<ServiceCategoryDtoOut>> GetAllCategoriesAsync(int? organizationId = null);
    Task<ServiceCategoryDtoOut?> GetCategoryByIdAsync(int id);
    Task<ServiceCategoryDtoOut?> CreateCategoryAsync(ServiceCategoryDtoIn categoryDto);
    Task<ServiceCategoryDtoOut?> UpdateCategoryAsync(int id, ServiceCategoryDtoIn categoryDto);
    Task<bool> DeleteCategoryAsync(int id);

    #endregion

    #region ServiceVariation

    Task<IEnumerable<ServiceVariationDtoOut>> GetVariationsByServiceIdAsync(int serviceId);
    Task<ServiceVariationDtoOut?> CreateVariationAsync(ServiceVariationDtoIn variationDto);
    Task<ServiceVariationDtoOut?> UpdateVariationAsync(int id, ServiceVariationDtoIn variationDto);
    Task<bool> DeleteVariationAsync(int id);

    #endregion

    #region ServicePricing

    Task<IEnumerable<ServicePricingDtoOut>> GetPricingsByServiceIdAsync(int serviceId);
    Task<ServicePricingDtoOut?> UpsertPricingAsync(ServicePricingDtoIn pricingDto);
    Task<bool> DeletePricingAsync(int id);

    #endregion

    #region Product CRUD

    Task<IEnumerable<ProductDtoOut>> GetAllProductsAsync(int? organizationId = null);
    Task<ProductDtoOut?> GetProductByIdAsync(int id);
    Task<ProductDtoOut?> CreateProductAsync(ProductDtoIn productDto);
    Task<ProductDtoOut?> UpdateProductAsync(int id, ProductDtoIn productDto);
    Task<bool> DeleteProductAsync(int id);

    #endregion

    #region ServiceProduct

    Task<IEnumerable<ServiceProductDtoOut>> GetProductsByServiceIdAsync(int serviceId);
    Task<ServiceProductDtoOut?> AddProductToServiceAsync(ServiceProductDtoIn serviceProductDto);
    Task<bool> RemoveProductFromServiceAsync(int id);

    #endregion

    #region ServicePackage CRUD

    Task<IEnumerable<ServicePackageListDtoOut>> GetAllPackagesAsync(int? organizationId = null, bool? isActive = null);
    Task<ServicePackageDtoOut?> GetPackageByIdAsync(int id);
    Task<ServicePackageDtoOut?> CreatePackageAsync(ServicePackageDtoIn packageDto);
    Task<ServicePackageDtoOut?> UpdatePackageAsync(int id, ServicePackageDtoIn packageDto);
    Task<bool> DeletePackageAsync(int id);

    #endregion

    #region ServicePackageItem

    Task<ServicePackageItemDtoOut?> AddServiceToPackageAsync(ServicePackageItemDtoIn itemDto);
    Task<ServicePackageItemDtoOut?> UpdatePackageItemOrderAsync(int id, int order);
    Task<bool> RemoveServiceFromPackageAsync(int id);

    #endregion

    #region ServicePromotion CRUD

    Task<IEnumerable<ServicePromotionDtoOut>> GetAllPromotionsAsync(int? organizationId = null, bool? activeOnly = null);
    Task<ServicePromotionDtoOut?> GetPromotionByIdAsync(int id);
    Task<ServicePromotionDtoOut?> CreatePromotionAsync(ServicePromotionDtoIn promotionDto);
    Task<ServicePromotionDtoOut?> UpdatePromotionAsync(int id, ServicePromotionDtoIn promotionDto);
    Task<bool> DeletePromotionAsync(int id);
    Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForServiceAsync(int serviceId);
    Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForPackageAsync(int packageId);

    #endregion
}
