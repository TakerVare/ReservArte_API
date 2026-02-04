namespace ReservArte_API.Models;

/// <summary>
/// Estados de una venta de producto.
/// </summary>
public static class SaleStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";
    
    public static readonly string[] All = { Pending, Completed, Cancelled, Refunded };
    
    public static bool IsValid(string? status)
    {
        return !string.IsNullOrEmpty(status) && All.Contains(status);
    }
}

/// <summary>
/// Venta de productos en el local o e-commerce.
/// </summary>
public class ProductSale
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID del cliente (opcional para ventas anónimas).
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// ID de la cita asociada (si la venta se realiza durante un servicio).
    /// </summary>
    public int? AppointmentId { get; set; }
    
    /// <summary>
    /// Total de la venta.
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Estado de la venta.
    /// </summary>
    public string Status { get; set; } = SaleStatus.Completed;
    
    /// <summary>
    /// Método de pago: Cash, Card, Transfer.
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    /// <summary>
    /// Notas de la venta.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Empleado que realizó la venta.
    /// </summary>
    public int? SoldBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public string? CustomerName { get; set; }
    public string? SoldByName { get; set; }
    public List<ProductSaleItem>? Items { get; set; }
}

/// <summary>
/// Línea de detalle de una venta.
/// </summary>
public class ProductSaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    
    // Navigation properties
    public string? ProductName { get; set; }
}
