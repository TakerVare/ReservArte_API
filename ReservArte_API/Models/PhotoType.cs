namespace ReservArte_API.Models;

/// <summary>
/// Tipos de fotografía para servicios.
/// Permite clasificar las fotos como antes o después del servicio.
/// </summary>
public static class PhotoType
{
    /// <summary>
    /// Fotografía tomada antes del servicio.
    /// </summary>
    public const string Before = "Before";
    
    /// <summary>
    /// Fotografía tomada después del servicio.
    /// </summary>
    public const string After = "After";
    
    public static readonly string[] All = { Before, After };
    
    public static bool IsValid(string? type)
    {
        return !string.IsNullOrEmpty(type) && All.Contains(type);
    }
}
