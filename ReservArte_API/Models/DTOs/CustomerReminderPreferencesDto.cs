using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// Preferencias del cliente para recordatorios (opt-in/opt-out, canal).
/// </summary>
public class CustomerReminderPreferencesDto
{
    /// <summary>Consentimiento para recibir recordatorios de citas.</summary>
    public bool ReminderConsent { get; set; }

    /// <summary>Canal preferido: Email o WhatsApp (debe ser válido según ContactMethod).</summary>
    [Required]
    public string PreferredReminderChannel { get; set; } = Models.ContactMethod.Email;
}
