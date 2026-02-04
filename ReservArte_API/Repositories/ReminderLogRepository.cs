using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ReminderLogRepository : IReminderLogRepository
{
    private readonly string _connectionString;

    public ReminderLogRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<ReminderLog?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT Id, AppointmentId, ReminderConfigurationId, Channel, SentAt, Status, ExternalMessageId, ErrorMessage
                     FROM ReminderLogs WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapFromReader(reader);
        return null;
    }

    public async Task<IEnumerable<ReminderLogDtoOut>> GetByAppointmentIdAsync(int appointmentId)
    {
        var list = new List<ReminderLogDtoOut>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT Id, AppointmentId, ReminderConfigurationId, Channel, SentAt, Status, ExternalMessageId, ErrorMessage
                     FROM ReminderLogs WHERE AppointmentId = @AppointmentId ORDER BY SentAt DESC";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ReminderLogDtoOut
            {
                Id = reader.GetGuid(0),
                AppointmentId = reader.GetInt32(1),
                ReminderConfigurationId = reader.GetGuid(2),
                Channel = reader.GetString(3),
                SentAt = reader.GetDateTime(4),
                Status = reader.GetString(5),
                ExternalMessageId = reader.IsDBNull(6) ? null : reader.GetString(6),
                ErrorMessage = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        return list;
    }

    public async Task<ReminderLog> CreateAsync(ReminderLog log)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var id = log.Id == Guid.Empty ? Guid.NewGuid() : log.Id;
        var query = @"INSERT INTO ReminderLogs (Id, AppointmentId, ReminderConfigurationId, Channel, SentAt, Status, ExternalMessageId, ErrorMessage)
                     VALUES (@Id, @AppointmentId, @ReminderConfigurationId, @Channel, @SentAt, @Status, @ExternalMessageId, @ErrorMessage)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@AppointmentId", log.AppointmentId);
        command.Parameters.AddWithValue("@ReminderConfigurationId", log.ReminderConfigurationId);
        command.Parameters.AddWithValue("@Channel", log.Channel);
        command.Parameters.AddWithValue("@SentAt", log.SentAt);
        command.Parameters.AddWithValue("@Status", log.Status);
        command.Parameters.AddWithValue("@ExternalMessageId", (object?)log.ExternalMessageId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ErrorMessage", (object?)log.ErrorMessage ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
        log.Id = id;
        return log;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, string? externalMessageId = null, string? errorMessage = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"UPDATE ReminderLogs SET Status = @Status,
                     ExternalMessageId = COALESCE(@ExternalMessageId, ExternalMessageId),
                     ErrorMessage = COALESCE(@ErrorMessage, ErrorMessage)
                     WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@ExternalMessageId", (object?)externalMessageId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> WasReminderSentAsync(int appointmentId, Guid reminderConfigurationId, string channel)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT 1 FROM ReminderLogs WHERE AppointmentId = @AppointmentId AND ReminderConfigurationId = @ReminderConfigurationId AND Channel = @Channel";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        command.Parameters.AddWithValue("@ReminderConfigurationId", reminderConfigurationId);
        command.Parameters.AddWithValue("@Channel", channel);
        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync();
    }

    private static ReminderLog MapFromReader(SqlDataReader reader)
    {
        return new ReminderLog
        {
            Id = reader.GetGuid(0),
            AppointmentId = reader.GetInt32(1),
            ReminderConfigurationId = reader.GetGuid(2),
            Channel = reader.GetString(3),
            SentAt = reader.GetDateTime(4),
            Status = reader.GetString(5),
            ExternalMessageId = reader.IsDBNull(6) ? null : reader.GetString(6),
            ErrorMessage = reader.IsDBNull(7) ? null : reader.GetString(7)
        };
    }
}
