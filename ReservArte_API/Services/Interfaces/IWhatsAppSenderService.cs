using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

/// <summary>
/// Servicio de envío de mensajes WhatsApp Business (recordatorios).
/// Implementación real: Meta WhatsApp Business API + BSP. Esta interfaz permite mock en desarrollo.
/// </summary>
public interface IWhatsAppSenderService
{
    /// <summary>
    /// Envía un mensaje de plantilla aprobada por Meta (categoría Utilidad).
    /// El número debe tener formato E.164 (ej. +34600000000).
    /// </summary>
    /// <param name="toPhone">Teléfono en E.164.</param>
    /// <param name="templateName">Nombre de la plantilla aprobada en Meta.</param>
    /// <param name="languageCode">Código de idioma (ej. es).</param>
    /// <param name="bodyParameters">Parámetros del cuerpo (variables de la plantilla en orden).</param>
    /// <param name="buttonPayloads">Opcional: payloads para botones (confirmar/cancelar).</param>
    /// <returns>MessageId de WhatsApp y éxito/error.</returns>
    Task<SendWhatsAppResultDto> SendTemplateAsync(
        string toPhone,
        string templateName,
        string languageCode,
        IReadOnlyList<string> bodyParameters,
        IReadOnlyList<string>? buttonPayloads = null);
}
