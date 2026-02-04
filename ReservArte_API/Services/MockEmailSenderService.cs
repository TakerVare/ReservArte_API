using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

/// <summary>
/// Implementación MOCK del envío de emails. Simula éxito y devuelve un MessageId ficticio.
/// 
/// ========== IMPLEMENTACIÓN REAL CON AMAZON SES ==========
/// 1. Añadir paquete: AWSSDK.SimpleEmail (NuGet)
/// 2. Configuración en appsettings.json:
///    "SES": {
///      "Region": "eu-west-1",
///      "FromEmail": "noreply@tudominio.com",
///      "FromName": "ReservArte",
///      "ReplyTo": "contacto@tudominio.com"
///    }
///    Credenciales: usar IAM (Access Key / Secret) o rol en EC2/ECS. No guardar secretos en appsettings.
/// 3. Registrar en Program.cs:
///    builder.Services.AddDefaultAWSOptions(config.GetAWSOptions());
///    builder.Services.AddAWSService&lt;IAmazonSimpleEmailService&gt;();
///    builder.Services.AddScoped&lt;IEmailSenderService, SesEmailSenderService&gt;();
/// 4. Implementación real (resumen):
///    - Crear SendEmailRequest con Source (From), Destination (ToAddresses), Message (Subject + Body).
///    - Body: crear tanto HtmlPart como TextPart para multipart/alternative (mejor deliverability).
///    - Opcional: SetConfigurationSetName para tracking (open/click) si configuras un Configuration Set en SES.
///    - Opcional: añadir headers (List-Unsubscribe, X-Entity-Ref-ID para id interno).
///    - Llamar a _ses.SendEmailAsync(request). Response.MessageId es el ExternalMessageId.
///    - Para tracking de bounces/complaints: configurar SNS topic en SES y un endpoint que actualice ReminderLog.
///    - Para open tracking: usar Configuration Set con Event Destination (SNS o CloudWatch), y pixel 1x1 en HTML.
/// 5. Producción: verificar dominio en SES, solicitar salida de sandbox si aplica, y cumplir políticas de envío.
/// </summary>
public class MockEmailSenderService : IEmailSenderService
{
    public Task<SendEmailResultDto> SendAsync(string to, string subject, string htmlBody, string plainTextBody, string? replyTo = null)
    {
        // Simular envío correcto
        return Task.FromResult(new SendEmailResultDto
        {
            Success = true,
            ExternalMessageId = $"mock-ses-{Guid.NewGuid():N}"
        });
    }
}
