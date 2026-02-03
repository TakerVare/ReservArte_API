using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repository;

    public ServiceService(IServiceRepository serviceRepository)
    {
        _repository = serviceRepository;
    }

    #region Service CRUD

    public async Task<IEnumerable<ServiceListDtoOut>> GetAllServicesAsync(int? categoryId = null, bool? isActive = null)
    {
        return await _repository.GetAllServicesAsync(categoryId, isActive);
    }

    public async Task<ServiceDtoOut?> GetServiceByIdAsync(int id)
    {
        var service = await _repository.GetServiceByIdAsync(id);
        if (service == null) return null;

        // Get related data
        var variations = await _repository.GetVariationsByServiceIdAsync(id);
        var pricings = await _repository.GetPricingsByServiceIdAsync(id);
        var products = await _repository.GetProductsByServiceIdAsync(id);

        return MapToServiceDtoOut(service, variations, pricings, products);
    }

    public async Task<ServiceDtoOut?> CreateServiceAsync(ServiceDtoIn serviceDto)
    {
        var service = new Service
        {
            Name = serviceDto.Name,
            Description = serviceDto.Description,
            DurationMinutes = serviceDto.DurationMinutes,
            BasePrice = serviceDto.BasePrice,
            CategoryId = serviceDto.CategoryId,
            ImageUrl = serviceDto.ImageUrl,
            IsActive = serviceDto.IsActive,
            RequiresAllergyTest = serviceDto.RequiresAllergyTest,
            AllergyTestHoursBefore = serviceDto.AllergyTestHoursBefore,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateServiceAsync(service);
        if (created == null) return null;

        return await GetServiceByIdAsync(created.Id);
    }

    public async Task<ServiceDtoOut?> UpdateServiceAsync(int id, ServiceDtoIn serviceDto)
    {
        var existingService = await _repository.GetServiceByIdAsync(id);
        if (existingService == null) return null;

        existingService.Name = serviceDto.Name;
        existingService.Description = serviceDto.Description;
        existingService.DurationMinutes = serviceDto.DurationMinutes;
        existingService.BasePrice = serviceDto.BasePrice;
        existingService.CategoryId = serviceDto.CategoryId;
        existingService.ImageUrl = serviceDto.ImageUrl;
        existingService.IsActive = serviceDto.IsActive;
        existingService.RequiresAllergyTest = serviceDto.RequiresAllergyTest;
        existingService.AllergyTestHoursBefore = serviceDto.AllergyTestHoursBefore;

        var updated = await _repository.UpdateServiceAsync(id, existingService);
        if (updated == null) return null;

        return await GetServiceByIdAsync(id);
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        return await _repository.DeleteServiceAsync(id);
    }

    #endregion

    #region ServiceCategory CRUD

    public async Task<IEnumerable<ServiceCategoryDtoOut>> GetAllCategoriesAsync()
    {
        return await _repository.GetAllCategoriesAsync();
    }

    public async Task<ServiceCategoryDtoOut?> GetCategoryByIdAsync(int id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null) return null;

        return MapToCategoryDtoOut(category);
    }

    public async Task<ServiceCategoryDtoOut?> CreateCategoryAsync(ServiceCategoryDtoIn categoryDto)
    {
        var category = new ServiceCategory
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            Color = categoryDto.Color,
            DisplayOrder = categoryDto.DisplayOrder,
            IsActive = categoryDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateCategoryAsync(category);
        if (created == null) return null;

        return MapToCategoryDtoOut(created);
    }

    public async Task<ServiceCategoryDtoOut?> UpdateCategoryAsync(int id, ServiceCategoryDtoIn categoryDto)
    {
        var existingCategory = await _repository.GetCategoryByIdAsync(id);
        if (existingCategory == null) return null;

        existingCategory.Name = categoryDto.Name;
        existingCategory.Description = categoryDto.Description;
        existingCategory.Color = categoryDto.Color;
        existingCategory.DisplayOrder = categoryDto.DisplayOrder;
        existingCategory.IsActive = categoryDto.IsActive;

        var updated = await _repository.UpdateCategoryAsync(id, existingCategory);
        if (updated == null) return null;

        return MapToCategoryDtoOut(updated);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _repository.DeleteCategoryAsync(id);
    }

    #endregion

    #region ServiceVariation

    public async Task<IEnumerable<ServiceVariationDtoOut>> GetVariationsByServiceIdAsync(int serviceId)
    {
        var variations = await _repository.GetVariationsByServiceIdAsync(serviceId);
        return variations.Select(MapToVariationDtoOut);
    }

    public async Task<ServiceVariationDtoOut?> CreateVariationAsync(ServiceVariationDtoIn variationDto)
    {
        // Verify service exists
        var service = await _repository.GetServiceByIdAsync(variationDto.ServiceId);
        if (service == null) return null;

        var variation = new ServiceVariation
        {
            ServiceId = variationDto.ServiceId,
            Name = variationDto.Name,
            PriceModifier = variationDto.PriceModifier,
            DurationModifier = variationDto.DurationModifier,
            IsActive = variationDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateVariationAsync(variation);
        if (created == null) return null;

        return MapToVariationDtoOut(created);
    }

    public async Task<ServiceVariationDtoOut?> UpdateVariationAsync(int id, ServiceVariationDtoIn variationDto)
    {
        var existingVariation = await _repository.GetVariationByIdAsync(id);
        if (existingVariation == null) return null;

        existingVariation.Name = variationDto.Name;
        existingVariation.PriceModifier = variationDto.PriceModifier;
        existingVariation.DurationModifier = variationDto.DurationModifier;
        existingVariation.IsActive = variationDto.IsActive;

        var updated = await _repository.UpdateVariationAsync(id, existingVariation);
        if (updated == null) return null;

        return MapToVariationDtoOut(updated);
    }

    public async Task<bool> DeleteVariationAsync(int id)
    {
        return await _repository.DeleteVariationAsync(id);
    }

    #endregion

    #region ServicePricing

    public async Task<IEnumerable<ServicePricingDtoOut>> GetPricingsByServiceIdAsync(int serviceId)
    {
        var pricings = await _repository.GetPricingsByServiceIdAsync(serviceId);
        return pricings.Select(MapToPricingDtoOut);
    }

    public async Task<ServicePricingDtoOut?> UpsertPricingAsync(ServicePricingDtoIn pricingDto)
    {
        // Verify service exists
        var service = await _repository.GetServiceByIdAsync(pricingDto.ServiceId);
        if (service == null) return null;

        // Validate employee level
        if (!EmployeeLevels.IsValid(pricingDto.EmployeeLevel))
        {
            return null;
        }

        var pricing = new ServicePricing
        {
            ServiceId = pricingDto.ServiceId,
            EmployeeLevel = pricingDto.EmployeeLevel,
            Price = pricingDto.Price,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.UpsertPricingAsync(pricing);
        if (result == null) return null;

        return MapToPricingDtoOut(result);
    }

    public async Task<bool> DeletePricingAsync(int id)
    {
        return await _repository.DeletePricingAsync(id);
    }

    #endregion

    #region Product CRUD

    public async Task<IEnumerable<ProductDtoOut>> GetAllProductsAsync()
    {
        return await _repository.GetAllProductsAsync();
    }

    public async Task<ProductDtoOut?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetProductByIdAsync(id);
        if (product == null) return null;

        return MapToProductDtoOut(product);
    }

    public async Task<ProductDtoOut?> CreateProductAsync(ProductDtoIn productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Brand = productDto.Brand,
            Sku = productDto.Sku,
            IsActive = productDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateProductAsync(product);
        if (created == null) return null;

        return MapToProductDtoOut(created);
    }

    public async Task<ProductDtoOut?> UpdateProductAsync(int id, ProductDtoIn productDto)
    {
        var existingProduct = await _repository.GetProductByIdAsync(id);
        if (existingProduct == null) return null;

        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Brand = productDto.Brand;
        existingProduct.Sku = productDto.Sku;
        existingProduct.IsActive = productDto.IsActive;

        var updated = await _repository.UpdateProductAsync(id, existingProduct);
        if (updated == null) return null;

        return MapToProductDtoOut(updated);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteProductAsync(id);
    }

    #endregion

    #region ServiceProduct

    public async Task<IEnumerable<ServiceProductDtoOut>> GetProductsByServiceIdAsync(int serviceId)
    {
        var products = await _repository.GetProductsByServiceIdAsync(serviceId);
        return products.Select(MapToServiceProductDtoOut);
    }

    public async Task<ServiceProductDtoOut?> AddProductToServiceAsync(ServiceProductDtoIn serviceProductDto)
    {
        // Verify service exists
        var service = await _repository.GetServiceByIdAsync(serviceProductDto.ServiceId);
        if (service == null) return null;

        // Verify product exists
        var product = await _repository.GetProductByIdAsync(serviceProductDto.ProductId);
        if (product == null) return null;

        var serviceProduct = new ServiceProduct
        {
            ServiceId = serviceProductDto.ServiceId,
            ProductId = serviceProductDto.ProductId,
            QuantityUsed = serviceProductDto.QuantityUsed,
            Notes = serviceProductDto.Notes,
            ProductName = product.Name,
            ProductBrand = product.Brand
        };

        var created = await _repository.CreateServiceProductAsync(serviceProduct);
        if (created == null) return null;

        created.ProductName = product.Name;
        created.ProductBrand = product.Brand;

        return MapToServiceProductDtoOut(created);
    }

    public async Task<bool> RemoveProductFromServiceAsync(int id)
    {
        return await _repository.DeleteServiceProductAsync(id);
    }

    #endregion

    #region ServicePackage CRUD

    public async Task<IEnumerable<ServicePackageListDtoOut>> GetAllPackagesAsync(bool? isActive = null)
    {
        return await _repository.GetAllPackagesAsync(isActive);
    }

    public async Task<ServicePackageDtoOut?> GetPackageByIdAsync(int id)
    {
        var package = await _repository.GetPackageByIdAsync(id);
        if (package == null) return null;

        // Get package items
        var items = await _repository.GetItemsByPackageIdAsync(id);

        return MapToPackageDtoOut(package, items);
    }

    public async Task<ServicePackageDtoOut?> CreatePackageAsync(ServicePackageDtoIn packageDto)
    {
        var package = new ServicePackage
        {
            Name = packageDto.Name,
            Description = packageDto.Description,
            TotalPrice = packageDto.TotalPrice,
            DiscountPercentage = packageDto.DiscountPercentage,
            ImageUrl = packageDto.ImageUrl,
            IsActive = packageDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreatePackageAsync(package);
        if (created == null) return null;

        return await GetPackageByIdAsync(created.Id);
    }

    public async Task<ServicePackageDtoOut?> UpdatePackageAsync(int id, ServicePackageDtoIn packageDto)
    {
        var existingPackage = await _repository.GetPackageByIdAsync(id);
        if (existingPackage == null) return null;

        existingPackage.Name = packageDto.Name;
        existingPackage.Description = packageDto.Description;
        existingPackage.TotalPrice = packageDto.TotalPrice;
        existingPackage.DiscountPercentage = packageDto.DiscountPercentage;
        existingPackage.ImageUrl = packageDto.ImageUrl;
        existingPackage.IsActive = packageDto.IsActive;

        var updated = await _repository.UpdatePackageAsync(id, existingPackage);
        if (updated == null) return null;

        return await GetPackageByIdAsync(id);
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        return await _repository.DeletePackageAsync(id);
    }

    #endregion

    #region ServicePackageItem

    public async Task<ServicePackageItemDtoOut?> AddServiceToPackageAsync(ServicePackageItemDtoIn itemDto)
    {
        // Verify package exists
        var package = await _repository.GetPackageByIdAsync(itemDto.ServicePackageId);
        if (package == null) return null;

        // Verify service exists
        var service = await _repository.GetServiceByIdAsync(itemDto.ServiceId);
        if (service == null) return null;

        var item = new ServicePackageItem
        {
            ServicePackageId = itemDto.ServicePackageId,
            ServiceId = itemDto.ServiceId,
            Order = itemDto.Order
        };

        var created = await _repository.CreatePackageItemAsync(item);
        if (created == null) return null;

        return new ServicePackageItemDtoOut
        {
            Id = created.Id,
            ServicePackageId = created.ServicePackageId,
            ServiceId = created.ServiceId,
            ServiceName = service.Name,
            ServicePrice = service.BasePrice,
            ServiceDurationMinutes = service.DurationMinutes,
            Order = created.Order
        };
    }

    public async Task<ServicePackageItemDtoOut?> UpdatePackageItemOrderAsync(int id, int order)
    {
        var updated = await _repository.UpdatePackageItemOrderAsync(id, order);
        if (updated == null) return null;

        return new ServicePackageItemDtoOut
        {
            Id = updated.Id,
            Order = updated.Order
        };
    }

    public async Task<bool> RemoveServiceFromPackageAsync(int id)
    {
        return await _repository.DeletePackageItemAsync(id);
    }

    #endregion

    #region ServicePromotion CRUD

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetAllPromotionsAsync(bool? activeOnly = null)
    {
        return await _repository.GetAllPromotionsAsync(activeOnly);
    }

    public async Task<ServicePromotionDtoOut?> GetPromotionByIdAsync(int id)
    {
        var promotion = await _repository.GetPromotionByIdAsync(id);
        if (promotion == null) return null;

        return MapToPromotionDtoOut(promotion);
    }

    public async Task<ServicePromotionDtoOut?> CreatePromotionAsync(ServicePromotionDtoIn promotionDto)
    {
        // Validate that either serviceId or packageId is provided (but not both)
        if (promotionDto.ServiceId == null && promotionDto.ServicePackageId == null)
        {
            return null;
        }

        // Validate date range
        if (promotionDto.EndDate <= promotionDto.StartDate)
        {
            return null;
        }

        // Verify service or package exists
        if (promotionDto.ServiceId.HasValue)
        {
            var service = await _repository.GetServiceByIdAsync(promotionDto.ServiceId.Value);
            if (service == null) return null;
        }

        if (promotionDto.ServicePackageId.HasValue)
        {
            var package = await _repository.GetPackageByIdAsync(promotionDto.ServicePackageId.Value);
            if (package == null) return null;
        }

        var promotion = new ServicePromotion
        {
            ServiceId = promotionDto.ServiceId,
            ServicePackageId = promotionDto.ServicePackageId,
            Name = promotionDto.Name,
            Description = promotionDto.Description,
            DiscountPercentage = promotionDto.DiscountPercentage,
            DiscountAmount = promotionDto.DiscountAmount,
            StartDate = promotionDto.StartDate,
            EndDate = promotionDto.EndDate,
            IsSeasonalService = promotionDto.IsSeasonalService,
            IsActive = promotionDto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreatePromotionAsync(promotion);
        if (created == null) return null;

        return await GetPromotionByIdAsync(created.Id);
    }

    public async Task<ServicePromotionDtoOut?> UpdatePromotionAsync(int id, ServicePromotionDtoIn promotionDto)
    {
        var existingPromotion = await _repository.GetPromotionByIdAsync(id);
        if (existingPromotion == null) return null;

        // Validate date range
        if (promotionDto.EndDate <= promotionDto.StartDate)
        {
            return null;
        }

        existingPromotion.ServiceId = promotionDto.ServiceId;
        existingPromotion.ServicePackageId = promotionDto.ServicePackageId;
        existingPromotion.Name = promotionDto.Name;
        existingPromotion.Description = promotionDto.Description;
        existingPromotion.DiscountPercentage = promotionDto.DiscountPercentage;
        existingPromotion.DiscountAmount = promotionDto.DiscountAmount;
        existingPromotion.StartDate = promotionDto.StartDate;
        existingPromotion.EndDate = promotionDto.EndDate;
        existingPromotion.IsSeasonalService = promotionDto.IsSeasonalService;
        existingPromotion.IsActive = promotionDto.IsActive;

        var updated = await _repository.UpdatePromotionAsync(id, existingPromotion);
        if (updated == null) return null;

        return await GetPromotionByIdAsync(id);
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        return await _repository.DeletePromotionAsync(id);
    }

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForServiceAsync(int serviceId)
    {
        return await _repository.GetActivePromotionsForServiceAsync(serviceId);
    }

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForPackageAsync(int packageId)
    {
        return await _repository.GetActivePromotionsForPackageAsync(packageId);
    }

    #endregion

    #region Private Mappers

    private static ServiceDtoOut MapToServiceDtoOut(Service service, 
        IEnumerable<ServiceVariation> variations, 
        IEnumerable<ServicePricing> pricings, 
        IEnumerable<ServiceProduct> products)
    {
        return new ServiceDtoOut
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            BasePrice = service.BasePrice,
            CategoryId = service.CategoryId,
            CategoryName = service.CategoryName,
            ImageUrl = service.ImageUrl,
            IsActive = service.IsActive,
            RequiresAllergyTest = service.RequiresAllergyTest,
            AllergyTestHoursBefore = service.AllergyTestHoursBefore,
            CreatedAt = service.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = service.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Variations = variations.Select(MapToVariationDtoOut).ToList(),
            Pricings = pricings.Select(MapToPricingDtoOut).ToList(),
            Products = products.Select(MapToServiceProductDtoOut).ToList()
        };
    }

    private static ServiceCategoryDtoOut MapToCategoryDtoOut(ServiceCategory category)
    {
        return new ServiceCategoryDtoOut
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static ServiceVariationDtoOut MapToVariationDtoOut(ServiceVariation variation)
    {
        return new ServiceVariationDtoOut
        {
            Id = variation.Id,
            ServiceId = variation.ServiceId,
            Name = variation.Name,
            PriceModifier = variation.PriceModifier,
            DurationModifier = variation.DurationModifier,
            IsActive = variation.IsActive,
            CreatedAt = variation.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static ServicePricingDtoOut MapToPricingDtoOut(ServicePricing pricing)
    {
        return new ServicePricingDtoOut
        {
            Id = pricing.Id,
            ServiceId = pricing.ServiceId,
            EmployeeLevel = pricing.EmployeeLevel,
            Price = pricing.Price,
            CreatedAt = pricing.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static ProductDtoOut MapToProductDtoOut(Product product)
    {
        return new ProductDtoOut
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Sku = product.Sku,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static ServiceProductDtoOut MapToServiceProductDtoOut(ServiceProduct sp)
    {
        return new ServiceProductDtoOut
        {
            Id = sp.Id,
            ServiceId = sp.ServiceId,
            ProductId = sp.ProductId,
            ProductName = sp.ProductName ?? string.Empty,
            ProductBrand = sp.ProductBrand,
            QuantityUsed = sp.QuantityUsed,
            Notes = sp.Notes
        };
    }

    private static ServicePackageDtoOut MapToPackageDtoOut(ServicePackage package, IEnumerable<ServicePackageItem> items)
    {
        var itemsList = items.ToList();
        var originalPrice = itemsList.Sum(i => i.ServicePrice ?? 0);
        var totalDuration = itemsList.Sum(i => i.ServiceDurationMinutes ?? 0);

        return new ServicePackageDtoOut
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            TotalPrice = package.TotalPrice,
            DiscountPercentage = package.DiscountPercentage,
            OriginalPrice = originalPrice,
            Savings = originalPrice - package.TotalPrice,
            TotalDurationMinutes = totalDuration,
            ImageUrl = package.ImageUrl,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = package.UpdatedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Items = itemsList.Select(i => new ServicePackageItemDtoOut
            {
                Id = i.Id,
                ServicePackageId = i.ServicePackageId,
                ServiceId = i.ServiceId,
                ServiceName = i.ServiceName ?? string.Empty,
                ServicePrice = i.ServicePrice ?? 0,
                ServiceDurationMinutes = i.ServiceDurationMinutes ?? 0,
                Order = i.Order
            }).ToList()
        };
    }

    private static ServicePromotionDtoOut MapToPromotionDtoOut(ServicePromotion promotion)
    {
        return new ServicePromotionDtoOut
        {
            Id = promotion.Id,
            ServiceId = promotion.ServiceId,
            ServiceName = promotion.ServiceName,
            ServicePackageId = promotion.ServicePackageId,
            PackageName = promotion.PackageName,
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountPercentage = promotion.DiscountPercentage,
            DiscountAmount = promotion.DiscountAmount,
            StartDate = promotion.StartDate.ToString("yyyy-MM-dd"),
            EndDate = promotion.EndDate.ToString("yyyy-MM-dd"),
            IsSeasonalService = promotion.IsSeasonalService,
            IsActive = promotion.IsActive,
            IsCurrentlyActive = promotion.IsCurrentlyActive,
            CreatedAt = promotion.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    #endregion
}
