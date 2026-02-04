using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO para registrar un movimiento de inventario.
/// </summary>
public class InventoryMovementDtoIn
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    public int Quantity { get; set; }
    
    [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
    public string MovementType { get; set; } = Models.MovementType.Adjustment;
    
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para registrar una compra (entrada de stock).
/// </summary>
public class PurchaseDtoIn
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }
    
    /// <summary>
    /// Coste unitario de compra (para referencia).
    /// </summary>
    public decimal? UnitCost { get; set; }
    
    public string? Notes { get; set; }
}

/// <summary>
/// DTO para ajuste manual de inventario.
/// </summary>
public class StockAdjustmentDtoIn
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }
    
    /// <summary>
    /// Cantidad a ajustar (positivo para añadir, negativo para reducir).
    /// </summary>
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    public int Quantity { get; set; }
    
    [Required(ErrorMessage = "El motivo del ajuste es obligatorio")]
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta para movimientos de inventario.
/// </summary>
public class InventoryMovementDtoOut
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para filtrar movimientos de inventario.
/// </summary>
public class InventoryMovementFilterDto
{
    public int? ProductId { get; set; }
    public string? MovementType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// DTO para registrar merma de producto.
/// </summary>
public class WasteDtoIn
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }
    
    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }
    
    [Required(ErrorMessage = "El motivo es obligatorio")]
    public string Notes { get; set; } = string.Empty;
}
