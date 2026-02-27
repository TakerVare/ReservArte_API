using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class MessageTemplateRepository : IMessageTemplateRepository
{
    private readonly string _connectionString;

    public MessageTemplateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<MessageTemplate?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "SELECT Id, Name, Type, Subject, Body, Language FROM MessageTemplates WHERE Id = @Id AND IsActive = 1";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapFromReader(reader);
        return null;
    }

    public async Task<IEnumerable<MessageTemplate>> GetAllAsync()
    {
        var list = new List<MessageTemplate>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "SELECT Id, Name, Type, Subject, Body, Language FROM MessageTemplates WHERE IsActive = 1 ORDER BY Name";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapFromReader(reader));
        return list;
    }

    public async Task<MessageTemplate?> CreateAsync(MessageTemplate template)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var id = template.Id == Guid.Empty ? Guid.NewGuid() : template.Id;
        var query = @"INSERT INTO MessageTemplates (Id, Name, Type, Subject, Body, Language, IsActive)
                     VALUES (@Id, @Name, @Type, @Subject, @Body, @Language, 1)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@Type", template.Type);
        command.Parameters.AddWithValue("@Subject", (object?)template.Subject ?? DBNull.Value);
        command.Parameters.AddWithValue("@Body", template.Body);
        command.Parameters.AddWithValue("@Language", template.Language);
        await command.ExecuteNonQueryAsync();
        template.Id = id;
        return template;
    }

    public async Task<MessageTemplate?> UpdateAsync(Guid id, MessageTemplate template)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"UPDATE MessageTemplates SET Name = @Name, Type = @Type, Subject = @Subject, Body = @Body, Language = @Language WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@Type", template.Type);
        command.Parameters.AddWithValue("@Subject", (object?)template.Subject ?? DBNull.Value);
        command.Parameters.AddWithValue("@Body", template.Body);
        command.Parameters.AddWithValue("@Language", template.Language);
        var rows = await command.ExecuteNonQueryAsync();
        if (rows > 0) { template.Id = id; return template; }
        return null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand("UPDATE MessageTemplates SET IsActive = 0 WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<IEnumerable<MessageTemplate>> GetByTypeAsync(string type)
    {
        var list = new List<MessageTemplate>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "SELECT Id, Name, Type, Subject, Body, Language FROM MessageTemplates WHERE Type = @Type AND IsActive = 1 ORDER BY Name";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Type", type);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapFromReader(reader));
        return list;
    }

    private static MessageTemplate MapFromReader(SqlDataReader reader)
    {
        return new MessageTemplate
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Type = reader.GetString(2),
            Subject = reader.IsDBNull(3) ? null : reader.GetString(3),
            Body = reader.GetString(4),
            Language = reader.GetString(5)
        };
    }
}
