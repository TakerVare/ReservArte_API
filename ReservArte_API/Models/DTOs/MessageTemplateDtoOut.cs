namespace ReservArte_API.Models.DTOs;

public class MessageTemplateDtoOut
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
}
