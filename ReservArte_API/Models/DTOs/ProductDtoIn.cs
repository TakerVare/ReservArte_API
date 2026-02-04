using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para creación/actualización de productos del catálogo.
/// </summary>
public class CatalogProductDtoIn
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? Brand { get; set; }
    
    public string? Sku { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
    public decimal Price { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
    public int Stock { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
    public int MinStockAlert { get; set; } = 5;
    
    public string? ImageUrl { get; set; }
    
    public int? CategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO para creación/actualización de categorías de productos.
/// </summary>
public class ProductCategoryDtoIn
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}
