namespace ReservArte_API.Models.DTOs;

public class ReminderLogDtoOut
{
    public Guid Id { get; set; }
    public int AppointmentId { get; set; }
    public Guid ReminderConfigurationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExternalMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
