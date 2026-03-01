using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    #region Employee CRUD

    /// <summary>
    /// Get all employees
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDtoOut>>> GetAll()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDtoOut>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }
        return Ok(employee);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeDtoOut>> Create([FromBody] EmployeeDtoIn employeeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdEmployee = await _employeeService.CreateAsync(employeeDto);
        if (createdEmployee == null)
        {
            return BadRequest(new { message = "Error al crear el empleado" });
        }

        return CreatedAtAction(nameof(GetById), new { id = createdEmployee.Id }, createdEmployee);
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeDtoOut>> Update(int id, [FromBody] EmployeeDtoIn employeeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedEmployee = await _employeeService.UpdateAsync(id, employeeDto);
        if (updatedEmployee == null)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }

        return Ok(updatedEmployee);
    }

    /// <summary>
    /// Delete an employee
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }

        return NoContent();
    }

    #endregion

    #region Employee Availability

    /// <summary>
    /// Get employee availability schedule
    /// </summary>
    [HttpGet("{id}/availability")]
    public async Task<ActionResult<IEnumerable<EmployeeAvailabilityDto>>> GetAvailability(int id)
    {
        var availability = await _employeeService.GetAvailabilityByEmployeeIdAsync(id);
        return Ok(availability);
    }

    /// <summary>
    /// Add availability for an employee
    /// </summary>
    [HttpPost("{id}/availability")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeAvailabilityDto>> CreateAvailability(int id, [FromBody] EmployeeAvailabilityDto availabilityDto)
    {
        var created = await _employeeService.CreateAvailabilityAsync(id, availabilityDto);
        if (created == null)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }

        return CreatedAtAction(nameof(GetAvailability), new { id }, created);
    }

    /// <summary>
    /// Update availability
    /// </summary>
    [HttpPut("availability/{availabilityId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeAvailabilityDto>> UpdateAvailability(int availabilityId, [FromBody] EmployeeAvailabilityDto availabilityDto)
    {
        var updated = await _employeeService.UpdateAvailabilityAsync(availabilityId, availabilityDto);
        if (updated == null)
        {
            return NotFound(new { message = "Disponibilidad no encontrada" });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Delete availability
    /// </summary>
    [HttpDelete("availability/{availabilityId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteAvailability(int availabilityId)
    {
        var deleted = await _employeeService.DeleteAvailabilityAsync(availabilityId);
        if (!deleted)
        {
            return NotFound(new { message = "Disponibilidad no encontrada" });
        }

        return NoContent();
    }

    #endregion

    #region Employee Exceptions

    /// <summary>
    /// Get employee exceptions (vacations, sick days, etc.)
    /// </summary>
    [HttpGet("{id}/exceptions")]
    public async Task<ActionResult<IEnumerable<EmployeeExceptionDto>>> GetExceptions(int id)
    {
        var exceptions = await _employeeService.GetExceptionsByEmployeeIdAsync(id);
        return Ok(exceptions);
    }

    /// <summary>
    /// Add an exception for an employee
    /// </summary>
    [HttpPost("{id}/exceptions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeExceptionDto>> CreateException(int id, [FromBody] EmployeeExceptionDto exceptionDto)
    {
        var created = await _employeeService.CreateExceptionAsync(id, exceptionDto);
        if (created == null)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }

        return CreatedAtAction(nameof(GetExceptions), new { id }, created);
    }

    /// <summary>
    /// Update an exception
    /// </summary>
    [HttpPut("exceptions/{exceptionId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeExceptionDto>> UpdateException(int exceptionId, [FromBody] EmployeeExceptionDto exceptionDto)
    {
        var updated = await _employeeService.UpdateExceptionAsync(exceptionId, exceptionDto);
        if (updated == null)
        {
            return NotFound(new { message = "Excepción no encontrada" });
        }

        return Ok(updated);
    }

    /// <summary>
    /// Delete an exception
    /// </summary>
    [HttpDelete("exceptions/{exceptionId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteException(int exceptionId)
    {
        var deleted = await _employeeService.DeleteExceptionAsync(exceptionId);
        if (!deleted)
        {
            return NotFound(new { message = "Excepción no encontrada" });
        }

        return NoContent();
    }

    #endregion

    #region Employee Services

    /// <summary>
    /// Get services assigned to an employee
    /// </summary>
    [HttpGet("{id}/services")]
    public async Task<ActionResult<IEnumerable<EmployeeServiceDto>>> GetServices(int id)
    {
        var services = await _employeeService.GetServicesByEmployeeIdAsync(id);
        return Ok(services);
    }

    /// <summary>
    /// Assign a service to an employee
    /// </summary>
    [HttpPost("{id}/services")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<EmployeeServiceDto>> AssignService(int id, [FromBody] EmployeeServiceDto serviceDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var assigned = await _employeeService.AssignServiceAsync(id, serviceDto);
        if (assigned == null)
        {
            return NotFound(new { message = "Empleado no encontrado" });
        }

        return CreatedAtAction(nameof(GetServices), new { id }, assigned);
    }

    /// <summary>
    /// Remove a service from an employee
    /// </summary>
    [HttpDelete("{id}/services/{serviceId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RemoveService(int id, int serviceId)
    {
        var removed = await _employeeService.RemoveServiceAsync(id, serviceId);
        if (!removed)
        {
            return NotFound(new { message = "Servicio no asignado al empleado" });
        }

        return NoContent();
    }

    #endregion
}
