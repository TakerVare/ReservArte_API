namespace ReservArte_API.Models;

/// <summary>
/// Tipos de movimiento de inventario.
/// </summary>
public static class MovementType
{
    /// <summary>
    /// Compra o entrada de stock.
    /// </summary>
    public const string Purchase = "Purchase";
    
    /// <summary>
    /// Venta de producto.
    /// </summary>
    public const string Sale = "Sale";
    
    /// <summary>
    /// Ajuste manual de inventario.
    /// </summary>
    public const string Adjustment = "Adjustment";
    
    /// <summary>
    /// Merma o desperdicio.
    /// </summary>
    public const string Waste = "Waste";
    
    /// <summary>
    /// Uso en un servicio (vinculado a ServiceProduct).
    /// </summary>
    public const string ServiceUse = "ServiceUse";
    
    /// <summary>
    /// Devolución de producto.
    /// </summary>
    public const string Return = "Return";
    
    public static readonly string[] All = 
    { 
        Purchase, Sale, Adjustment, Waste, ServiceUse, Return 
    };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
    
    /// <summary>
    /// Indica si el tipo de movimiento suma stock (entrada).
    /// </summary>
    public static bool IsIncoming(string type)
    {
        return type == Purchase || type == Return || type == Adjustment;
    }
}
