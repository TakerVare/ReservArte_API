using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

public class ReminderConfigurationDtoIn
{
    [Required]
    [Range(1, 10, ErrorMessage = "El orden debe estar entre 1 y 10")]
    public int ReminderOrder { get; set; }

    [Required]
    [Range(0, 720, ErrorMessage = "Horas antes entre 0 y 720 (30 días)")]
    public int HoursBeforeAppointment { get; set; }

    [Required]
    public string Channel { get; set; } = Models.ReminderChannel.Email;

    public bool IsActive { get; set; } = true;

    [Required]
    public Guid MessageTemplateId { get; set; }

    /// <summary>Hora mínima para enviar (ej. 08:00). Null = sin límite.</summary>
    public TimeOnly? AllowedSendStartTime { get; set; }

    /// <summary>Hora máxima para enviar (ej. 22:00). Null = sin límite.</summary>
    public TimeOnly? AllowedSendEndTime { get; set; }
}
