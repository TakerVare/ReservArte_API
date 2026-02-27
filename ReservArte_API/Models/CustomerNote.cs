namespace ReservArte_API.Models;

/// <summary>
/// Notas internas sobre un cliente (solo visibles para el personal)
/// </summary>
public class CustomerNote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public string Note { get; set; } = string.Empty;
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Propiedades de navegación (para uso en memoria)
    public string? EmployeeName { get; set; }
}
