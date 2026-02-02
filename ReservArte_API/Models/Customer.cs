namespace ReservArte_API.Models;

/// <summary>
/// Entidad principal de cliente - hereda de User
/// </summary>
public class Customer : User
{
    public int OrganizationId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Category { get; set; } = CustomerCategory.New;
    public int LoyaltyPoints { get; set; } = 0;
    public bool IsBlocked { get; set; } = false;
    public string? BlockedReason { get; set; }
    public string PreferredContactMethod { get; set; } = ContactMethod.Email;
    public bool MarketingConsent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
