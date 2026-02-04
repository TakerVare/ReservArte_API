namespace ReservArte_API.Models;

/// <summary>
/// Entidad de producto para catálogo e inventario.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    
    /// <summary>
    /// Precio de venta del producto.
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Cantidad actual en stock.
    /// </summary>
    public int Stock { get; set; } = 0;
    
    /// <summary>
    /// Cantidad mínima de stock para generar alerta.
    /// </summary>
    public int MinStockAlert { get; set; } = 5;
    
    /// <summary>
    /// URL de la imagen del producto.
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// ID de la categoría del producto.
    /// </summary>
    public int? CategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties (for DTOs)
    public string? CategoryName { get; set; }
}
