using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    #region Productos

    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogProductListDtoOut>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var products = await _productService.GetAllAsync(includeInactive);
        return Ok(products);
    }

    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CatalogProductDtoOut>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        return Ok(product);
    }

    /// <summary>
    /// Busca productos por nombre, SKU o marca.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CatalogProductListDtoOut>>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { message = "Debe proporcionar un término de búsqueda" });
        }
        var products = await _productService.SearchAsync(q);
        return Ok(products);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CatalogProductDtoOut>> Create([FromBody] CatalogProductDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CatalogProductDtoOut>> Update(int id, [FromBody] CatalogProductDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _productService.UpdateAsync(id, dto);
        if (product == null)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        return Ok(product);
    }

    /// <summary>
    /// Elimina (desactiva) un producto.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Producto no encontrado" });
        }
        return NoContent();
    }

    #endregion

    #region Categorías

    /// <summary>
    /// Obtiene todas las categorías de productos.
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<ProductCategoryDtoOut>>> GetCategories([FromQuery] bool includeInactive = false)
    {
        var categories = await _productService.GetCategoriesAsync(includeInactive);
        return Ok(categories);
    }

    /// <summary>
    /// Obtiene una categoría por su ID.
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<ActionResult<ProductCategoryDtoOut>> GetCategoryById(int id)
    {
        var category = await _productService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }
        return Ok(category);
    }

    /// <summary>
    /// Obtiene productos de una categoría.
    /// </summary>
    [HttpGet("categories/{id}/products")]
    public async Task<ActionResult<IEnumerable<CatalogProductListDtoOut>>> GetByCategory(int id)
    {
        var products = await _productService.GetByCategoryAsync(id);
        return Ok(products);
    }

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<ProductCategoryDtoOut>> CreateCategory([FromBody] ProductCategoryDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _productService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<ProductCategoryDtoOut>> UpdateCategory(int id, [FromBody] ProductCategoryDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await _productService.UpdateCategoryAsync(id, dto);
        if (category == null)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }
        return Ok(category);
    }

    /// <summary>
    /// Elimina (desactiva) una categoría.
    /// </summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _productService.DeleteCategoryAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Categoría no encontrada" });
        }
        return NoContent();
    }

    #endregion

    #region Alertas de Stock

    /// <summary>
    /// Obtiene productos con stock bajo.
    /// </summary>
    [HttpGet("alerts/low-stock")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<StockAlertDtoOut>>> GetLowStockAlerts()
    {
        var alerts = await _productService.GetLowStockAlertsAsync();
        return Ok(alerts);
    }

    #endregion
}
