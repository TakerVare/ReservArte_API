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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
