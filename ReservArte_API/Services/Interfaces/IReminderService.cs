using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IReminderService
{
    #region Configuración de recordatorios
    Task<IEnumerable<ReminderConfigurationDtoOut>> GetConfigurationsAsync();
    Task<ReminderConfigurationDtoOut?> GetConfigurationByIdAsync(Guid id);
    Task<ReminderConfigurationDtoOut?> CreateConfigurationAsync(ReminderConfigurationDtoIn dto);
    Task<ReminderConfigurationDtoOut?> UpdateConfigurationAsync(Guid id, ReminderConfigurationDtoIn dto);
    Task<bool> DeleteConfigurationAsync(Guid id);
    #endregion

    #region Plantillas de mensaje
    Task<IEnumerable<MessageTemplateDtoOut>> GetTemplatesAsync();
    Task<MessageTemplateDtoOut?> GetTemplateByIdAsync(Guid id);
    Task<MessageTemplateDtoOut?> CreateTemplateAsync(MessageTemplateDtoIn dto);
    Task<MessageTemplateDtoOut?> UpdateTemplateAsync(Guid id, MessageTemplateDtoIn dto);
    Task<bool> DeleteTemplateAsync(Guid id);
    #endregion

    #region Logs y procesamiento
    Task<IEnumerable<ReminderLogDtoOut>> GetLogsByAppointmentIdAsync(int appointmentId);
    /// <summary>Procesa y envía los recordatorios que tocan en este momento (llamado por el job en background).</summary>
    Task ProcessDueRemindersAsync();
    #endregion

    #region Confirmación / cancelación por token
    /// <summary>Genera tokens y URLs de confirmar/cancelar para una cita (para incluir en email/WhatsApp).</summary>
    Task<(string confirmUrl, string cancelUrl, DateTime confirmExpiresAt, DateTime cancelExpiresAt)> GenerateConfirmationUrlsAsync(int appointmentId, string baseUrl);
    Task<ConfirmAppointmentByTokenResultDto> ConfirmOrCancelByTokenAsync(string token);
    #endregion

    #region Preferencias del cliente (opt-in/opt-out)
    Task<bool> UpdateCustomerReminderPreferencesAsync(int customerId, CustomerReminderPreferencesDto dto);
    #endregion
}
