namespace ReservArte_API.Models;

/// <summary>
/// Alergias registradas de un cliente (historial médico relevante)
/// </summary>
public class CustomerAllergy
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string AllergyDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = AllergySeverity.Low;
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
