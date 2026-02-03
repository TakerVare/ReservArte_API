namespace ReservArte_API.Models;

public class ServicePromotion
{
    public int Id { get; set; }
    public int? ServiceId { get; set; }
    public int? ServicePackageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; } = 0;
    public decimal? DiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsSeasonalService { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (for DTOs)
    public string? ServiceName { get; set; }
    public string? PackageName { get; set; }

    // Computed property
    public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}
