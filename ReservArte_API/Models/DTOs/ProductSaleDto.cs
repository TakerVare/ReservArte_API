using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para crear una venta de productos.
/// </summary>
public class ProductSaleDtoIn
{
    /// <summary>
    /// ID del cliente (opcional para ventas anónimas).
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// ID de la cita (si la venta es durante un servicio).
    /// </summary>
    public int? AppointmentId { get; set; }
    
    /// <summary>
    /// Método de pago: Cash, Card, Transfer.
    /// </summary>
    public string? PaymentMethod { get; set; }
    
    public string? Notes { get; set; }
    
    [Required(ErrorMessage = "Debe incluir al menos un producto")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
    public List<ProductSaleItemDtoIn> Items { get; set; } = new();
}

/// <summary>
/// DTO para una línea de venta.
/// </summary>
public class ProductSaleItemDtoIn
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }
    
    /// <summary>
    /// Precio unitario (si se omite, se usa el precio del producto).
    /// </summary>
    public decimal? UnitPrice { get; set; }
}

/// <summary>
/// DTO de respuesta para ventas.
/// </summary>
public class ProductSaleDtoOut
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? AppointmentId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public int? SoldBy { get; set; }
    public string? SoldByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductSaleItemDtoOut> Items { get; set; } = new();
}

/// <summary>
/// DTO de respuesta para línea de venta.
/// </summary>
public class ProductSaleItemDtoOut
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// DTO para filtrar ventas.
/// </summary>
public class ProductSaleFilterDto
{
    public int? CustomerId { get; set; }
    public int? AppointmentId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// DTO de resumen de ventas.
/// </summary>
public class SalesSummaryDtoOut
{
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItemsSold { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
