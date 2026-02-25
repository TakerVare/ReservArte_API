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
        public Task<UserDtoOut> AddUserFromCredentialsAsync(UserDtoIn userDtoIn);
        public Task<UserDtoOut> GetUserFromCredentialsAsync(LoginDtoIn loginDtoIn);
    }
}
