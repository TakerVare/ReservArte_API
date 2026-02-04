namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de respuesta para productos.
/// </summary>
public class CatalogProductDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinStockAlert { get; set; }
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Indica si el stock está por debajo del mínimo.
    /// </summary>
    public bool IsLowStock => Stock <= MinStockAlert;
}

/// <summary>
/// DTO simplificado para listados de productos.
/// </summary>
public class CatalogProductListDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
}

/// <summary>
/// DTO de respuesta para categorías de productos.
/// </summary>
public class ProductCategoryDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>
/// DTO para alertas de stock bajo.
/// </summary>
public class StockAlertDtoOut
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockAlert { get; set; }
    public int Deficit => MinStockAlert - CurrentStock;
}
