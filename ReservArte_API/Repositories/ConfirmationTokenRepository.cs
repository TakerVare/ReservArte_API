using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ConfirmationTokenRepository : IConfirmationTokenRepository
{
    private readonly string _connectionString;

    public ConfirmationTokenRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<ConfirmationToken?> GetByTokenAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "SELECT Token, AppointmentId, Action, ExpiresAt, UsedAt, CreatedAt FROM ConfirmationTokens WHERE Token = @Token";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ConfirmationToken
            {
                Token = reader.GetString(0),
                AppointmentId = reader.GetInt32(1),
                Action = reader.GetString(2),
                ExpiresAt = reader.GetDateTime(3),
                UsedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }
        return null;
    }

    public async Task<ConfirmationToken> CreateAsync(ConfirmationToken token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"INSERT INTO ConfirmationTokens (Token, AppointmentId, Action, ExpiresAt, UsedAt, CreatedAt)
                     VALUES (@Token, @AppointmentId, @Action, @ExpiresAt, @UsedAt, @CreatedAt)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token.Token);
        command.Parameters.AddWithValue("@AppointmentId", token.AppointmentId);
        command.Parameters.AddWithValue("@Action", token.Action);
        command.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);
        command.Parameters.AddWithValue("@UsedAt", (object?)token.UsedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", token.CreatedAt);
        await command.ExecuteNonQueryAsync();
        return token;
    }

    public async Task<bool> MarkAsUsedAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "UPDATE ConfirmationTokens SET UsedAt = @UsedAt WHERE Token = @Token";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Token", token);
        command.Parameters.AddWithValue("@UsedAt", DateTime.UtcNow);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<int> DeleteExpiredAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "DELETE FROM ConfirmationTokens WHERE ExpiresAt < @Now";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        return await command.ExecuteNonQueryAsync();
    }
}
