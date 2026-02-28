using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories
{
    public interface IUserRepository
    {
        public void Add(UserDtoIn user);
        public IEnumerable<UserDtoOut> GetAll();
        public UserDtoOut Get(int id);
        public void Update(UserDtoIn user);
        public void Delete(int id);
        /// <summary>Inserta un usuario en Users y devuelve el Id asignado. Usado al crear Customer/Employee para unificar Id.</summary>
        public Task<int> CreateUserAsync(string firstName, string lastName, string email, string password, string rol, string? phone = null, string? profileImageUrl = null);
        public Task<UserDtoOut> AddUserFromCredentialsAsync(UserDtoIn userDtoIn);
        public Task<UserDtoOut> GetUserFromCredentialsAsync(LoginDtoIn loginDtoIn);
    }
}
