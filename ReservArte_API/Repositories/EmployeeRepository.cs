using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region Employee CRUD

    public async Task<IEnumerable<EmployeeDtoOut>> GetAllAsync()
    {
        var employees = new List<EmployeeDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, FirstName, LastName, Email, Phone, ProfileImageUrl, HireDate, IsActive 
                         FROM Employees WHERE Rol = @Rol";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Rol", Roles.Employee);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var firstName = reader.GetString(1);
                        var lastName = reader.GetString(2);
                        employees.Add(new EmployeeDtoOut
                        {
                            Id = reader.GetInt32(0),
                            FirstName = firstName,
                            LastName = lastName,
                            FullName = $"{firstName} {lastName}".Trim(),
                            Email = reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            ProfileImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                            HireDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6).ToString("yyyy-MM-dd"),
                            IsActive = reader.GetBoolean(7)
                        });
                    }
                }
            }
        }

        return employees;
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, FirstName, LastName, Email, Phone, Rol, ProfileImageUrl, HireDate, IsActive 
                         FROM Employees WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Employee
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Rol = reader.GetString(5),
                            ProfileImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                            HireDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            IsActive = reader.GetBoolean(8)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<Employee?> CreateAsync(Employee employee)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO Employees (FirstName, LastName, Email, Phone, Rol, ProfileImageUrl, HireDate, IsActive)
                         VALUES (@FirstName, @LastName, @Email, @Phone, @Rol, @ProfileImageUrl, @HireDate, @IsActive);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@Phone", (object?)employee.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Rol", Roles.Employee);
                command.Parameters.AddWithValue("@ProfileImageUrl", (object?)employee.ProfileImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@HireDate", (object?)employee.HireDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", employee.IsActive);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                employee.Id = newId;

                return employee;
            }
        }
    }

    public async Task<Employee?> UpdateAsync(int id, Employee employee)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Employees 
                         SET FirstName = @FirstName,
                             LastName = @LastName, 
                             Email = @Email, 
                             Phone = @Phone,
                             ProfileImageUrl = @ProfileImageUrl, 
                             HireDate = @HireDate, 
                             IsActive = @IsActive
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@Phone", (object?)employee.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@ProfileImageUrl", (object?)employee.ProfileImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@HireDate", (object?)employee.HireDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", employee.IsActive);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    employee.Id = id;
                    return employee;
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
            var query = "DELETE FROM Employees WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Employee Availability

    public async Task<IEnumerable<EmployeeAvailability>> GetAvailabilityByEmployeeIdAsync(int employeeId)
    {
        var availabilities = new List<EmployeeAvailability>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, EmployeeId, DayOfWeek, StartTime, EndTime, IsRecurring 
                         FROM EmployeeAvailabilities WHERE EmployeeId = @EmployeeId
                         ORDER BY DayOfWeek, StartTime";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        availabilities.Add(new EmployeeAvailability
                        {
                            Id = reader.GetInt32(0),
                            EmployeeId = reader.GetInt32(1),
                            DayOfWeek = reader.GetInt32(2),
                            StartTime = reader.GetTimeSpan(3),
                            EndTime = reader.GetTimeSpan(4),
                            IsRecurring = reader.GetBoolean(5)
                        });
                    }
                }
            }
        }

        return availabilities;
    }

    public async Task<EmployeeAvailability?> CreateAvailabilityAsync(EmployeeAvailability availability)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO EmployeeAvailabilities (EmployeeId, DayOfWeek, StartTime, EndTime, IsRecurring)
                         VALUES (@EmployeeId, @DayOfWeek, @StartTime, @EndTime, @IsRecurring);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", availability.EmployeeId);
                command.Parameters.AddWithValue("@DayOfWeek", availability.DayOfWeek);
                command.Parameters.AddWithValue("@StartTime", availability.StartTime);
                command.Parameters.AddWithValue("@EndTime", availability.EndTime);
                command.Parameters.AddWithValue("@IsRecurring", availability.IsRecurring);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                availability.Id = newId;

                return availability;
            }
        }
    }

    public async Task<EmployeeAvailability?> UpdateAvailabilityAsync(int id, EmployeeAvailability availability)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE EmployeeAvailabilities 
                         SET DayOfWeek = @DayOfWeek, 
                             StartTime = @StartTime, 
                             EndTime = @EndTime,
                             IsRecurring = @IsRecurring
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@DayOfWeek", availability.DayOfWeek);
                command.Parameters.AddWithValue("@StartTime", availability.StartTime);
                command.Parameters.AddWithValue("@EndTime", availability.EndTime);
                command.Parameters.AddWithValue("@IsRecurring", availability.IsRecurring);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    availability.Id = id;
                    return availability;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteAvailabilityAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM EmployeeAvailabilities WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Employee Exceptions

    public async Task<IEnumerable<EmployeeException>> GetExceptionsByEmployeeIdAsync(int employeeId)
    {
        var exceptions = new List<EmployeeException>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, EmployeeId, StartDateTime, EndDateTime, Reason, Type 
                         FROM EmployeeExceptions WHERE EmployeeId = @EmployeeId
                         ORDER BY StartDateTime DESC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        exceptions.Add(new EmployeeException
                        {
                            Id = reader.GetInt32(0),
                            EmployeeId = reader.GetInt32(1),
                            StartDateTime = reader.GetDateTime(2),
                            EndDateTime = reader.GetDateTime(3),
                            Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Type = reader.GetString(5)
                        });
                    }
                }
            }
        }

        return exceptions;
    }

    public async Task<EmployeeException?> CreateExceptionAsync(EmployeeException exception)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO EmployeeExceptions (EmployeeId, StartDateTime, EndDateTime, Reason, Type)
                         VALUES (@EmployeeId, @StartDateTime, @EndDateTime, @Reason, @Type);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", exception.EmployeeId);
                command.Parameters.AddWithValue("@StartDateTime", exception.StartDateTime);
                command.Parameters.AddWithValue("@EndDateTime", exception.EndDateTime);
                command.Parameters.AddWithValue("@Reason", (object?)exception.Reason ?? DBNull.Value);
                command.Parameters.AddWithValue("@Type", exception.Type);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                exception.Id = newId;

                return exception;
            }
        }
    }

    public async Task<EmployeeException?> UpdateExceptionAsync(int id, EmployeeException exception)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE EmployeeExceptions 
                         SET StartDateTime = @StartDateTime, 
                             EndDateTime = @EndDateTime, 
                             Reason = @Reason,
                             Type = @Type
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@StartDateTime", exception.StartDateTime);
                command.Parameters.AddWithValue("@EndDateTime", exception.EndDateTime);
                command.Parameters.AddWithValue("@Reason", (object?)exception.Reason ?? DBNull.Value);
                command.Parameters.AddWithValue("@Type", exception.Type);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    exception.Id = id;
                    return exception;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteExceptionAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM EmployeeExceptions WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Employee Services

    public async Task<IEnumerable<EmployeeServiceDto>> GetServicesByEmployeeIdAsync(int employeeId)
    {
        var services = new List<EmployeeServiceDto>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT es.EmployeeId, es.ServiceId, es.ProficiencyLevel, 
                                s.Name, s.BasePrice, s.DurationMinutes
                         FROM EmployeeServices es
                         INNER JOIN Services s ON es.ServiceId = s.Id
                         WHERE es.EmployeeId = @EmployeeId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", employeeId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        services.Add(new EmployeeServiceDto
                        {
                            EmployeeId = reader.GetInt32(0),
                            ServiceId = reader.GetInt32(1),
                            ProficiencyLevel = reader.GetInt32(2),
                            ServiceName = reader.GetString(3),
                            ServicePrice = reader.GetDecimal(4),
                            ServiceDurationMinutes = reader.GetInt32(5)
                        });
                    }
                }
            }
        }

        return services;
    }

    public async Task<EmployeeServiceLink?> AssignServiceAsync(EmployeeServiceLink employeeService)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            // Check if already exists
            var checkQuery = @"SELECT COUNT(*) FROM EmployeeServices 
                              WHERE EmployeeId = @EmployeeId AND ServiceId = @ServiceId";
            
            using (var checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@EmployeeId", employeeService.EmployeeId);
                checkCommand.Parameters.AddWithValue("@ServiceId", employeeService.ServiceId);
                
                var exists = (int)(await checkCommand.ExecuteScalarAsync() ?? 0) > 0;
                
                if (exists)
                {
                    // Update existing
                    var updateQuery = @"UPDATE EmployeeServices 
                                       SET ProficiencyLevel = @ProficiencyLevel
                                       WHERE EmployeeId = @EmployeeId AND ServiceId = @ServiceId";
                    
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@EmployeeId", employeeService.EmployeeId);
                        updateCommand.Parameters.AddWithValue("@ServiceId", employeeService.ServiceId);
                        updateCommand.Parameters.AddWithValue("@ProficiencyLevel", employeeService.ProficiencyLevel);
                        
                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    // Insert new
                    var insertQuery = @"INSERT INTO EmployeeServices (EmployeeId, ServiceId, ProficiencyLevel)
                                       VALUES (@EmployeeId, @ServiceId, @ProficiencyLevel)";
                    
                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@EmployeeId", employeeService.EmployeeId);
                        insertCommand.Parameters.AddWithValue("@ServiceId", employeeService.ServiceId);
                        insertCommand.Parameters.AddWithValue("@ProficiencyLevel", employeeService.ProficiencyLevel);
                        
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            return employeeService;
        }
    }

    public async Task<bool> RemoveServiceAsync(int employeeId, int serviceId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM EmployeeServices WHERE EmployeeId = @EmployeeId AND ServiceId = @ServiceId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmployeeId", employeeId);
                command.Parameters.AddWithValue("@ServiceId", serviceId);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion
}
