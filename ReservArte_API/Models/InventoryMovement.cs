namespace ReservArte_API.Models;

/// <summary>
/// Registro de movimiento de inventario.
/// Cada entrada/salida de stock se registra aquí para trazabilidad.
/// </summary>
public class InventoryMovement
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID del producto afectado.
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Cantidad del movimiento.
    /// Positivo para entradas, negativo para salidas.
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Tipo de movimiento: Purchase, Sale, Adjustment, Waste, ServiceUse, Return.
    /// </summary>
    public string MovementType { get; set; } = Models.MovementType.Adjustment;
    
    /// <summary>
    /// ID de referencia según el tipo (AppointmentId, SaleId, etc.).
    /// </summary>
    public int? ReferenceId { get; set; }
    
    /// <summary>
    /// Notas adicionales sobre el movimiento.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// ID del empleado que registró el movimiento.
    /// </summary>
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties (for DTOs)
    public string? ProductName { get; set; }
    public string? CreatedByName { get; set; }
}
