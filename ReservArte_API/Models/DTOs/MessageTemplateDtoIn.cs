using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

public class MessageTemplateDtoIn
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = Models.MessageTemplateType.EmailReminder;

    [StringLength(500)]
    public string? Subject { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;

    [StringLength(10)]
    public string Language { get; set; } = "es-ES";
}
