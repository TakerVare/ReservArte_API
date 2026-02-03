namespace ReservArte_API.Models;

public class ServiceProduct
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public int ProductId { get; set; }
    public decimal? QuantityUsed { get; set; }
    public string? Notes { get; set; }

    // Navigation properties (for DTOs)
    public string? ProductName { get; set; }
    public string? ProductBrand { get; set; }
}
