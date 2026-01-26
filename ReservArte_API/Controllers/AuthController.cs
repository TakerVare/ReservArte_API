using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Services;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers
{
[ApiController]
[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginDtoIn loginDtoIn)
        {
            try
            {
                if (!ModelState.IsValid)  {return BadRequest(ModelState); } 

                var token = _authService.Login(loginDtoIn);
                return Ok(token);
            }
            catch (KeyNotFoundException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating the token: " + ex.Message);
            }
        }

        [HttpPost("Register")]
        public IActionResult Register(UserDtoIn userDtoIn)
        {
            try
            {
                if (!ModelState.IsValid)  {return BadRequest(ModelState); } 

                var token = _authService.Register(userDtoIn);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating the token: " + ex.Message);
            }
        }

        [HttpPost("CreateUser")]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult CreateUser(UserDtoIn userDtoIn)
        {
            try
            {
                if (!ModelState.IsValid)  {return BadRequest(ModelState); } 

                var token = _authService.Register(userDtoIn);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest
                ("Error generating the token: " + ex.Message);
            }
        }


    }
}