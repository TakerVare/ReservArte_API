using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ReservArteDB") ?? throw new ArgumentNullException("Connection string not found");
        }

        public async Task<IEnumerable<AppointmentDtoToList>> GetAllAsync()
        {
            var appointments = new List<AppointmentDtoToList>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT a.Id, a.CustomerId, a.EmployeeId, a.AppointmentDate, 
                             a.StartTime, a.EndTime, a.Status
                             FROM Appointments a";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        appointments.Add(new AppointmentDtoToList
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1).ToString(),
                            EmployeeId = reader.GetInt32(2).ToString(),
                            AppointmentDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                            StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4)),
                            EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5)),
                            Status = reader.GetString(6)
                        });
                    }
                }
            }
            
            return appointments;
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT Id, CustomerId, EmployeeId, AppointmentDate, 
                             StartTime, EndTime, Status, TotalPrice
                             FROM Appointments WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Appointment
                            {
                                Id = reader.GetInt32(0),
                                CustomerId = reader.GetInt32(1),
                                EmployeeId = reader.GetInt32(2),
                                AppointmentDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                                StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4)),
                                EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5)),
                                Status = reader.GetString(6),
                                TotalPrice = reader.GetDecimal(7)
                            };
                        }
                    }
                }
            }
            
            return null;
        }

        public async Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId)
        {
            var appointments = new List<AppointmentDtoToList>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT a.Id, a.CustomerId, a.EmployeeId, a.AppointmentDate, 
                             a.StartTime, a.EndTime, a.Status
                             FROM Appointments a 
                             WHERE a.CustomerId = @UserId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            appointments.Add(new AppointmentDtoToList
                            {
                                Id = reader.GetInt32(0),
                                CustomerId = reader.GetInt32(1).ToString(),
                                EmployeeId = reader.GetInt32(2).ToString(),
                                AppointmentDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
                                StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4)),
                                EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5)),
                                Status = reader.GetString(6)
                            });
                        }
                    }
                }
            }
            
            return appointments;
        }

        public async Task<IEnumerable<User>> GetCustomerAsync(int customerId)
        {
            var users = new List<User>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT Id, FirstName, LastName, Email, Rol FROM Users WHERE Id = @CustomerId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                Rol = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            
            return users;
        }

        public async Task<IEnumerable<User>> GetEmployeeAsync(int employeeId)
        {
            var users = new List<User>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT Id, FirstName, LastName, Email, Rol FROM Users WHERE Id = @EmployeeId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", employeeId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                Rol = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            
            return users;
        }

        public async Task<Appointment?> CreateAsync(Appointment appointment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"INSERT INTO Appointments (CustomerId, EmployeeId, AppointmentDate, 
                             StartTime, EndTime, Status, TotalPrice)
                             VALUES (@CustomerId, @EmployeeId, @AppointmentDate, 
                             @StartTime, @EndTime, @Status, @TotalPrice);
                             SELECT CAST(SCOPE_IDENTITY() as int)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
                    command.Parameters.AddWithValue("@EmployeeId", appointment.EmployeeId);
                    command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));
                    command.Parameters.AddWithValue("@StartTime", appointment.StartTime.ToTimeSpan());
                    command.Parameters.AddWithValue("@EndTime", appointment.EndTime.ToTimeSpan());
                    command.Parameters.AddWithValue("@Status", appointment.Status);
                    command.Parameters.AddWithValue("@TotalPrice", appointment.TotalPrice);
                    
                    var newId = (int)await command.ExecuteScalarAsync();
                    appointment.Id = newId;
                    
                    return appointment;
                }
            }
        }

        public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"UPDATE Appointments 
                             SET CustomerId = @CustomerId, 
                                 EmployeeId = @EmployeeId, 
                                 AppointmentDate = @AppointmentDate,
                                 StartTime = @StartTime, 
                                 EndTime = @EndTime, 
                                 Status = @Status, 
                                 TotalPrice = @TotalPrice
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
                    command.Parameters.AddWithValue("@EmployeeId", appointment.EmployeeId);
                    command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));
                    command.Parameters.AddWithValue("@StartTime", appointment.StartTime.ToTimeSpan());
                    command.Parameters.AddWithValue("@EndTime", appointment.EndTime.ToTimeSpan());
                    command.Parameters.AddWithValue("@Status", appointment.Status);
                    command.Parameters.AddWithValue("@TotalPrice", appointment.TotalPrice);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    
                    if (rowsAffected > 0)
                    {
                        appointment.Id = id;
                        return appointment;
                    }
                }
            }
            
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM Appointments WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    
                    return rowsAffected > 0;
                }
            }
        }
    }
}