using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

/// <summary>
/// Implementación MOCK del envío de WhatsApp. Simula éxito y devuelve un MessageId ficticio.
/// 
/// ========== IMPLEMENTACIÓN REAL CON WHATSAPP BUSINESS API ==========
/// 1. Flujo general:
///    - Meta ofrece la API; para usarla necesitas un BSP (Business Solution Provider) como Twilio, MessageBird, 360dialog, etc.
///    - Alternativa: Cloud API de Meta directamente (https://developers.facebook.com/docs/whatsapp/cloud-api).
/// 2. Requisitos:
///    - Cuenta de negocio Meta (Business Manager).
///    - Número de teléfono dedicado para WhatsApp Business (no el personal).
///    - Plantillas de mensaje aprobadas por Meta (categoría "Utilidad" para recordatorios, ~€0.01/mensaje en España).
///    - Cliente debe haber dado opt-in (iniciado conversación o aceptado recibir mensajes).
/// 3. Configuración en appsettings.json (ejemplo con Cloud API):
///    "WhatsApp": {
///      "PhoneNumberId": "...",
///      "AccessToken": "...",  // En producción usar secret manager / Key Vault
///      "ApiVersion": "v18.0"
///    }
/// 4. Envío de plantilla (POST a https://graph.facebook.com/{version}/{PhoneNumberId}/messages):
///    Body JSON: {
///      "messaging_product": "whatsapp",
///      "to": "34600000000",  // sin +
///      "type": "template",
///      "template": {
///        "name": "recordatorio_cita",
///        "language": { "code": "es" },
///        "components": [
///          { "type": "body", "parameters": [ { "type": "text", "text": "Juan" }, { "type": "text", "text": "15/02/2025 10:00" } ] },
///          { "type": "button", "sub_type": "url", "index": 0, "parameters": [ { "type": "payload", "payload": "confirm_123" } ] }
///        ]
///      }
///    }
///    Respuesta incluye messages[0].id → ExternalMessageId para ReminderLog.
/// 5. Respuesta dentro de 24h al cliente es gratuita; mensajes fuera de ventana deben usar plantillas (de pago).
/// 6. Gestión de lista de supresión: guardar bounces y quejas y no volver a enviar a ese número.
/// 7. Registrar en Program.cs: builder.Services.AddScoped&lt;IWhatsAppSenderService, WhatsAppCloudApiSenderService&gt;();
/// </summary>
public class MockWhatsAppSenderService : IWhatsAppSenderService
{
    public Task<SendWhatsAppResultDto> SendTemplateAsync(
        string toPhone,
        string templateName,
        string languageCode,
        IReadOnlyList<string> bodyParameters,
        IReadOnlyList<string>? buttonPayloads = null)
    {
        return Task.FromResult(new SendWhatsAppResultDto
        {
            Success = true,
            ExternalMessageId = $"mock-wamid-{Guid.NewGuid():N}"
        });
    }
}
