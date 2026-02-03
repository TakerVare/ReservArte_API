namespace ReservArte_API.Models;

public class ServicePricing
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string EmployeeLevel { get; set; } = EmployeeLevels.Junior;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Employee experience levels for pricing
/// </summary>
public static class EmployeeLevels
{
    public const string Junior = "Junior";
    public const string Senior = "Senior";
    public const string Expert = "Expert";

    public static bool IsValid(string level)
    {
        return level == Junior || level == Senior || level == Expert;
    }

    public static IEnumerable<string> GetAll()
    {
        return new[] { Junior, Senior, Expert };
    }
}
