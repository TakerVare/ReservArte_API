using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ReminderConfigurationRepository : IReminderConfigurationRepository
{
    private readonly string _connectionString;

    public ReminderConfigurationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<ReminderConfiguration?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT Id, ReminderOrder, HoursBeforeAppointment, Channel, IsActive, MessageTemplateId,
                     AllowedSendStartTime, AllowedSendEndTime FROM ReminderConfigurations WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapFromReader(reader);
        return null;
    }

    public async Task<IEnumerable<ReminderConfigurationDtoOut>> GetAllAsync()
    {
        var list = new List<ReminderConfigurationDtoOut>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT rc.Id, rc.ReminderOrder, rc.HoursBeforeAppointment, rc.Channel, rc.IsActive,
                     rc.MessageTemplateId, rc.AllowedSendStartTime, rc.AllowedSendEndTime, mt.Name AS MessageTemplateName
                     FROM ReminderConfigurations rc
                     LEFT JOIN MessageTemplates mt ON rc.MessageTemplateId = mt.Id
                     ORDER BY rc.ReminderOrder";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ReminderConfigurationDtoOut
            {
                Id = reader.GetGuid(0),
                ReminderOrder = reader.GetInt32(1),
                HoursBeforeAppointment = reader.GetInt32(2),
                Channel = reader.GetString(3),
                IsActive = reader.GetBoolean(4),
                MessageTemplateId = reader.GetGuid(5),
                AllowedSendStartTime = reader.IsDBNull(6) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(6)),
                AllowedSendEndTime = reader.IsDBNull(7) ? null : TimeOnly.FromTimeSpan(reader.GetTimeSpan(7)),
                MessageTemplateName = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }
        return list;
    }

    public async Task<ReminderConfiguration?> CreateAsync(ReminderConfiguration config)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"INSERT INTO ReminderConfigurations (Id, ReminderOrder, HoursBeforeAppointment, Channel, IsActive, MessageTemplateId, AllowedSendStartTime, AllowedSendEndTime)
                     VALUES (@Id, @ReminderOrder, @HoursBeforeAppointment, @Channel, @IsActive, @MessageTemplateId, @AllowedSendStartTime, @AllowedSendEndTime)";
        using var command = new SqlCommand(query, connection);
        var id = config.Id == Guid.Empty ? Guid.NewGuid() : config.Id;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@ReminderOrder", config.ReminderOrder);
        command.Parameters.AddWithValue("@HoursBeforeAppointment", config.HoursBeforeAppointment);
        command.Parameters.AddWithValue("@Channel", config.Channel);
        command.Parameters.AddWithValue("@IsActive", config.IsActive);
        command.Parameters.AddWithValue("@MessageTemplateId", config.MessageTemplateId);
        command.Parameters.AddWithValue("@AllowedSendStartTime", config.AllowedSendStartTime.HasValue ? config.AllowedSendStartTime.Value.ToTimeSpan() : (object)DBNull.Value);
        command.Parameters.AddWithValue("@AllowedSendEndTime", config.AllowedSendEndTime.HasValue ? config.AllowedSendEndTime.Value.ToTimeSpan() : (object)DBNull.Value);
        await command.ExecuteNonQueryAsync();
        config.Id = id;
        return config;
    }

    public async Task<ReminderConfiguration?> UpdateAsync(Guid id, ReminderConfiguration config)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"UPDATE ReminderConfigurations SET ReminderOrder = @ReminderOrder, HoursBeforeAppointment = @HoursBeforeAppointment,
                     Channel = @Channel, IsActive = @IsActive, MessageTemplateId = @MessageTemplateId,
                     AllowedSendStartTime = @AllowedSendStartTime, AllowedSendEndTime = @AllowedSendEndTime
                     WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@ReminderOrder", config.ReminderOrder);
        command.Parameters.AddWithValue("@HoursBeforeAppointment", config.HoursBeforeAppointment);
        command.Parameters.AddWithValue("@Channel", config.Channel);
        command.Parameters.AddWithValue("@IsActive", config.IsActive);
        command.Parameters.AddWithValue("@MessageTemplateId", config.MessageTemplateId);
        command.Parameters.AddWithValue("@AllowedSendStartTime", config.AllowedSendStartTime.HasValue ? config.AllowedSendStartTime.Value.ToTimeSpan() : (object)DBNull.Value);
        command.Parameters.AddWithValue("@AllowedSendEndTime", config.AllowedSendEndTime.HasValue ? config.AllowedSendEndTime.Value.ToTimeSpan() : (object)DBNull.Value);
        var rows = await command.ExecuteNonQueryAsync();
        if (rows > 0) { config.Id = id; return config; }
        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand("DELETE FROM ReminderConfigurations WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<IEnumerable<ReminderConfiguration>> GetActiveOrderedAsync()
    {
        var list = new List<ReminderConfiguration>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"SELECT Id, ReminderOrder, HoursBeforeAppointment, Channel, IsActive, MessageTemplateId, AllowedSendStartTime, AllowedSendEndTime
                     FROM ReminderConfigurations WHERE IsActive = 1 ORDER BY ReminderOrder";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapFromReader(reader));
        return list;
    }

    private static ReminderConfiguration MapFromReader(SqlDataReader reader)
    {
        var c = new ReminderConfiguration
        {
            Id = reader.GetGuid(0),
            ReminderOrder = reader.GetInt32(1),
            HoursBeforeAppointment = reader.GetInt32(2),
            Channel = reader.GetString(3),
            IsActive = reader.GetBoolean(4),
            MessageTemplateId = reader.GetGuid(5)
        };
        if (reader.FieldCount > 6 && !reader.IsDBNull(6))
            c.AllowedSendStartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(6));
        if (reader.FieldCount > 7 && !reader.IsDBNull(7))
            c.AllowedSendEndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(7));
        return c;
    }
}
