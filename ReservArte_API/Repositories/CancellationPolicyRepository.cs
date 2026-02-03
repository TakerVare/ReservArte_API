using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class CancellationPolicyRepository : ICancellationPolicyRepository
{
    private readonly string _connectionString;

    public CancellationPolicyRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<CancellationPolicy?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, OrganizationId, MinHoursBeforeCancel, PenaltyPercentage,
                     MaxNoShowsBeforeBlock, VipMinHoursBeforeCancel, VipPenaltyPercentage,
                     IsActive, CreatedAt, UpdatedAt
                     FROM CancellationPolicies WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPolicyFromReader(reader);
        }
        
        return null;
    }

    public async Task<CancellationPolicy?> GetByOrganizationAsync(int organizationId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, OrganizationId, MinHoursBeforeCancel, PenaltyPercentage,
                     MaxNoShowsBeforeBlock, VipMinHoursBeforeCancel, VipPenaltyPercentage,
                     IsActive, CreatedAt, UpdatedAt
                     FROM CancellationPolicies 
                     WHERE OrganizationId = @OrganizationId AND IsActive = 1";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OrganizationId", organizationId);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPolicyFromReader(reader);
        }
        
        return null;
    }

    public async Task<CancellationPolicy?> CreateAsync(CancellationPolicy policy)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"INSERT INTO CancellationPolicies 
                     (OrganizationId, MinHoursBeforeCancel, PenaltyPercentage, MaxNoShowsBeforeBlock,
                      VipMinHoursBeforeCancel, VipPenaltyPercentage, IsActive, CreatedAt)
                     VALUES 
                     (@OrganizationId, @MinHoursBeforeCancel, @PenaltyPercentage, @MaxNoShowsBeforeBlock,
                      @VipMinHoursBeforeCancel, @VipPenaltyPercentage, @IsActive, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OrganizationId", policy.OrganizationId);
        command.Parameters.AddWithValue("@MinHoursBeforeCancel", policy.MinHoursBeforeCancel);
        command.Parameters.AddWithValue("@PenaltyPercentage", policy.PenaltyPercentage);
        command.Parameters.AddWithValue("@MaxNoShowsBeforeBlock", policy.MaxNoShowsBeforeBlock);
        command.Parameters.AddWithValue("@VipMinHoursBeforeCancel", (object?)policy.VipMinHoursBeforeCancel ?? DBNull.Value);
        command.Parameters.AddWithValue("@VipPenaltyPercentage", (object?)policy.VipPenaltyPercentage ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", policy.IsActive);
        command.Parameters.AddWithValue("@CreatedAt", policy.CreatedAt);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        policy.Id = newId;
        
        return policy;
    }

    public async Task<CancellationPolicy?> UpdateAsync(int id, CancellationPolicy policy)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE CancellationPolicies SET
                     MinHoursBeforeCancel = @MinHoursBeforeCancel,
                     PenaltyPercentage = @PenaltyPercentage,
                     MaxNoShowsBeforeBlock = @MaxNoShowsBeforeBlock,
                     VipMinHoursBeforeCancel = @VipMinHoursBeforeCancel,
                     VipPenaltyPercentage = @VipPenaltyPercentage,
                     IsActive = @IsActive,
                     UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@MinHoursBeforeCancel", policy.MinHoursBeforeCancel);
        command.Parameters.AddWithValue("@PenaltyPercentage", policy.PenaltyPercentage);
        command.Parameters.AddWithValue("@MaxNoShowsBeforeBlock", policy.MaxNoShowsBeforeBlock);
        command.Parameters.AddWithValue("@VipMinHoursBeforeCancel", (object?)policy.VipMinHoursBeforeCancel ?? DBNull.Value);
        command.Parameters.AddWithValue("@VipPenaltyPercentage", (object?)policy.VipPenaltyPercentage ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", policy.IsActive);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        if (rowsAffected > 0)
        {
            policy.Id = id;
            return policy;
        }
        
        return null;
    }

    public async Task<CancellationPolicy?> CreateOrUpdateAsync(CancellationPolicy policy)
    {
        var existing = await GetByOrganizationAsync(policy.OrganizationId);
        
        if (existing != null)
        {
            return await UpdateAsync(existing.Id, policy);
        }
        
        return await CreateAsync(policy);
    }

    #region Helpers

    private static CancellationPolicy MapPolicyFromReader(SqlDataReader reader)
    {
        var policy = new CancellationPolicy
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            OrganizationId = reader.GetInt32(reader.GetOrdinal("OrganizationId")),
            MinHoursBeforeCancel = reader.GetInt32(reader.GetOrdinal("MinHoursBeforeCancel")),
            PenaltyPercentage = reader.GetInt32(reader.GetOrdinal("PenaltyPercentage")),
            MaxNoShowsBeforeBlock = reader.GetInt32(reader.GetOrdinal("MaxNoShowsBeforeBlock")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var vipMinHoursIdx = reader.GetOrdinal("VipMinHoursBeforeCancel");
        if (!reader.IsDBNull(vipMinHoursIdx))
            policy.VipMinHoursBeforeCancel = reader.GetInt32(vipMinHoursIdx);
        
        var vipPenaltyIdx = reader.GetOrdinal("VipPenaltyPercentage");
        if (!reader.IsDBNull(vipPenaltyIdx))
            policy.VipPenaltyPercentage = reader.GetInt32(vipPenaltyIdx);
        
        var updatedAtIdx = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(updatedAtIdx))
            policy.UpdatedAt = reader.GetDateTime(updatedAtIdx);
        
        return policy;
    }

    #endregion
}
