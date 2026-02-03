using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WaitingListController : ControllerBase
{
    private readonly IWaitingListService _waitingListService;

    public WaitingListController(IWaitingListService waitingListService)
    {
        _waitingListService = waitingListService;
    }

    /// <summary>
    /// Obtiene una entrada de lista de espera por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<WaitingListDtoOut>> GetById(int id)
    {
        var entry = await _waitingListService.GetByIdAsync(id);
        if (entry == null)
        {
            return NotFound(new { message = "Entrada no encontrada en la lista de espera" });
        }
        return Ok(entry);
    }

    /// <summary>
    /// Obtiene la lista de espera de un cliente
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<WaitingListDtoOut>>> GetByCustomer(int customerId)
    {
        var entries = await _waitingListService.GetByCustomerAsync(customerId);
        return Ok(entries);
    }

    /// <summary>
    /// Obtiene la lista de espera de una organización
    /// </summary>
    [HttpGet("organization/{organizationId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<WaitingListDtoOut>>> GetByOrganization(int organizationId)
    {
        var entries = await _waitingListService.GetByOrganizationAsync(organizationId);
        return Ok(entries);
    }

    /// <summary>
    /// Añade a un cliente a la lista de espera
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WaitingListDtoOut>> Add([FromBody] WaitingListDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _waitingListService.AddToWaitingListAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created?.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina a un cliente de la lista de espera
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var deleted = await _waitingListService.RemoveFromWaitingListAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Entrada no encontrada en la lista de espera" });
        }
        return NoContent();
    }

    /// <summary>
    /// Notifica a los clientes que coinciden con un slot liberado
    /// </summary>
    [HttpPost("notify")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<WaitingListDtoOut>>> NotifyMatchingCustomers(
        [FromQuery] int organizationId,
        [FromQuery] int serviceId,
        [FromQuery] DateTime date,
        [FromQuery] int? employeeId = null)
    {
        try
        {
            var notified = await _waitingListService.NotifyMatchingCustomersAsync(
                organizationId, serviceId, date, employeeId);
            
            return Ok(new { 
                message = $"Se ha notificado a {notified.Count()} cliente(s)",
                notifiedCustomers = notified 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
