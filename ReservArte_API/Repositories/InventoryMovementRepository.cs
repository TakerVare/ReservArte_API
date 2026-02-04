using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly string _connectionString;

    public InventoryMovementRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<InventoryMovement?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT im.Id, im.ProductId, im.Quantity, im.MovementType, 
                     im.ReferenceId, im.Notes, im.CreatedBy, im.CreatedAt,
                     p.Name as ProductName, e.FirstName + ' ' + e.LastName as CreatedByName
                     FROM InventoryMovements im
                     INNER JOIN Products p ON im.ProductId = p.Id
                     LEFT JOIN Employees e ON im.CreatedBy = e.Id
                     WHERE im.Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapMovementFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<InventoryMovement>> GetByProductIdAsync(int productId)
    {
        return await GetAllAsync(productId: productId);
    }

    public async Task<IEnumerable<InventoryMovement>> GetAllAsync(
        int? productId = null,
        string? movementType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var movements = new List<InventoryMovement>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT im.Id, im.ProductId, im.Quantity, im.MovementType, 
                     im.ReferenceId, im.Notes, im.CreatedBy, im.CreatedAt,
                     p.Name as ProductName, e.FirstName + ' ' + e.LastName as CreatedByName
                     FROM InventoryMovements im
                     INNER JOIN Products p ON im.ProductId = p.Id
                     LEFT JOIN Employees e ON im.CreatedBy = e.Id
                     WHERE 1=1";

        if (productId.HasValue)
        {
            query += " AND im.ProductId = @ProductId";
        }
        if (!string.IsNullOrEmpty(movementType))
        {
            query += " AND im.MovementType = @MovementType";
        }
        if (fromDate.HasValue)
        {
            query += " AND im.CreatedAt >= @FromDate";
        }
        if (toDate.HasValue)
        {
            query += " AND im.CreatedAt <= @ToDate";
        }

        query += " ORDER BY im.CreatedAt DESC";

        using var command = new SqlCommand(query, connection);

        if (productId.HasValue)
        {
            command.Parameters.AddWithValue("@ProductId", productId.Value);
        }
        if (!string.IsNullOrEmpty(movementType))
        {
            command.Parameters.AddWithValue("@MovementType", movementType);
        }
        if (fromDate.HasValue)
        {
            command.Parameters.AddWithValue("@FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            command.Parameters.AddWithValue("@ToDate", toDate.Value);
        }

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            movements.Add(MapMovementFromReader(reader));
        }

        return movements;
    }

    public async Task<InventoryMovement> CreateAsync(InventoryMovement movement)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"INSERT INTO InventoryMovements (ProductId, Quantity, MovementType, 
                     ReferenceId, Notes, CreatedBy, CreatedAt)
                     VALUES (@ProductId, @Quantity, @MovementType, 
                     @ReferenceId, @Notes, @CreatedBy, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ProductId", movement.ProductId);
        command.Parameters.AddWithValue("@Quantity", movement.Quantity);
        command.Parameters.AddWithValue("@MovementType", movement.MovementType);
        command.Parameters.AddWithValue("@ReferenceId", (object?)movement.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)movement.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", (object?)movement.CreatedBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", movement.CreatedAt);

        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        movement.Id = newId;

        return movement;
    }

    private static InventoryMovement MapMovementFromReader(SqlDataReader reader)
    {
        return new InventoryMovement
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
            MovementType = reader.GetString(reader.GetOrdinal("MovementType")),
            ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByName"))
        };
    }
}
