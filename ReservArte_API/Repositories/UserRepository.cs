using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RestauranteDB") ?? "Not found";;
        }
        public void Add(UserDtoIn user) {
            throw new NotImplementedException("Not implemented yet");
        }
        public IEnumerable<UserDtoOut> GetAll() {
            throw new NotImplementedException("Not implemented yet");
        }
        public UserDtoOut Get(int id) {
            throw new NotImplementedException("Not implemented yet");
        }
        public void Update(UserDtoIn user) {
            throw new NotImplementedException("Not implemented yet");
        }
        public void Delete(int id) {
            throw new NotImplementedException("Not implemented yet");
        }
        public void SaveChanges() {
            throw new NotImplementedException("Not implemented yet");
        }

        public UserDtoOut AddUserFromCredentials(UserDtoIn userDtoIn) {
            var userId = 2; //fake userID 
            var user = new UserDtoOut { UserId = userId, UserName = userDtoIn.UserName, Email = userDtoIn.Email, Role = Roles.Admin};
            if (user == null)
            {
                //Simulating register failed
                throw new KeyNotFoundException("User not created.");
            }
            return user;
        }
        
        public UserDtoOut GetUserFromCredentials(LoginDtoIn loginDtoIn) {
            if ((loginDtoIn.Email != "guille@svalero.com") && (loginDtoIn.Password != "1234"))
            {
                //Simulating login failed
                throw new KeyNotFoundException("User not found.");
            } else {
                var user = new UserDtoOut { UserId = 1, UserName = "agimenez", Email = "agimenezg@svalero.com", Role = Roles.Admin};
                return user;
            }
        }
        
    }   
}