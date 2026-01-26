using System.Security.Claims;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces
{
    public interface IAuthService 
    {
        public string Login(LoginDtoIn userDtoIn);
        public string Register(UserDtoIn userDtoIn);
        public string GenerateToken(UserDtoOut userDtoOut);
        public bool HasAccessToResource(int requestedUserID, ClaimsPrincipal user);


    }
}
