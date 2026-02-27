namespace ReservArte_API.Models;

/// <summary>
/// Entidad principal de cliente - hereda de User
/// </summary>
public class Customer : User
{
    public DateTime? BirthDate { get; set; }
    public string Category { get; set; } = CustomerCategory.New;
    public int LoyaltyPoints { get; set; } = 0;
    public bool IsBlocked { get; set; } = false;
    public string? BlockedReason { get; set; }
    public string PreferredContactMethod { get; set; } = ContactMethod.Email;
    public bool MarketingConsent { get; set; } = false;
    /// <summary>Consentimiento para recibir recordatorios de citas (opt-in por defecto).</summary>
    public bool ReminderConsent { get; set; } = true;
    /// <summary>Soft delete: false cuando el registro está "eliminado".</summary>
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
