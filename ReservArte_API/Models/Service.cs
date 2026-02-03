namespace ReservArte_API.Models;

public class Service
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public int? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresAllergyTest { get; set; } = false;
    public int AllergyTestHoursBefore { get; set; } = 48;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (for DTOs)
    public string? CategoryName { get; set; }
    public List<ServiceVariation>? Variations { get; set; }
    public List<ServicePricing>? Pricings { get; set; }
    public List<ServiceProduct>? Products { get; set; }
}
