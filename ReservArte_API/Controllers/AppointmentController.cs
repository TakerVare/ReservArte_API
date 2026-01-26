using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Services;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers
{
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

        [HttpGet]
        // [Authorize(Roles = "Admin,Employee")]
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.Employee)]
        public async Task<ActionResult<IEnumerable<AppointmentDtoToList>>> GetAll()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.Employee)]
        public async Task<ActionResult<Appointment?>> GetById(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }
        [HttpGet("user/{userId}")]
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.Employee)]
        [Authorize(Policy = "ClientOwnAppointment")]
        public async Task<ActionResult<IEnumerable<AppointmentDtoToList>>> GetByUserId(int userId)
        {
            var appointments = await _appointmentService.GetByUserIdAsync(userId);
            return Ok(appointments);
        }
        [HttpPost]
        public async Task<ActionResult<Appointment?>> Create(Appointment appointment)
        {
            var createdAppointment = await _appointmentService.CreateAsync(appointment);
            return CreatedAtAction(nameof(GetById), new { id = createdAppointment?.Id }, createdAppointment);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Appointment?>> Update(int id, Appointment appointment)
        {
            var updatedAppointment = await _appointmentService.UpdateAsync(id, appointment);
            if (updatedAppointment == null)
            {
                return NotFound();
            }
            return Ok(updatedAppointment);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _appointmentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }


        // [HttpPost("CreateUser")]
        // [Authorize(Roles = Roles.Admin)]
        // public IActionResult CreateUser(UserDtoIn userDtoIn)
        // {
        //     try
        //     {
        //         if (!ModelState.IsValid)  {return BadRequest(ModelState); } 

        //         var token = _authService.CreateUser(userDtoIn);
        //         return Ok(token);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest
        //         ("Error generating the token: " + ex.Message);
        //     }
        // }
    }
}   