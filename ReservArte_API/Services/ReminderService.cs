using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class ReminderService : IReminderService
{
    private readonly IReminderConfigurationRepository _configRepo;
    private readonly IMessageTemplateRepository _templateRepo;
    private readonly IReminderLogRepository _logRepo;
    private readonly IConfirmationTokenRepository _tokenRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IEmailSenderService _emailSender;
    private readonly IWhatsAppSenderService _whatsAppSender;
    private readonly IConfiguration _configuration;
    private static readonly TimeSpan ReminderWindowTolerance = TimeSpan.FromMinutes(10);
    private static readonly int TokenValidityHours = 168; // 7 días

    public ReminderService(
        IReminderConfigurationRepository configRepo,
        IMessageTemplateRepository templateRepo,
        IReminderLogRepository logRepo,
        IConfirmationTokenRepository tokenRepo,
        IAppointmentRepository appointmentRepo,
        ICustomerRepository customerRepo,
        IEmailSenderService emailSender,
        IWhatsAppSenderService whatsAppSender,
        IConfiguration configuration)
    {
        _configRepo = configRepo;
        _templateRepo = templateRepo;
        _logRepo = logRepo;
        _tokenRepo = tokenRepo;
        _appointmentRepo = appointmentRepo;
        _customerRepo = customerRepo;
        _emailSender = emailSender;
        _whatsAppSender = whatsAppSender;
        _configuration = configuration;
    }

    #region Configuración

    public async Task<IEnumerable<ReminderConfigurationDtoOut>> GetConfigurationsAsync()
    {
        return await _configRepo.GetAllAsync();
    }

    public async Task<ReminderConfigurationDtoOut?> GetConfigurationByIdAsync(Guid id)
    {
        var list = (await _configRepo.GetAllAsync()).ToList();
        return list.FirstOrDefault(c => c.Id == id);
    }

    public async Task<ReminderConfigurationDtoOut?> CreateConfigurationAsync(ReminderConfigurationDtoIn dto)
    {
        if (!ReminderChannel.IsValid(dto.Channel))
            throw new InvalidOperationException("Canal de recordatorio no válido");
        var template = await _templateRepo.GetByIdAsync(dto.MessageTemplateId);
        if (template == null)
            throw new InvalidOperationException("Plantilla no encontrada");
        var config = new ReminderConfiguration
        {
            Id = Guid.NewGuid(),
            ReminderOrder = dto.ReminderOrder,
            HoursBeforeAppointment = dto.HoursBeforeAppointment,
            Channel = dto.Channel,
            IsActive = dto.IsActive,
            MessageTemplateId = dto.MessageTemplateId,
            AllowedSendStartTime = dto.AllowedSendStartTime,
            AllowedSendEndTime = dto.AllowedSendEndTime
        };
        var created = await _configRepo.CreateAsync(config);
        return created != null ? (await _configRepo.GetAllAsync()).FirstOrDefault(c => c.Id == created.Id) : null;
    }

    public async Task<ReminderConfigurationDtoOut?> UpdateConfigurationAsync(Guid id, ReminderConfigurationDtoIn dto)
    {
        if (!ReminderChannel.IsValid(dto.Channel))
            throw new InvalidOperationException("Canal de recordatorio no válido");
        var existing = await _configRepo.GetByIdAsync(id);
        if (existing == null) return null;
        var template = await _templateRepo.GetByIdAsync(dto.MessageTemplateId);
        if (template == null)
            throw new InvalidOperationException("Plantilla no encontrada");
        existing.ReminderOrder = dto.ReminderOrder;
        existing.HoursBeforeAppointment = dto.HoursBeforeAppointment;
        existing.Channel = dto.Channel;
        existing.IsActive = dto.IsActive;
        existing.MessageTemplateId = dto.MessageTemplateId;
        existing.AllowedSendStartTime = dto.AllowedSendStartTime;
        existing.AllowedSendEndTime = dto.AllowedSendEndTime;
        var updated = await _configRepo.UpdateAsync(id, existing);
        return updated != null ? (await _configRepo.GetAllAsync()).FirstOrDefault(c => c.Id == id) : null;
    }

    public async Task<bool> DeleteConfigurationAsync(Guid id)
    {
        return await _configRepo.DeleteAsync(id);
    }

    #endregion

    #region Plantillas

    public async Task<IEnumerable<MessageTemplateDtoOut>> GetTemplatesAsync()
    {
        var list = await _templateRepo.GetAllAsync();
        return list.Select(t => new MessageTemplateDtoOut
        {
            Id = t.Id,
            Name = t.Name,
            Type = t.Type,
            Subject = t.Subject,
            Body = t.Body,
            Language = t.Language
        });
    }

    public async Task<MessageTemplateDtoOut?> GetTemplateByIdAsync(Guid id)
    {
        var t = await _templateRepo.GetByIdAsync(id);
        return t == null ? null : new MessageTemplateDtoOut
        {
            Id = t.Id,
            Name = t.Name,
            Type = t.Type,
            Subject = t.Subject,
            Body = t.Body,
            Language = t.Language
        };
    }

    public async Task<MessageTemplateDtoOut?> CreateTemplateAsync(MessageTemplateDtoIn dto)
    {
        if (!MessageTemplateType.IsValid(dto.Type))
            throw new InvalidOperationException("Tipo de plantilla no válido");
        var template = new MessageTemplate
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Type = dto.Type,
            Subject = dto.Subject,
            Body = dto.Body,
            Language = dto.Language
        };
        var created = await _templateRepo.CreateAsync(template);
        return created != null ? new MessageTemplateDtoOut
        {
            Id = created.Id,
            Name = created.Name,
            Type = created.Type,
            Subject = created.Subject,
            Body = created.Body,
            Language = created.Language
        } : null;
    }

    public async Task<MessageTemplateDtoOut?> UpdateTemplateAsync(Guid id, MessageTemplateDtoIn dto)
    {
        if (!MessageTemplateType.IsValid(dto.Type))
            throw new InvalidOperationException("Tipo de plantilla no válido");
        var existing = await _templateRepo.GetByIdAsync(id);
        if (existing == null) return null;
        existing.Name = dto.Name;
        existing.Type = dto.Type;
        existing.Subject = dto.Subject;
        existing.Body = dto.Body;
        existing.Language = dto.Language;
        var updated = await _templateRepo.UpdateAsync(id, existing);
        return updated != null ? new MessageTemplateDtoOut
        {
            Id = updated.Id,
            Name = updated.Name,
            Type = updated.Type,
            Subject = updated.Subject,
            Body = updated.Body,
            Language = updated.Language
        } : null;
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        return await _templateRepo.DeleteAsync(id);
    }

    #endregion

    #region Logs y procesamiento

    public async Task<IEnumerable<ReminderLogDtoOut>> GetLogsByAppointmentIdAsync(int appointmentId)
    {
        return await _logRepo.GetByAppointmentIdAsync(appointmentId);
    }

    public async Task ProcessDueRemindersAsync()
    {
        var configs = (await _configRepo.GetActiveOrderedAsync()).ToList();
        if (configs.Count == 0) return;

        var baseUrl = _configuration.GetSection(ReminderSettings.SectionName)[nameof(ReminderSettings.BaseUrl)] ?? "https://localhost:7000";
        var now = DateTime.UtcNow;

        foreach (var config in configs)
        {
            var from = now.AddHours(config.HoursBeforeAppointment) - ReminderWindowTolerance;
            var to = now.AddHours(config.HoursBeforeAppointment) + ReminderWindowTolerance;
            var appointments = (await _appointmentRepo.GetAppointmentsInDateTimeWindowAsync(from, to)).ToList();

            foreach (var appointment in appointments)
            {
                await TrySendReminderForAppointmentAsync(appointment, config, baseUrl);
            }
        }

        await _tokenRepo.DeleteExpiredAsync();
    }

    private async Task TrySendReminderForAppointmentAsync(Appointment appointment, ReminderConfiguration config, string baseUrl)
    {
        var customer = await _customerRepo.GetByIdAsync(appointment.CustomerId);
        if (customer == null || !customer.ReminderConsent) return;

        var channelsToSend = GetChannelsForConfig(config.Channel, customer.PreferredContactMethod);
        var nowLocal = DateTime.Now;
        var sendStart = config.AllowedSendStartTime ?? TimeOnly.MinValue;
        var sendEnd = config.AllowedSendEndTime ?? TimeOnly.MaxValue;
        var currentTime = TimeOnly.FromDateTime(nowLocal);
        if (currentTime < sendStart || currentTime > sendEnd)
            return;

        var template = await _templateRepo.GetByIdAsync(config.MessageTemplateId);
        if (template == null) return;

        var detailed = await _appointmentRepo.GetByIdDetailedAsync(appointment.Id);
        if (detailed == null) return;

        var (confirmUrl, cancelUrl, _, _) = await GenerateConfirmationUrlsAsync(appointment.Id, baseUrl);
        var serviceNames = detailed.Services != null ? string.Join(", ", detailed.Services.Select(s => s.ServiceName)) : "";
        var businessAddress = _configuration.GetSection(ReminderSettings.SectionName)[nameof(ReminderSettings.BusinessAddress)] ?? "";

        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["customerName"] = detailed.CustomerName ?? "",
            ["appointmentDate"] = detailed.AppointmentDate,
            ["appointmentTime"] = detailed.StartTime.ToString("HH:mm"),
            ["serviceName"] = serviceNames,
            ["employeeName"] = detailed.EmployeeName ?? "",
            ["businessAddress"] = businessAddress,
            ["confirmUrl"] = confirmUrl,
            ["cancelUrl"] = cancelUrl
        };

        var body = ReplaceTemplateVariables(template.Body, variables);
        var subject = ReplaceTemplateVariables(template.Subject ?? "", variables);

        foreach (var channel in channelsToSend)
        {
            if (await _logRepo.WasReminderSentAsync(appointment.Id, config.Id, channel))
                continue;

            if (channel == ReminderChannel.Email)
            {
                if (string.IsNullOrEmpty(customer.Email)) continue;
                var result = await _emailSender.SendAsync(customer.Email, subject, body, body);
                await _logRepo.CreateAsync(new ReminderLog
                {
                    AppointmentId = appointment.Id,
                    ReminderConfigurationId = config.Id,
                    Channel = channel,
                    SentAt = DateTime.UtcNow,
                    Status = result.Success ? ReminderLogStatus.Sent : ReminderLogStatus.Failed,
                    ExternalMessageId = result.ExternalMessageId,
                    ErrorMessage = result.ErrorMessage
                });
            }
            else if (channel == ReminderChannel.WhatsApp)
            {
                if (string.IsNullOrEmpty(customer.Phone)) continue;
                var phone = NormalizePhoneForWhatsApp(customer.Phone);
                var bodyParams = new List<string> { detailed.CustomerName ?? "", $"{detailed.AppointmentDate} {detailed.StartTime:HH:mm}", serviceNames };
                var result = await _whatsAppSender.SendTemplateAsync(phone, "recordatorio_cita", "es", bodyParams);
                await _logRepo.CreateAsync(new ReminderLog
                {
                    AppointmentId = appointment.Id,
                    ReminderConfigurationId = config.Id,
                    Channel = channel,
                    SentAt = DateTime.UtcNow,
                    Status = result.Success ? ReminderLogStatus.Sent : ReminderLogStatus.Failed,
                    ExternalMessageId = result.ExternalMessageId,
                    ErrorMessage = result.ErrorMessage
                });
            }
        }
    }

    private static List<string> GetChannelsForConfig(string configChannel, string customerPreferred)
    {
        if (configChannel == ReminderChannel.Both)
            return new List<string> { ReminderChannel.Email, ReminderChannel.WhatsApp };
        if (configChannel == ReminderChannel.Email) return new List<string> { ReminderChannel.Email };
        if (configChannel == ReminderChannel.WhatsApp) return new List<string> { ReminderChannel.WhatsApp };
        return new List<string>();
    }

    private static string ReplaceTemplateVariables(string text, IReadOnlyDictionary<string, string> variables)
    {
        foreach (var kv in variables)
        {
            text = text.Replace("{{" + kv.Key + "}}", kv.Value, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    private static string NormalizePhoneForWhatsApp(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("34") && digits.Length >= 11) return digits;
        if (!digits.StartsWith("34") && digits.Length >= 9) return "34" + digits;
        return digits;
    }

    #endregion

    #region Confirmación por token

    public async Task<(string confirmUrl, string cancelUrl, DateTime confirmExpiresAt, DateTime cancelExpiresAt)> GenerateConfirmationUrlsAsync(int appointmentId, string baseUrl)
    {
        var expiresAt = DateTime.UtcNow.AddHours(TokenValidityHours);
        var confirmToken = Guid.NewGuid().ToString("N");
        var cancelToken = Guid.NewGuid().ToString("N");

        await _tokenRepo.CreateAsync(new ConfirmationToken
        {
            Token = confirmToken,
            AppointmentId = appointmentId,
            Action = ConfirmationTokenAction.Confirm,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        await _tokenRepo.CreateAsync(new ConfirmationToken
        {
            Token = cancelToken,
            AppointmentId = appointmentId,
            Action = ConfirmationTokenAction.Cancel,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        var baseUrlTrim = baseUrl.TrimEnd('/');
        return (
            $"{baseUrlTrim}/api/reminder/confirm?token={confirmToken}",
            $"{baseUrlTrim}/api/reminder/cancel?token={cancelToken}",
            expiresAt,
            expiresAt
        );
    }

    public async Task<ConfirmAppointmentByTokenResultDto> ConfirmOrCancelByTokenAsync(string token)
    {
        var ct = await _tokenRepo.GetByTokenAsync(token);
        if (ct == null)
            return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "Token no encontrado" };
        if (ct.UsedAt.HasValue)
            return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "El enlace ya ha sido utilizado" };
        if (ct.ExpiresAt < DateTime.UtcNow)
            return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "El enlace ha caducado" };

        var appointment = await _appointmentRepo.GetByIdAsync(ct.AppointmentId);
        if (appointment == null)
            return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "Cita no encontrada" };
        if (!Status.Active.Contains(appointment.Status))
            return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "La cita ya no está activa" };

        if (ct.Action == ConfirmationTokenAction.Confirm)
        {
            await _appointmentRepo.UpdateStatusAsync(ct.AppointmentId, Status.Confirmed);
            await _tokenRepo.MarkAsUsedAsync(token);
            return new ConfirmAppointmentByTokenResultDto
            {
                Success = true,
                Message = "Asistencia confirmada",
                AppointmentId = ct.AppointmentId,
                NewStatus = Status.Confirmed
            };
        }

        if (ct.Action == ConfirmationTokenAction.Cancel)
        {
            await _appointmentRepo.CancelAsync(ct.AppointmentId, "Cancelado por el cliente desde el enlace de recordatorio", appointment.CustomerId, CancelledByType.Customer);
            await _tokenRepo.MarkAsUsedAsync(token);
            return new ConfirmAppointmentByTokenResultDto
            {
                Success = true,
                Message = "Cita cancelada",
                AppointmentId = ct.AppointmentId,
                NewStatus = Status.CancelledByCustomer
            };
        }

        return new ConfirmAppointmentByTokenResultDto { Success = false, Message = "Acción no válida" };
    }

    #endregion

    #region Preferencias cliente

    public async Task<bool> UpdateCustomerReminderPreferencesAsync(int customerId, CustomerReminderPreferencesDto dto)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId);
        if (customer == null) return false;
        if (!ContactMethod.IsValid(dto.PreferredReminderChannel))
            dto.PreferredReminderChannel = ContactMethod.Email;
        customer.ReminderConsent = dto.ReminderConsent;
        customer.PreferredContactMethod = dto.PreferredReminderChannel;
        var updated = await _customerRepo.UpdateAsync(customerId, customer);
        return updated != null;
    }

    #endregion
}
