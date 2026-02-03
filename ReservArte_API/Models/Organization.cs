namespace ReservArte_API.Models;

/// <summary>
/// Entidad de configuración del negocio (single-tenant).
/// Almacena la información general de la empresa como nombre, dirección, contacto, etc.
/// En arquitectura single-tenant, solo existirá un registro en esta tabla.
/// </summary>
public class Organization
{
    public int Id { get; set; }
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
