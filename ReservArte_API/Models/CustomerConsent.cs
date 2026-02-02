namespace ReservArte_API.Models;

/// <summary>
/// Consentimientos y autorizaciones del cliente (RGPD)
/// </summary>
public class CustomerConsent
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool IsGranted { get; set; } = false;
    public DateTime? GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
