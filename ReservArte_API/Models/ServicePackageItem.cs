namespace ReservArte_API.Models;

public class ServicePackageItem
{
    public int Id { get; set; }
    public int ServicePackageId { get; set; }
    public int ServiceId { get; set; }
    public int Order { get; set; } = 0;

    // Navigation properties (for DTOs)
    public string? ServiceName { get; set; }
    public decimal? ServicePrice { get; set; }
    public int? ServiceDurationMinutes { get; set; }
}
