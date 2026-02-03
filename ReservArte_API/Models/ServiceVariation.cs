namespace ReservArte_API.Models;

public class ServiceVariation
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; } = 0;
    public int DurationModifier { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
