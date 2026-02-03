namespace ReservArte_API.Models;

public class ServicePackage
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (for DTOs)
    public List<ServicePackageItem>? Items { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int? TotalDurationMinutes { get; set; }
}
