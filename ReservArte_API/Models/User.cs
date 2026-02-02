namespace ReservArte_API.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Nombre completo del usuario (FirstName + LastName)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();
}