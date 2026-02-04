namespace ReservArte_API.Models;

/// <summary>
/// Configuración del negocio (singleton - siempre Id=1)
/// Anteriormente "Organization" en arquitectura multi-tenant
/// </summary>
public class BusinessSettings
{
    public int Id { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresApprovalForNewCustomers { get; set; } = false;
    public bool WhitelistOnly { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
