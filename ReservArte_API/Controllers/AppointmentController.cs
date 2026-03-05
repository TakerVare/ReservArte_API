using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    #region CRUD Básico

    /// <summary>
    /// Obtiene todas las citas
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<IEnumerable<AppointmentDtoToList>>> GetAll()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    /// <summary>
    /// Obtiene una cita por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDtoOut>> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdDetailedAsync(id);
        if (appointment == null)
        {
            return NotFound(new { message = "Cita no encontrada" });
        }
        return Ok(appointment);
    }

    /// <summary>
    /// Crea una nueva cita con validaciones
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDtoOut>> Create([FromBody] AppointmentDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created?.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una cita existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<Appointment>> Update(int id, [FromBody] Appointment appointment)
    {
        var updated = await _appointmentService.UpdateAsync(id, appointment);
        if (updated == null)
        {
            return NotFound(new { message = "Cita no encontrada" });
        }
        return Ok(updated);
    }

    /// <summary>
    /// Elimina una cita
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _appointmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Cita no encontrada" });
        }
        return NoContent();
    }

    #endregion

    #region Consultas de Agenda

    /// <summary>
    /// Obtiene la agenda según los filtros especificados
    /// </summary>
    [HttpGet("agenda")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee},{Roles.Client}")]
    public async Task<ActionResult<AgendaResponseDto>> GetAgenda([FromQuery] AgendaQueryDto query)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var agenda = await _appointmentService.GetAgendaAsync(query);
            return Ok(agenda);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los slots disponibles para un servicio en una fecha
    /// </summary>
    [HttpGet("slots")]
    public async Task<ActionResult<AvailableSlotsResponseDto>> GetAvailableSlots([FromQuery] AvailableSlotsRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var slots = await _appointmentService.GetAvailableSlotsAsync(request);
            return Ok(slots);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las citas de un cliente. Filtros opcionales vía query: startDate, endDate, status, employeeId, servicesDescription.
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<AgendaAppointmentDto>>> GetByCustomer(
        int customerId,
        [FromQuery] CustomerAppointmentsQueryDto? query = null)
    {
        var appointments = await _appointmentService.GetByCustomerIdAsync(customerId, query);
        return Ok(appointments);
    }

    /// <summary>
    /// Obtiene las citas de un empleado en un rango de fechas.
    /// startDate y endDate son opcionales: si no se envían, se usa desde el primer día del mes actual hasta el último del mes siguiente.
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<AgendaAppointmentDto>>> GetByEmployee(
        int employeeId,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = startDate ?? new DateOnly(today.Year, today.Month, 1);
        var to = endDate ?? from.AddMonths(2).AddDays(-1);
        if (to < from)
            return BadRequest(new { message = "endDate debe ser mayor o igual que startDate" });
        var appointments = await _appointmentService.GetByEmployeeIdAsync(employeeId, from, to);
        return Ok(appointments);
    }

    /// <summary>
    /// Obtiene las citas de un usuario (alias para compatibilidad)
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AppointmentDtoToList>>> GetByUserId(int userId)
    {
        var appointments = await _appointmentService.GetByUserIdAsync(userId);
        return Ok(appointments);
    }

    #endregion

    #region Gestión de Estados

    /// <summary>
    /// Actualiza el estado de una cita
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<AppointmentDtoOut>> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _appointmentService.UpdateStatusAsync(id, dto);
            if (updated == null)
            {
                return NotFound(new { message = "Cita no encontrada" });
            }
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una cita
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<AppointmentCancelResultDto>> Cancel(int id, [FromBody] AppointmentCancelDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _appointmentService.CancelAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reagenda una cita
    /// </summary>
    [HttpPut("{id}/reschedule")]
    public async Task<ActionResult<AppointmentDtoOut>> Reschedule(int id, [FromBody] AppointmentRescheduleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _appointmentService.RescheduleAsync(id, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Marca una cita como no-show
    /// </summary>
    [HttpPut("{id}/no-show")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<AppointmentCancelResultDto>> MarkAsNoShow(int id)
    {
        try
        {
            var result = await _appointmentService.MarkAsNoShowAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Validaciones

    /// <summary>
    /// Verifica la disponibilidad de un empleado
    /// </summary>
    [HttpGet("check-availability")]
    public async Task<ActionResult<object>> CheckAvailability(
        [FromQuery] int employeeId,
        [FromQuery] DateOnly date,
        [FromQuery] TimeOnly startTime,
        [FromQuery] TimeOnly endTime,
        [FromQuery] int? excludeAppointmentId = null)
    {
        var isAvailable = await _appointmentService.CheckAvailabilityAsync(
            employeeId, date, startTime, endTime, excludeAppointmentId);

        return Ok(new { 
            employeeId,
            date = date.ToString("yyyy-MM-dd"),
            startTime = startTime.ToString("HH:mm"),
            endTime = endTime.ToString("HH:mm"),
            isAvailable
        });
    }

    /// <summary>
    /// Calcula la penalización por cancelación
    /// </summary>
    [HttpGet("{id}/penalty")]
    public async Task<ActionResult<object>> GetPenalty(int id)
    {
        try
        {
            var penalty = await _appointmentService.CalculatePenaltyAsync(id);
            return Ok(new { appointmentId = id, penaltyAmount = penalty });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
