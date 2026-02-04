using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReminderController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly IConfiguration _configuration;

    public ReminderController(IReminderService reminderService, IConfiguration configuration)
    {
        _reminderService = reminderService;
        _configuration = configuration;
    }

    #region Configuración de recordatorios

    /// <summary>Obtiene todas las configuraciones de recordatorios.</summary>
    [HttpGet("configurations")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<ReminderConfigurationDtoOut>>> GetConfigurations()
    {
        var list = await _reminderService.GetConfigurationsAsync();
        return Ok(list);
    }

    /// <summary>Obtiene una configuración por ID.</summary>
    [HttpGet("configurations/{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<ReminderConfigurationDtoOut>> GetConfigurationById(Guid id)
    {
        var item = await _reminderService.GetConfigurationByIdAsync(id);
        if (item == null) return NotFound(new { message = "Configuración no encontrada" });
        return Ok(item);
    }

    /// <summary>Crea una configuración de recordatorio.</summary>
    [HttpPost("configurations")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ReminderConfigurationDtoOut>> CreateConfiguration([FromBody] ReminderConfigurationDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _reminderService.CreateConfigurationAsync(dto);
            return created != null ? CreatedAtAction(nameof(GetConfigurationById), new { id = created.Id }, created) : BadRequest();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza una configuración de recordatorio.</summary>
    [HttpPut("configurations/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ReminderConfigurationDtoOut>> UpdateConfiguration(Guid id, [FromBody] ReminderConfigurationDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _reminderService.UpdateConfigurationAsync(id, dto);
            if (updated == null) return NotFound(new { message = "Configuración no encontrada" });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Elimina una configuración de recordatorio.</summary>
    [HttpDelete("configurations/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteConfiguration(Guid id)
    {
        var deleted = await _reminderService.DeleteConfigurationAsync(id);
        if (!deleted) return NotFound(new { message = "Configuración no encontrada" });
        return NoContent();
    }

    #endregion

    #region Plantillas de mensaje

    /// <summary>Obtiene todas las plantillas.</summary>
    [HttpGet("templates")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<MessageTemplateDtoOut>>> GetTemplates()
    {
        var list = await _reminderService.GetTemplatesAsync();
        return Ok(list);
    }

    /// <summary>Obtiene una plantilla por ID.</summary>
    [HttpGet("templates/{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<MessageTemplateDtoOut>> GetTemplateById(Guid id)
    {
        var item = await _reminderService.GetTemplateByIdAsync(id);
        if (item == null) return NotFound(new { message = "Plantilla no encontrada" });
        return Ok(item);
    }

    /// <summary>Crea una plantilla.</summary>
    [HttpPost("templates")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MessageTemplateDtoOut>> CreateTemplate([FromBody] MessageTemplateDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _reminderService.CreateTemplateAsync(dto);
            return created != null ? CreatedAtAction(nameof(GetTemplateById), new { id = created.Id }, created) : BadRequest();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza una plantilla.</summary>
    [HttpPut("templates/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MessageTemplateDtoOut>> UpdateTemplate(Guid id, [FromBody] MessageTemplateDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _reminderService.UpdateTemplateAsync(id, dto);
            if (updated == null) return NotFound(new { message = "Plantilla no encontrada" });
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Elimina una plantilla.</summary>
    [HttpDelete("templates/{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var deleted = await _reminderService.DeleteTemplateAsync(id);
        if (!deleted) return NotFound(new { message = "Plantilla no encontrada" });
        return NoContent();
    }

    #endregion

    #region Logs

    /// <summary>Obtiene el historial de recordatorios enviados para una cita.</summary>
    [HttpGet("appointments/{appointmentId:int}/logs")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<ReminderLogDtoOut>>> GetLogsByAppointmentId(int appointmentId)
    {
        var list = await _reminderService.GetLogsByAppointmentIdAsync(appointmentId);
        return Ok(list);
    }

    #endregion

    #region Confirmar / cancelar por token (público, sin auth)

    /// <summary>Confirma asistencia desde el link del email/WhatsApp (token en query).</summary>
    [HttpGet("confirm")]
    [AllowAnonymous]
    public async Task<ActionResult<ConfirmAppointmentByTokenResultDto>> ConfirmByToken([FromQuery] string token)
    {
        var result = await _reminderService.ConfirmOrCancelByTokenAsync(token);
        return Ok(result);
    }

    /// <summary>Cancela la cita desde el link del email/WhatsApp (token en query).</summary>
    [HttpGet("cancel")]
    [AllowAnonymous]
    public async Task<ActionResult<ConfirmAppointmentByTokenResultDto>> CancelByToken([FromQuery] string token)
    {
        var result = await _reminderService.ConfirmOrCancelByTokenAsync(token);
        return Ok(result);
    }

    #endregion

    #region Preferencias del cliente (opt-in / opt-out)

    /// <summary>Actualiza las preferencias de recordatorios del cliente (consentimiento y canal). El cliente solo puede actualizar las suyas.</summary>
    [HttpPut("customers/{customerId:int}/preferences")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<IActionResult> UpdateCustomerPreferences(int customerId, [FromBody] CustomerReminderPreferencesDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userIdClaim = User.FindFirst("id")?.Value;
        if (User.IsInRole(Roles.Client) && (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId != customerId))
            return Forbid();
        var updated = await _reminderService.UpdateCustomerReminderPreferencesAsync(customerId, dto);
        if (!updated) return NotFound(new { message = "Cliente no encontrado" });
        return NoContent();
    }

    #endregion
}
