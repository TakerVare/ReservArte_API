using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ProductSaleRepository : IProductSaleRepository
{
    private readonly string _connectionString;

    public ProductSaleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<ProductSale?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT ps.Id, ps.CustomerId, ps.AppointmentId, ps.TotalAmount, 
                     ps.Status, ps.PaymentMethod, ps.Notes, ps.SoldBy, ps.CreatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName,
                     e.FirstName + ' ' + e.LastName as SoldByName
                     FROM ProductSales ps
                     LEFT JOIN Customers c ON ps.CustomerId = c.Id
                     LEFT JOIN Employees e ON ps.SoldBy = e.Id
                     WHERE ps.Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var sale = MapSaleFromReader(reader);
            sale.Items = (await GetSaleItemsAsync(id)).ToList();
            return sale;
        }

        return null;
    }

    public async Task<IEnumerable<ProductSale>> GetAllAsync(
        int? customerId = null,
        int? appointmentId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var sales = new List<ProductSale>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT ps.Id, ps.CustomerId, ps.AppointmentId, ps.TotalAmount, 
                     ps.Status, ps.PaymentMethod, ps.Notes, ps.SoldBy, ps.CreatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName,
                     e.FirstName + ' ' + e.LastName as SoldByName
                     FROM ProductSales ps
                     LEFT JOIN Customers c ON ps.CustomerId = c.Id
                     LEFT JOIN Employees e ON ps.SoldBy = e.Id
                     WHERE 1=1";

        if (customerId.HasValue)
        {
            query += " AND ps.CustomerId = @CustomerId";
        }
        if (appointmentId.HasValue)
        {
            query += " AND ps.AppointmentId = @AppointmentId";
        }
        if (!string.IsNullOrEmpty(status))
        {
            query += " AND ps.Status = @Status";
        }
        if (fromDate.HasValue)
        {
            query += " AND ps.CreatedAt >= @FromDate";
        }
        if (toDate.HasValue)
        {
            query += " AND ps.CreatedAt <= @ToDate";
        }

        query += " ORDER BY ps.CreatedAt DESC";

        using var command = new SqlCommand(query, connection);

        if (customerId.HasValue)
        {
            command.Parameters.AddWithValue("@CustomerId", customerId.Value);
        }
        if (appointmentId.HasValue)
        {
            command.Parameters.AddWithValue("@AppointmentId", appointmentId.Value);
        }
        if (!string.IsNullOrEmpty(status))
        {
            command.Parameters.AddWithValue("@Status", status);
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
            sales.Add(MapSaleFromReader(reader));
        }

        return sales;
    }

    public async Task<ProductSale> CreateAsync(ProductSale sale)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"INSERT INTO ProductSales (CustomerId, AppointmentId, TotalAmount, 
                     Status, PaymentMethod, Notes, SoldBy, CreatedAt)
                     VALUES (@CustomerId, @AppointmentId, @TotalAmount, 
                     @Status, @PaymentMethod, @Notes, @SoldBy, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", (object?)sale.CustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@AppointmentId", (object?)sale.AppointmentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
        command.Parameters.AddWithValue("@Status", sale.Status);
        command.Parameters.AddWithValue("@PaymentMethod", (object?)sale.PaymentMethod ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)sale.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@SoldBy", (object?)sale.SoldBy ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", sale.CreatedAt);

        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        sale.Id = newId;

        return sale;
    }

    public async Task<bool> AddSaleItemAsync(ProductSaleItem item)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"INSERT INTO ProductSaleItems (SaleId, ProductId, Quantity, UnitPrice, Subtotal)
                     VALUES (@SaleId, @ProductId, @Quantity, @UnitPrice, @Subtotal);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SaleId", item.SaleId);
        command.Parameters.AddWithValue("@ProductId", item.ProductId);
        command.Parameters.AddWithValue("@Quantity", item.Quantity);
        command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
        command.Parameters.AddWithValue("@Subtotal", item.Subtotal);

        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        item.Id = newId;

        return newId > 0;
    }

    public async Task<IEnumerable<ProductSaleItem>> GetSaleItemsAsync(int saleId)
    {
        var items = new List<ProductSaleItem>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT psi.Id, psi.SaleId, psi.ProductId, psi.Quantity, 
                     psi.UnitPrice, psi.Subtotal, p.Name as ProductName
                     FROM ProductSaleItems psi
                     INNER JOIN Products p ON psi.ProductId = p.Id
                     WHERE psi.SaleId = @SaleId";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SaleId", saleId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new ProductSaleItem
            {
                Id = reader.GetInt32(0),
                SaleId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                Quantity = reader.GetInt32(3),
                UnitPrice = reader.GetDecimal(4),
                Subtotal = reader.GetDecimal(5),
                ProductName = reader.GetString(6)
            });
        }

        return items;
    }

    public async Task<bool> UpdateStatusAsync(int saleId, string status)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "UPDATE ProductSales SET Status = @Status WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", saleId);
        command.Parameters.AddWithValue("@Status", status);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<(int totalSales, decimal totalAmount, int itemsSold)> GetSalesSummaryAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT 
                     COUNT(*) as TotalSales,
                     ISNULL(SUM(TotalAmount), 0) as TotalAmount,
                     ISNULL((SELECT SUM(Quantity) FROM ProductSaleItems psi 
                            INNER JOIN ProductSales ps ON psi.SaleId = ps.Id
                            WHERE ps.Status = 'Completed'";

        if (fromDate.HasValue)
        {
            query += " AND ps.CreatedAt >= @FromDate";
        }
        if (toDate.HasValue)
        {
            query += " AND ps.CreatedAt <= @ToDate";
        }

        query += @"), 0) as ItemsSold
                  FROM ProductSales WHERE Status = 'Completed'";

        if (fromDate.HasValue)
        {
            query += " AND CreatedAt >= @FromDate";
        }
        if (toDate.HasValue)
        {
            query += " AND CreatedAt <= @ToDate";
        }

        using var command = new SqlCommand(query, connection);

        if (fromDate.HasValue)
        {
            command.Parameters.AddWithValue("@FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            command.Parameters.AddWithValue("@ToDate", toDate.Value);
        }

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetInt32(0),
                reader.GetDecimal(1),
                reader.GetInt32(2)
            );
        }

        return (0, 0, 0);
    }

    private static ProductSale MapSaleFromReader(SqlDataReader reader)
    {
        return new ProductSale
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
            AppointmentId = reader.IsDBNull(reader.GetOrdinal("AppointmentId")) ? null : reader.GetInt32(reader.GetOrdinal("AppointmentId")),
            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            SoldBy = reader.IsDBNull(reader.GetOrdinal("SoldBy")) ? null : reader.GetInt32(reader.GetOrdinal("SoldBy")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
            SoldByName = reader.IsDBNull(reader.GetOrdinal("SoldByName")) ? null : reader.GetString(reader.GetOrdinal("SoldByName"))
        };
    }
}
