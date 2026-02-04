using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

/// <summary>
/// Servicio de envío de emails transaccionales (recordatorios).
/// Implementación real: Amazon SES. Esta interfaz permite mock en desarrollo.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Envía un email con contenido HTML y texto plano.
    /// </summary>
    /// <param name="to">Email del destinatario.</param>
    /// <param name="subject">Asunto.</param>
    /// <param name="htmlBody">Cuerpo HTML (responsive).</param>
    /// <param name="plainTextBody">Cuerpo en texto plano (alternativo).</param>
    /// <param name="replyTo">Opcional.</param>
    /// <returns>MessageId del proveedor y éxito/error.</returns>
    Task<SendEmailResultDto> SendAsync(string to, string subject, string htmlBody, string plainTextBody, string? replyTo = null);
}
