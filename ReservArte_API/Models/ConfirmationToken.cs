namespace ReservArte_API.Models;

/// <summary>
/// Token UUID de un solo uso para confirmar o cancelar cita desde link (email/WhatsApp).
/// Almacenado en BD para poder revocarlo y usarlo una sola vez.
/// </summary>
public class ConfirmationToken
{
    /// <summary>Token UUID (clave).</summary>
    public string Token { get; set; } = string.Empty;
    public int AppointmentId { get; set; }
    /// <summary>confirm | cancel</summary>
    public string Action { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
