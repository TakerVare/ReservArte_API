namespace ReservArte_API.Models.DTOs;

public class ReminderConfigurationDtoOut
{
    public Guid Id { get; set; }
    public int ReminderOrder { get; set; }
    public int HoursBeforeAppointment { get; set; }
    public string Channel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid MessageTemplateId { get; set; }
    public string? MessageTemplateName { get; set; }
    public TimeOnly? AllowedSendStartTime { get; set; }
    public TimeOnly? AllowedSendEndTime { get; set; }
}
