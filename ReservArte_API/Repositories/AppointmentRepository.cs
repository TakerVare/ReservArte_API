using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly string _connectionString;

    public AppointmentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region CRUD Básico

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, CustomerId, EmployeeId, AppointmentDate, 
                     StartTime, EndTime, Status, TotalPrice, DepositAmount,
                     RedsysOrderNumber, RedsysPreAuthToken, PaymentMethodId,
                     CancellationReason, CancelledAt, CancelledById, CancelledByType,
                     Notes, CreatedAt, UpdatedAt
                     FROM Appointments WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAppointmentFromReader(reader);
        }
        
        return null;
    }

    public async Task<AppointmentDtoOut?> GetByIdDetailedAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.CustomerId, a.EmployeeId, a.AppointmentDate, 
                     a.StartTime, a.EndTime, a.Status, a.TotalPrice, a.DepositAmount,
                     a.RedsysOrderNumber, a.RedsysPreAuthToken, a.PaymentMethodId,
                     a.CancellationReason, a.CancelledAt, a.CancelledById, a.CancelledByType,
                     a.Notes, a.CreatedAt, a.UpdatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName,
                     e.FirstName + ' ' + e.LastName as EmployeeName,
                     pm.CardLast4 as PaymentMethodLast4
                     FROM Appointments a
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     INNER JOIN Employees e ON a.EmployeeId = e.Id
                     LEFT JOIN CustomerPaymentMethods pm ON a.PaymentMethodId = pm.Id
                     WHERE a.Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var dto = new AppointmentDtoOut
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")).ToString("yyyy-MM-dd"),
                StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("StartTime"))),
                EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("EndTime"))),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                DepositAmount = reader.GetDecimal(reader.GetOrdinal("DepositAmount")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
            
            // Campos opcionales
            var redsysOrderIdx = reader.GetOrdinal("RedsysOrderNumber");
            if (!reader.IsDBNull(redsysOrderIdx))
                dto.RedsysOrderNumber = reader.GetString(redsysOrderIdx);
            
            var paymentMethodIdx = reader.GetOrdinal("PaymentMethodId");
            if (!reader.IsDBNull(paymentMethodIdx))
                dto.PaymentMethodId = reader.GetInt32(paymentMethodIdx);
            
            var paymentLast4Idx = reader.GetOrdinal("PaymentMethodLast4");
            if (!reader.IsDBNull(paymentLast4Idx))
                dto.PaymentMethodLast4 = reader.GetString(paymentLast4Idx);
            
            var cancellationReasonIdx = reader.GetOrdinal("CancellationReason");
            if (!reader.IsDBNull(cancellationReasonIdx))
                dto.CancellationReason = reader.GetString(cancellationReasonIdx);
            
            var cancelledAtIdx = reader.GetOrdinal("CancelledAt");
            if (!reader.IsDBNull(cancelledAtIdx))
                dto.CancelledAt = reader.GetDateTime(cancelledAtIdx);
            
            var cancelledByIdIdx = reader.GetOrdinal("CancelledById");
            if (!reader.IsDBNull(cancelledByIdIdx))
                dto.CancelledById = reader.GetInt32(cancelledByIdIdx);
            
            var cancelledByTypeIdx = reader.GetOrdinal("CancelledByType");
            if (!reader.IsDBNull(cancelledByTypeIdx))
                dto.CancelledByType = reader.GetString(cancelledByTypeIdx);
            
            var notesIdx = reader.GetOrdinal("Notes");
            if (!reader.IsDBNull(notesIdx))
                dto.Notes = reader.GetString(notesIdx);
            
            var updatedAtIdx = reader.GetOrdinal("UpdatedAt");
            if (!reader.IsDBNull(updatedAtIdx))
                dto.UpdatedAt = reader.GetDateTime(updatedAtIdx);
            
            // Obtener servicios
            dto.Services = (await GetAppointmentServicesDetailedAsync(id)).ToList();
            
            return dto;
        }
        
        return null;
    }

    public async Task<Appointment?> CreateAsync(Appointment appointment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Appointments (CustomerId, EmployeeId, AppointmentDate, 
                     StartTime, EndTime, Status, TotalPrice, DepositAmount,
                     RedsysOrderNumber, RedsysPreAuthToken, PaymentMethodId, Notes, CreatedAt)
                     VALUES (@CustomerId, @EmployeeId, @AppointmentDate, 
                     @StartTime, @EndTime, @Status, @TotalPrice, @DepositAmount,
                     @RedsysOrderNumber, @RedsysPreAuthToken, @PaymentMethodId, @Notes, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
        command.Parameters.AddWithValue("@EmployeeId", appointment.EmployeeId);
        command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@StartTime", appointment.StartTime.ToTimeSpan());
        command.Parameters.AddWithValue("@EndTime", appointment.EndTime.ToTimeSpan());
        command.Parameters.AddWithValue("@Status", appointment.Status);
        command.Parameters.AddWithValue("@TotalPrice", appointment.TotalPrice);
        command.Parameters.AddWithValue("@DepositAmount", appointment.DepositAmount);
        command.Parameters.AddWithValue("@RedsysOrderNumber", (object?)appointment.RedsysOrderNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysPreAuthToken", (object?)appointment.RedsysPreAuthToken ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentMethodId", (object?)appointment.PaymentMethodId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)appointment.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", appointment.CreatedAt);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        appointment.Id = newId;
        
        return appointment;
    }

    public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE Appointments SET
                     CustomerId = @CustomerId,
                     EmployeeId = @EmployeeId,
                     AppointmentDate = @AppointmentDate,
                     StartTime = @StartTime,
                     EndTime = @EndTime,
                     Status = @Status,
                     TotalPrice = @TotalPrice,
                     DepositAmount = @DepositAmount,
                     RedsysOrderNumber = @RedsysOrderNumber,
                     RedsysPreAuthToken = @RedsysPreAuthToken,
                     PaymentMethodId = @PaymentMethodId,
                     Notes = @Notes,
                     UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
        command.Parameters.AddWithValue("@EmployeeId", appointment.EmployeeId);
        command.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@StartTime", appointment.StartTime.ToTimeSpan());
        command.Parameters.AddWithValue("@EndTime", appointment.EndTime.ToTimeSpan());
        command.Parameters.AddWithValue("@Status", appointment.Status);
        command.Parameters.AddWithValue("@TotalPrice", appointment.TotalPrice);
        command.Parameters.AddWithValue("@DepositAmount", appointment.DepositAmount);
        command.Parameters.AddWithValue("@RedsysOrderNumber", (object?)appointment.RedsysOrderNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysPreAuthToken", (object?)appointment.RedsysPreAuthToken ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentMethodId", (object?)appointment.PaymentMethodId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)appointment.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        if (rowsAffected > 0)
        {
            appointment.Id = id;
            return appointment;
        }
        
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Primero eliminar servicios asociados
        var deleteServicesQuery = "DELETE FROM AppointmentServiceItems WHERE AppointmentId = @Id";
        using (var deleteServicesCmd = new SqlCommand(deleteServicesQuery, connection))
        {
            deleteServicesCmd.Parameters.AddWithValue("@Id", id);
            await deleteServicesCmd.ExecuteNonQueryAsync();
        }
        
        var query = "DELETE FROM Appointments WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Consultas de Agenda

    public async Task<IEnumerable<AppointmentDtoToList>> GetAllAsync()
    {
        var appointments = new List<AppointmentDtoToList>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.CustomerId, a.EmployeeId, a.AppointmentDate, 
                     a.StartTime, a.EndTime, a.Status
                     FROM Appointments a
                     ORDER BY a.AppointmentDate DESC, a.StartTime";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
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
        
        return appointments;
    }

    public async Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId)
    {
        var appointments = new List<AppointmentDtoToList>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.CustomerId, a.EmployeeId, a.AppointmentDate, 
                     a.StartTime, a.EndTime, a.Status
                     FROM Appointments a 
                     WHERE a.CustomerId = @UserId
                     ORDER BY a.AppointmentDate DESC, a.StartTime";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);
        
        using var reader = await command.ExecuteReaderAsync();
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
        
        return appointments;
    }

    public async Task<IEnumerable<AgendaAppointmentDto>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        var appointments = new List<AgendaAppointmentDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                     a.CustomerId, c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     a.EmployeeId, e.FirstName + ' ' + e.LastName as EmployeeName,
                     a.Status, a.TotalPrice, a.Notes
                     FROM Appointments a
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     INNER JOIN Employees e ON a.EmployeeId = e.Id
                     WHERE a.AppointmentDate >= @StartDate
                     AND a.AppointmentDate <= @EndDate
                     ORDER BY a.AppointmentDate, a.StartTime";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@EndDate", endDate.ToDateTime(TimeOnly.MaxValue));
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var appointment = MapAgendaAppointmentFromReader(reader);
            appointments.Add(appointment);
        }
        
        // Obtener descripción de servicios para cada cita
        foreach (var apt in appointments)
        {
            apt.ServicesDescription = await GetServicesDescriptionAsync(apt.Id);
            apt.DurationMinutes = (int)(apt.EndTime - apt.StartTime).TotalMinutes;
            apt.ColorCode = GetColorCodeForAppointment(apt.Status, apt.IsVip);
        }
        
        return appointments;
    }

    public async Task<IEnumerable<AgendaAppointmentDto>> GetByEmployeeAsync(int employeeId, DateOnly startDate, DateOnly endDate)
    {
        var appointments = new List<AgendaAppointmentDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                     a.CustomerId, c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     a.EmployeeId, e.FirstName + ' ' + e.LastName as EmployeeName,
                     a.Status, a.TotalPrice, a.Notes
                     FROM Appointments a
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     INNER JOIN Employees e ON a.EmployeeId = e.Id
                     WHERE a.EmployeeId = @EmployeeId
                     AND a.AppointmentDate >= @StartDate
                     AND a.AppointmentDate <= @EndDate
                     ORDER BY a.AppointmentDate, a.StartTime";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.Parameters.AddWithValue("@StartDate", startDate.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@EndDate", endDate.ToDateTime(TimeOnly.MaxValue));
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var appointment = MapAgendaAppointmentFromReader(reader);
            appointments.Add(appointment);
        }
        
        foreach (var apt in appointments)
        {
            apt.ServicesDescription = await GetServicesDescriptionAsync(apt.Id);
            apt.DurationMinutes = (int)(apt.EndTime - apt.StartTime).TotalMinutes;
            apt.ColorCode = GetColorCodeForAppointment(apt.Status, apt.IsVip);
        }
        
        return appointments;
    }

    public async Task<IEnumerable<AgendaAppointmentDto>> GetByCustomerAsync(int customerId)
    {
        var appointments = new List<AgendaAppointmentDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime,
                     a.CustomerId, c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     a.EmployeeId, e.FirstName + ' ' + e.LastName as EmployeeName,
                     a.Status, a.TotalPrice, a.Notes
                     FROM Appointments a
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     INNER JOIN Employees e ON a.EmployeeId = e.Id
                     WHERE a.CustomerId = @CustomerId
                     ORDER BY a.AppointmentDate DESC, a.StartTime";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var appointment = MapAgendaAppointmentFromReader(reader);
            appointments.Add(appointment);
        }
        
        foreach (var apt in appointments)
        {
            apt.ServicesDescription = await GetServicesDescriptionAsync(apt.Id);
            apt.DurationMinutes = (int)(apt.EndTime - apt.StartTime).TotalMinutes;
            apt.ColorCode = GetColorCodeForAppointment(apt.Status, apt.IsVip);
        }
        
        return appointments;
    }

    #endregion

    #region Validaciones y Comprobaciones

    public async Task<bool> CheckOverlapAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT COUNT(1) FROM Appointments 
                     WHERE EmployeeId = @EmployeeId
                     AND AppointmentDate = @Date
                     AND Status NOT IN ('cancelled', 'cancelled_by_customer', 'cancelled_by_business', 'no_show')
                     AND (
                         (@StartTime >= StartTime AND @StartTime < EndTime)
                         OR (@EndTime > StartTime AND @EndTime <= EndTime)
                         OR (@StartTime <= StartTime AND @EndTime >= EndTime)
                     )";
        
        if (excludeAppointmentId.HasValue)
        {
            query += " AND Id != @ExcludeId";
        }
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.Parameters.AddWithValue("@Date", date.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@StartTime", startTime.ToTimeSpan());
        command.Parameters.AddWithValue("@EndTime", endTime.ToTimeSpan());
        
        if (excludeAppointmentId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeAppointmentId.Value);
        }
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    public async Task<int> GetCustomerNoShowCountAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT COUNT(1) FROM Appointments 
                     WHERE CustomerId = @CustomerId 
                     AND Status = 'no_show'";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    #endregion

    #region Gestión de Estados

    public async Task<bool> UpdateStatusAsync(int id, string status, string? notes = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE Appointments SET 
                     Status = @Status, 
                     Notes = CASE WHEN @Notes IS NOT NULL THEN @Notes ELSE Notes END,
                     UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CancelAsync(int id, string reason, int cancelledById, string cancelledByType)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var status = cancelledByType == CancelledByType.Customer 
            ? Status.CancelledByCustomer 
            : Status.CancelledByBusiness;
        
        var query = @"UPDATE Appointments SET 
                     Status = @Status,
                     CancellationReason = @Reason,
                     CancelledAt = @CancelledAt,
                     CancelledById = @CancelledById,
                     CancelledByType = @CancelledByType,
                     UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Reason", reason);
        command.Parameters.AddWithValue("@CancelledAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@CancelledById", cancelledById);
        command.Parameters.AddWithValue("@CancelledByType", cancelledByType);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Servicios de Cita

    public async Task<bool> AddAppointmentServicesAsync(int appointmentId, List<AppointmentServiceItem> services)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        foreach (var service in services)
        {
            var query = @"INSERT INTO AppointmentServiceItems 
                         (AppointmentId, ServiceId, ServiceVariationId, Price, DurationMinutes, [Order])
                         VALUES (@AppointmentId, @ServiceId, @ServiceVariationId, @Price, @DurationMinutes, @Order)";
            
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AppointmentId", appointmentId);
            command.Parameters.AddWithValue("@ServiceId", service.ServiceId);
            command.Parameters.AddWithValue("@ServiceVariationId", (object?)service.ServiceVariationId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Price", service.Price);
            command.Parameters.AddWithValue("@DurationMinutes", service.DurationMinutes);
            command.Parameters.AddWithValue("@Order", service.Order);
            
            await command.ExecuteNonQueryAsync();
        }
        
        return true;
    }

    public async Task<IEnumerable<AppointmentServiceItem>> GetAppointmentServicesAsync(int appointmentId)
    {
        var services = new List<AppointmentServiceItem>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, AppointmentId, ServiceId, ServiceVariationId, Price, DurationMinutes, [Order]
                     FROM AppointmentServiceItems
                     WHERE AppointmentId = @AppointmentId
                     ORDER BY [Order]";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            services.Add(new AppointmentServiceItem
            {
                Id = reader.GetInt32(0),
                AppointmentId = reader.GetInt32(1),
                ServiceId = reader.GetInt32(2),
                ServiceVariationId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                Price = reader.GetDecimal(4),
                DurationMinutes = reader.GetInt32(5),
                Order = reader.GetInt32(6)
            });
        }
        
        return services;
    }

    private async Task<IEnumerable<AppointmentServiceItemDtoOut>> GetAppointmentServicesDetailedAsync(int appointmentId)
    {
        var services = new List<AppointmentServiceItemDtoOut>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT asi.Id, asi.ServiceId, s.Name as ServiceName, 
                     asi.ServiceVariationId, sv.Name as VariationName,
                     asi.Price, asi.DurationMinutes, asi.[Order]
                     FROM AppointmentServiceItems asi
                     INNER JOIN Services s ON asi.ServiceId = s.Id
                     LEFT JOIN ServiceVariations sv ON asi.ServiceVariationId = sv.Id
                     WHERE asi.AppointmentId = @AppointmentId
                     ORDER BY asi.[Order]";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            services.Add(new AppointmentServiceItemDtoOut
            {
                Id = reader.GetInt32(0),
                ServiceId = reader.GetInt32(1),
                ServiceName = reader.GetString(2),
                ServiceVariationId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                VariationName = reader.IsDBNull(4) ? null : reader.GetString(4),
                Price = reader.GetDecimal(5),
                DurationMinutes = reader.GetInt32(6),
                Order = reader.GetInt32(7)
            });
        }
        
        return services;
    }

    private async Task<string> GetServicesDescriptionAsync(int appointmentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT s.Name + ISNULL(' - ' + sv.Name, '')
                     FROM AppointmentServiceItems asi
                     INNER JOIN Services s ON asi.ServiceId = s.Id
                     LEFT JOIN ServiceVariations sv ON asi.ServiceVariationId = sv.Id
                     WHERE asi.AppointmentId = @AppointmentId
                     ORDER BY asi.[Order]";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        
        var services = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            services.Add(reader.GetString(0));
        }
        
        return string.Join(", ", services);
    }

    #endregion

    #region Usuarios Relacionados (Compatibilidad)

    public async Task<IEnumerable<User>> GetCustomerAsync(int customerId)
    {
        var users = new List<User>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT Id, FirstName, LastName, Email, Rol FROM Users WHERE Id = @CustomerId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
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
        
        return users;
    }

    public async Task<IEnumerable<User>> GetEmployeeAsync(int employeeId)
    {
        var users = new List<User>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT Id, FirstName, LastName, Email, Rol FROM Users WHERE Id = @EmployeeId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        
        using var reader = await command.ExecuteReaderAsync();
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
        
        return users;
    }

    #endregion

    #region Helpers

    private static Appointment MapAppointmentFromReader(SqlDataReader reader)
    {
        var appointment = new Appointment
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            AppointmentDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("AppointmentDate"))),
            StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("StartTime"))),
            EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("EndTime"))),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
            DepositAmount = reader.GetDecimal(reader.GetOrdinal("DepositAmount")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var redsysOrderIdx = reader.GetOrdinal("RedsysOrderNumber");
        if (!reader.IsDBNull(redsysOrderIdx))
            appointment.RedsysOrderNumber = reader.GetString(redsysOrderIdx);
        
        var redsysTokenIdx = reader.GetOrdinal("RedsysPreAuthToken");
        if (!reader.IsDBNull(redsysTokenIdx))
            appointment.RedsysPreAuthToken = reader.GetString(redsysTokenIdx);
        
        var paymentMethodIdx = reader.GetOrdinal("PaymentMethodId");
        if (!reader.IsDBNull(paymentMethodIdx))
            appointment.PaymentMethodId = reader.GetInt32(paymentMethodIdx);
        
        var cancellationReasonIdx = reader.GetOrdinal("CancellationReason");
        if (!reader.IsDBNull(cancellationReasonIdx))
            appointment.CancellationReason = reader.GetString(cancellationReasonIdx);
        
        var cancelledAtIdx = reader.GetOrdinal("CancelledAt");
        if (!reader.IsDBNull(cancelledAtIdx))
            appointment.CancelledAt = reader.GetDateTime(cancelledAtIdx);
        
        var cancelledByIdIdx = reader.GetOrdinal("CancelledById");
        if (!reader.IsDBNull(cancelledByIdIdx))
            appointment.CancelledById = reader.GetInt32(cancelledByIdIdx);
        
        var cancelledByTypeIdx = reader.GetOrdinal("CancelledByType");
        if (!reader.IsDBNull(cancelledByTypeIdx))
            appointment.CancelledByType = reader.GetString(cancelledByTypeIdx);
        
        var notesIdx = reader.GetOrdinal("Notes");
        if (!reader.IsDBNull(notesIdx))
            appointment.Notes = reader.GetString(notesIdx);
        
        var updatedAtIdx = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(updatedAtIdx))
            appointment.UpdatedAt = reader.GetDateTime(updatedAtIdx);
        
        return appointment;
    }

    private static AgendaAppointmentDto MapAgendaAppointmentFromReader(SqlDataReader reader)
    {
        var customerCategory = reader.IsDBNull(reader.GetOrdinal("CustomerCategory")) 
            ? null 
            : reader.GetString(reader.GetOrdinal("CustomerCategory"));
        
        return new AgendaAppointmentDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("StartTime"))),
            EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("EndTime"))),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
            CustomerCategory = customerCategory,
            IsVip = customerCategory == CustomerCategory.VIP,
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
        };
    }

    private static string GetColorCodeForAppointment(string status, bool isVip)
    {
        if (isVip) return "#FFD700"; // Gold para VIP
        
        return status switch
        {
            Status.Pending => "#FFA500", // Orange
            Status.Confirmed => "#4CAF50", // Green
            Status.InProgress => "#2196F3", // Blue
            Status.Completed => "#9E9E9E", // Gray
            Status.CancelledByCustomer or Status.CancelledByBusiness or Status.Cancelled => "#F44336", // Red
            Status.NoShow => "#9C27B0", // Purple
            _ => "#757575" // Default gray
        };
    }

    #endregion
}
