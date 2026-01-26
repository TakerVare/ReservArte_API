using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _repository;

        public AuthService(IConfiguration configuration, IUserRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        public string Login(LoginDtoIn loginDtoIn) {
            var user = _repository.GetUserFromCredentials(loginDtoIn);
            return GenerateToken(user);
        }

        public string Register(UserDtoIn userDtoIn) {
            var user = _repository.AddUserFromCredentials(userDtoIn);
            return GenerateToken(user);
        }

        public string GenerateToken(UserDtoOut userDtoOut) {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Subject = new ClaimsIdentity(new Claim[] 
                    {
                        new Claim(ClaimTypes.NameIdentifier, Convert.ToString(userDtoOut.UserId)),
                        new Claim(ClaimTypes.Name, userDtoOut.UserName),
                        new Claim(ClaimTypes.Role, userDtoOut.Role),
                        new Claim(ClaimTypes.Email, userDtoOut.Email),
                        new Claim("myCustomClaim", "myCustomClaimValue"),
                        // add other claims
                    }),
                Expires = DateTime.UtcNow.AddDays(7), // AddMinutes(60)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        } 
        public bool HasAccessToResource(int requestedUserID, ClaimsPrincipal user) 
        {
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId)) 
            { 
                return false; 
            }
            var isOwnResource = userId == requestedUserID;

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim != null) return false;
            var isAdmin = roleClaim!.Value == Roles.Admin;
            
            var hasAccess = isOwnResource || isAdmin;
            return hasAccess;
        }
     

    }
}
