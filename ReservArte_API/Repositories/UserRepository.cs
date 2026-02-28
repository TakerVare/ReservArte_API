using Microsoft.Data.SqlClient;
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
            _connectionString = configuration.GetConnectionString("ReservArteDB") ?? "Not found";
        }

        public async Task<int> CreateUserAsync(string firstName, string lastName, string email, string password, string rol, string? phone = null, string? profileImageUrl = null)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO Users (FirstName, LastName, Email, Password, Rol, Phone, ProfileImageUrl, CreatedAt)
                          VALUES (@FirstName, @LastName, @Email, @Password, @Rol, @Phone, @ProfileImageUrl, GETUTCDATE());
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", firstName);
            command.Parameters.AddWithValue("@LastName", lastName);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Password", password);
            command.Parameters.AddWithValue("@Rol", rol);
            command.Parameters.AddWithValue("@Phone", (object?)phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProfileImageUrl", (object?)profileImageUrl ?? DBNull.Value);
            var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
            return newId;
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

        public Task<UserDtoOut> AddUserFromCredentialsAsync(UserDtoIn userDtoIn)
        {
            var userId = 2; //fake userID
            var user = new UserDtoOut { UserId = userId, UserName = userDtoIn.UserName, Email = userDtoIn.Email, Role = Roles.Admin };
            if (user == null)
            {
                throw new KeyNotFoundException("User not created.");
            }
            return Task.FromResult(user);
        }

        public async Task<UserDtoOut> GetUserFromCredentialsAsync(LoginDtoIn loginDtoIn)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Id, FirstName, LastName, Email, Password, Rol FROM Users WHERE Email = @Email";
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", loginDtoIn.Email);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new KeyNotFoundException("Email o contraseña incorrectos.");
            }

            var userId = reader.GetInt32(0);
            var firstName = reader.GetString(1);
            var lastName = reader.GetString(2);
            var email = reader.GetString(3);
            var storedPassword = reader.GetString(4);
            var rol = reader.GetString(5);

            if (storedPassword != loginDtoIn.Password)
            {
                throw new KeyNotFoundException("Email o contraseña incorrectos.");
            }

            var userName = $"{firstName} {lastName}".Trim();
            return new UserDtoOut
            {
                UserId = userId,
                UserName = userName,
                Email = email,
                Role = rol
            };
        }
        
    }   
}