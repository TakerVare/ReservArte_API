using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ServicePhotoRepository : IServicePhotoRepository
{
    private readonly string _connectionString;

    public ServicePhotoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region CRUD Básico

    public async Task<ServicePhoto?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT sp.Id, sp.AppointmentId, sp.Type, sp.S3Key, sp.S3Bucket,
                     sp.UploadedBy, sp.UploadedAt, sp.IsPublic, sp.ExpiresAt,
                     e.FirstName + ' ' + e.LastName as EmployeeName,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM ServicePhotos sp
                     INNER JOIN Employees e ON sp.UploadedBy = e.Id
                     INNER JOIN Appointments a ON sp.AppointmentId = a.Id
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     WHERE sp.Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapServicePhotoFromReader(reader);
        }
        
        return null;
    }

    public async Task<ServicePhoto> CreateAsync(ServicePhoto photo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"INSERT INTO ServicePhotos (AppointmentId, Type, S3Key, S3Bucket, 
                     UploadedBy, UploadedAt, IsPublic, ExpiresAt)
                     VALUES (@AppointmentId, @Type, @S3Key, @S3Bucket,
                     @UploadedBy, @UploadedAt, @IsPublic, @ExpiresAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", photo.AppointmentId);
        command.Parameters.AddWithValue("@Type", photo.Type);
        command.Parameters.AddWithValue("@S3Key", photo.S3Key);
        command.Parameters.AddWithValue("@S3Bucket", photo.S3Bucket);
        command.Parameters.AddWithValue("@UploadedBy", photo.UploadedBy);
        command.Parameters.AddWithValue("@UploadedAt", photo.UploadedAt);
        command.Parameters.AddWithValue("@IsPublic", photo.IsPublic);
        command.Parameters.AddWithValue("@ExpiresAt", photo.ExpiresAt);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        photo.Id = newId;
        
        return photo;
    }

    public async Task<bool> UpdateAsync(ServicePhoto photo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE ServicePhotos SET
                     IsPublic = @IsPublic
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", photo.Id);
        command.Parameters.AddWithValue("@IsPublic", photo.IsPublic);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "DELETE FROM ServicePhotos WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Consultas por Relación

    public async Task<IEnumerable<ServicePhoto>> GetByAppointmentIdAsync(int appointmentId)
    {
        var photos = new List<ServicePhoto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT sp.Id, sp.AppointmentId, sp.Type, sp.S3Key, sp.S3Bucket,
                     sp.UploadedBy, sp.UploadedAt, sp.IsPublic, sp.ExpiresAt,
                     e.FirstName + ' ' + e.LastName as EmployeeName,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM ServicePhotos sp
                     INNER JOIN Employees e ON sp.UploadedBy = e.Id
                     INNER JOIN Appointments a ON sp.AppointmentId = a.Id
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     WHERE sp.AppointmentId = @AppointmentId
                     AND sp.ExpiresAt > @Now
                     ORDER BY sp.Type, sp.UploadedAt";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(MapServicePhotoFromReader(reader));
        }
        
        return photos;
    }

    public async Task<IEnumerable<ServicePhoto>> GetByCustomerIdAsync(int customerId)
    {
        var photos = new List<ServicePhoto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT sp.Id, sp.AppointmentId, sp.Type, sp.S3Key, sp.S3Bucket,
                     sp.UploadedBy, sp.UploadedAt, sp.IsPublic, sp.ExpiresAt,
                     e.FirstName + ' ' + e.LastName as EmployeeName,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM ServicePhotos sp
                     INNER JOIN Employees e ON sp.UploadedBy = e.Id
                     INNER JOIN Appointments a ON sp.AppointmentId = a.Id
                     INNER JOIN Customers c ON a.CustomerId = c.Id
                     WHERE a.CustomerId = @CustomerId
                     AND sp.ExpiresAt > @Now
                     ORDER BY sp.UploadedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(MapServicePhotoFromReader(reader));
        }
        
        return photos;
    }

    #endregion

    #region RGPD - Expiración Automática

    public async Task<IEnumerable<ServicePhoto>> GetExpiredPhotosAsync()
    {
        var photos = new List<ServicePhoto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT sp.Id, sp.AppointmentId, sp.Type, sp.S3Key, sp.S3Bucket,
                     sp.UploadedBy, sp.UploadedAt, sp.IsPublic, sp.ExpiresAt,
                     NULL as EmployeeName, NULL as CustomerName
                     FROM ServicePhotos sp
                     WHERE sp.ExpiresAt <= @Now";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            photos.Add(MapServicePhotoFromReader(reader));
        }
        
        return photos;
    }

    public async Task<int> DeleteExpiredAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "DELETE FROM ServicePhotos WHERE ExpiresAt <= @Now";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        
        return await command.ExecuteNonQueryAsync();
    }

    #endregion

    #region Auxiliares

    public async Task<int> GetCustomerIdByAppointmentAsync(int appointmentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT CustomerId FROM Appointments WHERE Id = @AppointmentId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        
        var result = await command.ExecuteScalarAsync();
        return result != null ? (int)result : 0;
    }

    #endregion

    #region Helpers

    private static ServicePhoto MapServicePhotoFromReader(SqlDataReader reader)
    {
        var photo = new ServicePhoto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
            Type = reader.GetString(reader.GetOrdinal("Type")),
            S3Key = reader.GetString(reader.GetOrdinal("S3Key")),
            S3Bucket = reader.GetString(reader.GetOrdinal("S3Bucket")),
            UploadedBy = reader.GetInt32(reader.GetOrdinal("UploadedBy")),
            UploadedAt = reader.GetDateTime(reader.GetOrdinal("UploadedAt")),
            IsPublic = reader.GetBoolean(reader.GetOrdinal("IsPublic")),
            ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
        };
        
        var employeeNameIdx = reader.GetOrdinal("EmployeeName");
        if (!reader.IsDBNull(employeeNameIdx))
            photo.EmployeeName = reader.GetString(employeeNameIdx);
        
        var customerNameIdx = reader.GetOrdinal("CustomerName");
        if (!reader.IsDBNull(customerNameIdx))
            photo.CustomerName = reader.GetString(customerNameIdx);
        
        return photo;
    }

    #endregion
}
