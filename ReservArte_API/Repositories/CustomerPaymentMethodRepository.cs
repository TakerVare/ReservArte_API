using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class CustomerPaymentMethodRepository : ICustomerPaymentMethodRepository
{
    private readonly string _connectionString;

    public CustomerPaymentMethodRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region CRUD

    public async Task<CustomerPaymentMethod?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, CustomerId, RedsysToken, RedsysCofTxnid, 
                     CardLast4, CardBrand, CardExpiry, IsDefault, CreatedAt, UpdatedAt
                     FROM CustomerPaymentMethods WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        
        return null;
    }

    public async Task<CustomerPaymentMethod?> CreateAsync(CustomerPaymentMethod paymentMethod)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Si es default, quitar default de los demás
        if (paymentMethod.IsDefault)
        {
            await ClearDefaultAsync(connection, paymentMethod.CustomerId);
        }
        
        var query = @"INSERT INTO CustomerPaymentMethods 
                     (CustomerId, RedsysToken, RedsysCofTxnid, CardLast4, CardBrand, CardExpiry, IsDefault, CreatedAt, UpdatedAt)
                     VALUES (@CustomerId, @RedsysToken, @RedsysCofTxnid, @CardLast4, @CardBrand, @CardExpiry, @IsDefault, @CreatedAt, @UpdatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", paymentMethod.CustomerId);
        command.Parameters.AddWithValue("@RedsysToken", paymentMethod.RedsysToken);
        command.Parameters.AddWithValue("@RedsysCofTxnid", (object?)paymentMethod.RedsysCofTxnid ?? DBNull.Value);
        command.Parameters.AddWithValue("@CardLast4", paymentMethod.CardLast4);
        command.Parameters.AddWithValue("@CardBrand", paymentMethod.CardBrand);
        command.Parameters.AddWithValue("@CardExpiry", paymentMethod.CardExpiry);
        command.Parameters.AddWithValue("@IsDefault", paymentMethod.IsDefault);
        command.Parameters.AddWithValue("@CreatedAt", paymentMethod.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", paymentMethod.UpdatedAt);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        paymentMethod.Id = newId;
        
        return paymentMethod;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "DELETE FROM CustomerPaymentMethods WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Consultas

    public async Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetByCustomerIdAsync(int customerId)
    {
        var methods = new List<CustomerPaymentMethodDtoOut>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, CustomerId, CardLast4, CardBrand, CardExpiry, IsDefault, CreatedAt
                     FROM CustomerPaymentMethods 
                     WHERE CustomerId = @CustomerId
                     ORDER BY IsDefault DESC, CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var expiry = reader.GetString(reader.GetOrdinal("CardExpiry"));
            methods.Add(new CustomerPaymentMethodDtoOut
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CardLast4 = reader.GetString(reader.GetOrdinal("CardLast4")),
                CardBrand = reader.GetString(reader.GetOrdinal("CardBrand")),
                CardExpiry = FormatExpiry(expiry),
                IsDefault = reader.GetBoolean(reader.GetOrdinal("IsDefault")),
                IsExpired = IsCardExpired(expiry),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }
        
        return methods;
    }

    public async Task<CustomerPaymentMethod?> GetDefaultByCustomerIdAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT TOP 1 Id, CustomerId, RedsysToken, RedsysCofTxnid, 
                     CardLast4, CardBrand, CardExpiry, IsDefault, CreatedAt, UpdatedAt
                     FROM CustomerPaymentMethods 
                     WHERE CustomerId = @CustomerId AND IsDefault = 1";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }
        
        return null;
    }

    public async Task<bool> CustomerHasPaymentMethodsAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT COUNT(1) FROM CustomerPaymentMethods WHERE CustomerId = @CustomerId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    public async Task<int> CountByCustomerIdAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "SELECT COUNT(1) FROM CustomerPaymentMethods WHERE CustomerId = @CustomerId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    #endregion

    #region Operaciones

    public async Task<bool> SetAsDefaultAsync(int id, int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Quitar default de todos los demás
        await ClearDefaultAsync(connection, customerId);
        
        // Establecer este como default
        var query = @"UPDATE CustomerPaymentMethods 
                     SET IsDefault = 1, UpdatedAt = @UpdatedAt 
                     WHERE Id = @Id AND CustomerId = @CustomerId";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> DeleteAllByCustomerIdAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "DELETE FROM CustomerPaymentMethods WHERE CustomerId = @CustomerId";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        return await command.ExecuteNonQueryAsync();
    }

    #endregion

    #region Helpers

    private static async Task ClearDefaultAsync(SqlConnection connection, int customerId)
    {
        var query = @"UPDATE CustomerPaymentMethods 
                     SET IsDefault = 0, UpdatedAt = @UpdatedAt 
                     WHERE CustomerId = @CustomerId AND IsDefault = 1";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        
        await command.ExecuteNonQueryAsync();
    }

    private static CustomerPaymentMethod MapFromReader(SqlDataReader reader)
    {
        var method = new CustomerPaymentMethod
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            RedsysToken = reader.GetString(reader.GetOrdinal("RedsysToken")),
            CardLast4 = reader.GetString(reader.GetOrdinal("CardLast4")),
            CardBrand = reader.GetString(reader.GetOrdinal("CardBrand")),
            CardExpiry = reader.GetString(reader.GetOrdinal("CardExpiry")),
            IsDefault = reader.GetBoolean(reader.GetOrdinal("IsDefault")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
        
        var cofTxnIdIdx = reader.GetOrdinal("RedsysCofTxnid");
        if (!reader.IsDBNull(cofTxnIdIdx))
            method.RedsysCofTxnid = reader.GetString(cofTxnIdIdx);
        
        return method;
    }

    private static string FormatExpiry(string expiry)
    {
        // Formato entrada: AAMM, Formato salida: MM/AA
        if (string.IsNullOrEmpty(expiry) || expiry.Length != 4)
            return "??/??";
        
        return $"{expiry.Substring(2, 2)}/{expiry.Substring(0, 2)}";
    }

    private static bool IsCardExpired(string expiry)
    {
        if (string.IsNullOrEmpty(expiry) || expiry.Length != 4)
            return true;
        
        if (!int.TryParse(expiry.Substring(0, 2), out int year) ||
            !int.TryParse(expiry.Substring(2, 2), out int month))
            return true;
        
        var expiryDate = new DateTime(2000 + year, month, 1).AddMonths(1).AddDays(-1);
        return expiryDate < DateTime.UtcNow.Date;
    }

    #endregion
}
