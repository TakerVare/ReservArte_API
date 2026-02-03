using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    #region Service CRUD

    /// <summary>
    /// Get all services (optionally filtered)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServiceListDtoOut>>> GetAllServices(
        [FromQuery] int? organizationId = null, 
        [FromQuery] int? categoryId = null, 
        [FromQuery] bool? isActive = null)
    {
        var services = await _serviceService.GetAllServicesAsync(organizationId, categoryId, isActive);
        return Ok(services);
    }

    /// <summary>
    /// Get service by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<ServiceDtoOut>> GetServiceById(int id)
    {
        var service = await _serviceService.GetServiceByIdAsync(id);
        if (service == null)
        {
            return NotFound(new { message = "Servicio no encontrado" });
        }
        return Ok(service);
    }

    /// <summary>
    /// Create a new service
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceDtoOut>> CreateService([FromBody] ServiceDtoIn serviceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdService = await _serviceService.CreateServiceAsync(serviceDto);
        if (createdService == null)
        {
            return BadRequest(new { message = "Error al crear el servicio" });
        }

        return CreatedAtAction(nameof(GetServiceById), new { id = createdService.Id }, createdService);
    }

    /// <summary>
    /// Update an existing service
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceDtoOut>> UpdateService(int id, [FromBody] ServiceDtoIn serviceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedService = await _serviceService.UpdateServiceAsync(id, serviceDto);
        if (updatedService == null)
        {
            return NotFound(new { message = "Servicio no encontrado" });
        }

        return Ok(updatedService);
    }

    /// <summary>
    /// Delete a service
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteService(int id)
    {
        var deleted = await _serviceService.DeleteServiceAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Servicio no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region ServiceCategory CRUD

    /// <summary>
    /// Get all service categories
    /// </summary>
    [HttpGet("categories")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServiceCategoryDtoOut>>> GetAllCategories([FromQuery] int? organizationId = null)
    {
        var categories = await _serviceService.GetAllCategoriesAsync(organizationId);
        return Ok(categories);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<ServiceCategoryDtoOut>> GetCategoryById(int id)
    {
        var category = await _serviceService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }
        return Ok(category);
    }

    /// <summary>
    /// Create a new service category
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceCategoryDtoOut>> CreateCategory([FromBody] ServiceCategoryDtoIn categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdCategory = await _serviceService.CreateCategoryAsync(categoryDto);
        if (createdCategory == null)
        {
            return BadRequest(new { message = "Error al crear la categoría" });
        }

        return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceCategoryDtoOut>> UpdateCategory(int id, [FromBody] ServiceCategoryDtoIn categoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedCategory = await _serviceService.UpdateCategoryAsync(id, categoryDto);
        if (updatedCategory == null)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }

        return Ok(updatedCategory);
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _serviceService.DeleteCategoryAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }

        return NoContent();
    }

    #endregion

    #region ServiceVariation

    /// <summary>
    /// Get variations for a service
    /// </summary>
    [HttpGet("{serviceId}/variations")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServiceVariationDtoOut>>> GetVariations(int serviceId)
    {
        var variations = await _serviceService.GetVariationsByServiceIdAsync(serviceId);
        return Ok(variations);
    }

    /// <summary>
    /// Create a variation for a service
    /// </summary>
    [HttpPost("{serviceId}/variations")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceVariationDtoOut>> CreateVariation(int serviceId, [FromBody] ServiceVariationDtoIn variationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        variationDto.ServiceId = serviceId;

        var createdVariation = await _serviceService.CreateVariationAsync(variationDto);
        if (createdVariation == null)
        {
            return NotFound(new { message = "Servicio no encontrado" });
        }

        return CreatedAtAction(nameof(GetVariations), new { serviceId }, createdVariation);
    }

    /// <summary>
    /// Update a variation
    /// </summary>
    [HttpPut("variations/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceVariationDtoOut>> UpdateVariation(int id, [FromBody] ServiceVariationDtoIn variationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedVariation = await _serviceService.UpdateVariationAsync(id, variationDto);
        if (updatedVariation == null)
        {
            return NotFound(new { message = "Variación no encontrada" });
        }

        return Ok(updatedVariation);
    }

    /// <summary>
    /// Delete a variation
    /// </summary>
    [HttpDelete("variations/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteVariation(int id)
    {
        var deleted = await _serviceService.DeleteVariationAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Variación no encontrada" });
        }

        return NoContent();
    }

    #endregion

    #region ServicePricing

    /// <summary>
    /// Get pricing levels for a service
    /// </summary>
    [HttpGet("{serviceId}/pricing")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<ServicePricingDtoOut>>> GetPricings(int serviceId)
    {
        var pricings = await _serviceService.GetPricingsByServiceIdAsync(serviceId);
        return Ok(pricings);
    }

    /// <summary>
    /// Set pricing for a service by employee level
    /// </summary>
    [HttpPut("{serviceId}/pricing")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePricingDtoOut>> UpsertPricing(int serviceId, [FromBody] ServicePricingDtoIn pricingDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        pricingDto.ServiceId = serviceId;

        var result = await _serviceService.UpsertPricingAsync(pricingDto);
        if (result == null)
        {
            return BadRequest(new { message = "Servicio no encontrado o nivel de empleado inválido" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a pricing level
    /// </summary>
    [HttpDelete("pricing/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeletePricing(int id)
    {
        var deleted = await _serviceService.DeletePricingAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Precio no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region Product CRUD

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet("products")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<ProductDtoOut>>> GetAllProducts([FromQuery] int? organizationId = null)
    {
        var products = await _serviceService.GetAllProductsAsync(organizationId);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("products/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<ProductDtoOut>> GetProductById(int id)
    {
        var product = await _serviceService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost("products")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDtoOut>> CreateProduct([FromBody] ProductDtoIn productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdProduct = await _serviceService.CreateProductAsync(productDto);
        if (createdProduct == null)
        {
            return BadRequest(new { message = "Error al crear el producto" });
        }

        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("products/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDtoOut>> UpdateProduct(int id, [FromBody] ProductDtoIn productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedProduct = await _serviceService.UpdateProductAsync(id, productDto);
        if (updatedProduct == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        return Ok(updatedProduct);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("products/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var deleted = await _serviceService.DeleteProductAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region ServiceProduct

    /// <summary>
    /// Get products used by a service
    /// </summary>
    [HttpGet("{serviceId}/products")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<ServiceProductDtoOut>>> GetServiceProducts(int serviceId)
    {
        var products = await _serviceService.GetProductsByServiceIdAsync(serviceId);
        return Ok(products);
    }

    /// <summary>
    /// Add a product to a service
    /// </summary>
    [HttpPost("{serviceId}/products")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServiceProductDtoOut>> AddProductToService(int serviceId, [FromBody] ServiceProductDtoIn serviceProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        serviceProductDto.ServiceId = serviceId;

        var result = await _serviceService.AddProductToServiceAsync(serviceProductDto);
        if (result == null)
        {
            return NotFound(new { message = "Servicio o producto no encontrado" });
        }

        return CreatedAtAction(nameof(GetServiceProducts), new { serviceId }, result);
    }

    /// <summary>
    /// Remove a product from a service
    /// </summary>
    [HttpDelete("service-products/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RemoveProductFromService(int id)
    {
        var deleted = await _serviceService.RemoveProductFromServiceAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Relación servicio-producto no encontrada" });
        }

        return NoContent();
    }

    #endregion

    #region ServicePackage CRUD

    /// <summary>
    /// Get all service packages
    /// </summary>
    [HttpGet("packages")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServicePackageListDtoOut>>> GetAllPackages(
        [FromQuery] int? organizationId = null, 
        [FromQuery] bool? isActive = null)
    {
        var packages = await _serviceService.GetAllPackagesAsync(organizationId, isActive);
        return Ok(packages);
    }

    /// <summary>
    /// Get package by ID
    /// </summary>
    [HttpGet("packages/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<ServicePackageDtoOut>> GetPackageById(int id)
    {
        var package = await _serviceService.GetPackageByIdAsync(id);
        if (package == null)
        {
            return NotFound(new { message = "Paquete no encontrado" });
        }
        return Ok(package);
    }

    /// <summary>
    /// Create a new service package
    /// </summary>
    [HttpPost("packages")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePackageDtoOut>> CreatePackage([FromBody] ServicePackageDtoIn packageDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdPackage = await _serviceService.CreatePackageAsync(packageDto);
        if (createdPackage == null)
        {
            return BadRequest(new { message = "Error al crear el paquete" });
        }

        return CreatedAtAction(nameof(GetPackageById), new { id = createdPackage.Id }, createdPackage);
    }

    /// <summary>
    /// Update an existing package
    /// </summary>
    [HttpPut("packages/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePackageDtoOut>> UpdatePackage(int id, [FromBody] ServicePackageDtoIn packageDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedPackage = await _serviceService.UpdatePackageAsync(id, packageDto);
        if (updatedPackage == null)
        {
            return NotFound(new { message = "Paquete no encontrado" });
        }

        return Ok(updatedPackage);
    }

    /// <summary>
    /// Delete a package
    /// </summary>
    [HttpDelete("packages/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var deleted = await _serviceService.DeletePackageAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Paquete no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region ServicePackageItem

    /// <summary>
    /// Add a service to a package
    /// </summary>
    [HttpPost("packages/{packageId}/services")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePackageItemDtoOut>> AddServiceToPackage(int packageId, [FromBody] ServicePackageItemDtoIn itemDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        itemDto.ServicePackageId = packageId;

        var result = await _serviceService.AddServiceToPackageAsync(itemDto);
        if (result == null)
        {
            return NotFound(new { message = "Paquete o servicio no encontrado" });
        }

        return CreatedAtAction(nameof(GetPackageById), new { id = packageId }, result);
    }

    /// <summary>
    /// Update the order of a service in a package
    /// </summary>
    [HttpPut("package-items/{id}/order")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePackageItemDtoOut>> UpdatePackageItemOrder(int id, [FromBody] int order)
    {
        var result = await _serviceService.UpdatePackageItemOrderAsync(id, order);
        if (result == null)
        {
            return NotFound(new { message = "Item del paquete no encontrado" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove a service from a package
    /// </summary>
    [HttpDelete("package-items/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RemoveServiceFromPackage(int id)
    {
        var deleted = await _serviceService.RemoveServiceFromPackageAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Item del paquete no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region ServicePromotion CRUD

    /// <summary>
    /// Get all promotions
    /// </summary>
    [HttpGet("promotions")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServicePromotionDtoOut>>> GetAllPromotions(
        [FromQuery] int? organizationId = null, 
        [FromQuery] bool? activeOnly = null)
    {
        var promotions = await _serviceService.GetAllPromotionsAsync(organizationId, activeOnly);
        return Ok(promotions);
    }

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    [HttpGet("promotions/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<ServicePromotionDtoOut>> GetPromotionById(int id)
    {
        var promotion = await _serviceService.GetPromotionByIdAsync(id);
        if (promotion == null)
        {
            return NotFound(new { message = "Promoción no encontrada" });
        }
        return Ok(promotion);
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    [HttpPost("promotions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePromotionDtoOut>> CreatePromotion([FromBody] ServicePromotionDtoIn promotionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdPromotion = await _serviceService.CreatePromotionAsync(promotionDto);
        if (createdPromotion == null)
        {
            return BadRequest(new { message = "Error al crear la promoción. Verifica que el servicio o paquete exista y que las fechas sean válidas." });
        }

        return CreatedAtAction(nameof(GetPromotionById), new { id = createdPromotion.Id }, createdPromotion);
    }

    /// <summary>
    /// Update an existing promotion
    /// </summary>
    [HttpPut("promotions/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ServicePromotionDtoOut>> UpdatePromotion(int id, [FromBody] ServicePromotionDtoIn promotionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedPromotion = await _serviceService.UpdatePromotionAsync(id, promotionDto);
        if (updatedPromotion == null)
        {
            return NotFound(new { message = "Promoción no encontrada o fechas inválidas" });
        }

        return Ok(updatedPromotion);
    }

    /// <summary>
    /// Delete a promotion
    /// </summary>
    [HttpDelete("promotions/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var deleted = await _serviceService.DeletePromotionAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Promoción no encontrada" });
        }

        return NoContent();
    }

    /// <summary>
    /// Get active promotions for a specific service
    /// </summary>
    [HttpGet("{serviceId}/promotions")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServicePromotionDtoOut>>> GetActivePromotionsForService(int serviceId)
    {
        var promotions = await _serviceService.GetActivePromotionsForServiceAsync(serviceId);
        return Ok(promotions);
    }

    /// <summary>
    /// Get active promotions for a specific package
    /// </summary>
    [HttpGet("packages/{packageId}/promotions")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<ServicePromotionDtoOut>>> GetActivePromotionsForPackage(int packageId)
    {
        var promotions = await _serviceService.GetActivePromotionsForPackageAsync(packageId);
        return Ok(promotions);
    }

    #endregion
}
