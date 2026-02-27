using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class WaitingListRepository : IWaitingListRepository
{
    private readonly string _connectionString;

    public WaitingListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<WaitingList?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, CustomerId, ServiceId, PreferredEmployeeId,
                     PreferredDate, DateRangeStart, DateRangeEnd, Priority, CreatedAt, NotifiedAt
                     FROM WaitingList WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapWaitingListFromReader(reader);
        }
        
        return null;
    }

    public async Task<WaitingListDtoOut?> GetByIdDetailedAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT wl.Id, wl.CustomerId, 
                     c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     wl.ServiceId, s.Name as ServiceName,
                     wl.PreferredEmployeeId, e.FirstName + ' ' + e.LastName as PreferredEmployeeName,
                     wl.PreferredDate, wl.DateRangeStart, wl.DateRangeEnd, 
                     wl.Priority, wl.CreatedAt, wl.NotifiedAt
                     FROM WaitingList wl
                     INNER JOIN Customers c ON wl.CustomerId = c.Id
                     INNER JOIN Services s ON wl.ServiceId = s.Id
                     LEFT JOIN Employees e ON wl.PreferredEmployeeId = e.Id
                     WHERE wl.Id = @Id AND wl.IsActive = 1";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapWaitingListDtoFromReader(reader);
        }
        
        return null;
    }

    public async Task<IEnumerable<WaitingListDtoOut>> GetByCustomerAsync(int customerId)
    {
        var list = new List<WaitingListDtoOut>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT wl.Id, wl.CustomerId, 
                     c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     wl.ServiceId, s.Name as ServiceName,
                     wl.PreferredEmployeeId, e.FirstName + ' ' + e.LastName as PreferredEmployeeName,
                     wl.PreferredDate, wl.DateRangeStart, wl.DateRangeEnd, 
                     wl.Priority, wl.CreatedAt, wl.NotifiedAt
                     FROM WaitingList wl
                     INNER JOIN Customers c ON wl.CustomerId = c.Id
                     INNER JOIN Services s ON wl.ServiceId = s.Id
                     LEFT JOIN Employees e ON wl.PreferredEmployeeId = e.Id
                     WHERE wl.CustomerId = @CustomerId AND wl.IsActive = 1
                     ORDER BY wl.CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapWaitingListDtoFromReader(reader));
        }
        
        return list;
    }

    public async Task<IEnumerable<WaitingListDtoOut>> GetAllAsync()
    {
        var list = new List<WaitingListDtoOut>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT wl.Id, wl.CustomerId, 
                     c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     wl.ServiceId, s.Name as ServiceName,
                     wl.PreferredEmployeeId, e.FirstName + ' ' + e.LastName as PreferredEmployeeName,
                     wl.PreferredDate, wl.DateRangeStart, wl.DateRangeEnd, 
                     wl.Priority, wl.CreatedAt, wl.NotifiedAt
                     FROM WaitingList wl
                     INNER JOIN Customers c ON wl.CustomerId = c.Id
                     INNER JOIN Services s ON wl.ServiceId = s.Id
                     LEFT JOIN Employees e ON wl.PreferredEmployeeId = e.Id
                     WHERE wl.NotifiedAt IS NULL AND wl.IsActive = 1
                     ORDER BY wl.Priority, wl.CreatedAt";
        
        using var command = new SqlCommand(query, connection);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapWaitingListDtoFromReader(reader));
        }
        
        return list;
    }

    public async Task<IEnumerable<WaitingListDtoOut>> GetMatchingForSlotAsync(int serviceId, DateTime date, int? employeeId = null)
    {
        var list = new List<WaitingListDtoOut>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT wl.Id, wl.CustomerId, 
                     c.FirstName + ' ' + c.LastName as CustomerName, c.Category as CustomerCategory,
                     wl.ServiceId, s.Name as ServiceName,
                     wl.PreferredEmployeeId, e.FirstName + ' ' + e.LastName as PreferredEmployeeName,
                     wl.PreferredDate, wl.DateRangeStart, wl.DateRangeEnd, 
                     wl.Priority, wl.CreatedAt, wl.NotifiedAt
                     FROM WaitingList wl
                     INNER JOIN Customers c ON wl.CustomerId = c.Id
                     INNER JOIN Services s ON wl.ServiceId = s.Id
                     LEFT JOIN Employees e ON wl.PreferredEmployeeId = e.Id
                     WHERE wl.ServiceId = @ServiceId
                     AND wl.NotifiedAt IS NULL AND wl.IsActive = 1
                     AND @Date >= wl.DateRangeStart
                     AND @Date <= wl.DateRangeEnd
                     AND (wl.PreferredDate IS NULL OR wl.PreferredDate = @Date)";
        
        if (employeeId.HasValue)
        {
            query += " AND (wl.PreferredEmployeeId IS NULL OR wl.PreferredEmployeeId = @EmployeeId)";
        }
        
        query += " ORDER BY wl.Priority, wl.CreatedAt";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ServiceId", serviceId);
        command.Parameters.AddWithValue("@Date", date);
        
        if (employeeId.HasValue)
        {
            command.Parameters.AddWithValue("@EmployeeId", employeeId.Value);
        }
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapWaitingListDtoFromReader(reader));
        }
        
        return list;
    }

    public async Task<WaitingList?> CreateAsync(WaitingList waitingList)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"INSERT INTO WaitingList 
                     (CustomerId, ServiceId, PreferredEmployeeId, 
                      PreferredDate, DateRangeStart, DateRangeEnd, Priority, IsActive, CreatedAt)
                     VALUES 
                     (@CustomerId, @ServiceId, @PreferredEmployeeId, 
                      @PreferredDate, @DateRangeStart, @DateRangeEnd, @Priority, 1, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", waitingList.CustomerId);
        command.Parameters.AddWithValue("@ServiceId", waitingList.ServiceId);
        command.Parameters.AddWithValue("@PreferredEmployeeId", (object?)waitingList.PreferredEmployeeId ?? DBNull.Value);
        command.Parameters.AddWithValue("@PreferredDate", (object?)waitingList.PreferredDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@DateRangeStart", waitingList.DateRangeStart);
        command.Parameters.AddWithValue("@DateRangeEnd", waitingList.DateRangeEnd);
        command.Parameters.AddWithValue("@Priority", waitingList.Priority);
        command.Parameters.AddWithValue("@CreatedAt", waitingList.CreatedAt);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        waitingList.Id = newId;
        
        return waitingList;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "UPDATE WaitingList SET IsActive = 0 WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> MarkNotifiedAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "UPDATE WaitingList SET NotifiedAt = @NotifiedAt WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@NotifiedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> CalculatePriorityAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener categoría del cliente y número de servicios completados
        var query = @"SELECT c.Category, 
                     (SELECT COUNT(*) FROM Appointments a 
                      WHERE a.CustomerId = @CustomerId 
                      AND a.Status = 'completed') as CompletedServices
                     FROM Customers c
                     WHERE c.Id = @CustomerId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var category = reader.GetString(0);
            var completedServices = reader.GetInt32(1);
            
            // VIP = prioridad 1, más servicios = menor número de prioridad
            int basePriority = category == CustomerCategory.VIP ? 100 : 1000;
            return basePriority - Math.Min(completedServices, 99);
        }
        
        return 1000; // Prioridad por defecto
    }

    #region Helpers

    private static WaitingList MapWaitingListFromReader(SqlDataReader reader)
    {
        var waitingList = new WaitingList
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
            DateRangeStart = reader.GetDateTime(reader.GetOrdinal("DateRangeStart")),
            DateRangeEnd = reader.GetDateTime(reader.GetOrdinal("DateRangeEnd")),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var preferredEmployeeIdx = reader.GetOrdinal("PreferredEmployeeId");
        if (!reader.IsDBNull(preferredEmployeeIdx))
            waitingList.PreferredEmployeeId = reader.GetInt32(preferredEmployeeIdx);
        
        var preferredDateIdx = reader.GetOrdinal("PreferredDate");
        if (!reader.IsDBNull(preferredDateIdx))
            waitingList.PreferredDate = reader.GetDateTime(preferredDateIdx);
        
        var notifiedAtIdx = reader.GetOrdinal("NotifiedAt");
        if (!reader.IsDBNull(notifiedAtIdx))
            waitingList.NotifiedAt = reader.GetDateTime(notifiedAtIdx);
        
        return waitingList;
    }

    private static WaitingListDtoOut MapWaitingListDtoFromReader(SqlDataReader reader)
    {
        var dto = new WaitingListDtoOut
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
            ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
            ServiceName = reader.GetString(reader.GetOrdinal("ServiceName")),
            DateRangeStart = reader.GetDateTime(reader.GetOrdinal("DateRangeStart")),
            DateRangeEnd = reader.GetDateTime(reader.GetOrdinal("DateRangeEnd")),
            Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var customerCategoryIdx = reader.GetOrdinal("CustomerCategory");
        if (!reader.IsDBNull(customerCategoryIdx))
            dto.CustomerCategory = reader.GetString(customerCategoryIdx);
        
        var preferredEmployeeIdx = reader.GetOrdinal("PreferredEmployeeId");
        if (!reader.IsDBNull(preferredEmployeeIdx))
            dto.PreferredEmployeeId = reader.GetInt32(preferredEmployeeIdx);
        
        var preferredEmployeeNameIdx = reader.GetOrdinal("PreferredEmployeeName");
        if (!reader.IsDBNull(preferredEmployeeNameIdx))
            dto.PreferredEmployeeName = reader.GetString(preferredEmployeeNameIdx);
        
        var preferredDateIdx = reader.GetOrdinal("PreferredDate");
        if (!reader.IsDBNull(preferredDateIdx))
            dto.PreferredDate = reader.GetDateTime(preferredDateIdx);
        
        var notifiedAtIdx = reader.GetOrdinal("NotifiedAt");
        if (!reader.IsDBNull(notifiedAtIdx))
            dto.NotifiedAt = reader.GetDateTime(notifiedAtIdx);
        
        return dto;
    }

    #endregion
}
