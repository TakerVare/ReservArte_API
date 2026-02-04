using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region CRUD Productos

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.Id, p.Name, p.Description, p.Brand, p.Sku, p.Price, 
                     p.Stock, p.MinStockAlert, p.ImageUrl, p.CategoryId, p.IsActive, 
                     p.CreatedAt, p.UpdatedAt, c.Name as CategoryName
                     FROM Products p
                     LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                     WHERE p.Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapProductFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(bool includeInactive = false)
    {
        var products = new List<Product>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.Id, p.Name, p.Description, p.Brand, p.Sku, p.Price, 
                     p.Stock, p.MinStockAlert, p.ImageUrl, p.CategoryId, p.IsActive, 
                     p.CreatedAt, p.UpdatedAt, c.Name as CategoryName
                     FROM Products p
                     LEFT JOIN ProductCategories c ON p.CategoryId = c.Id";

        if (!includeInactive)
        {
            query += " WHERE p.IsActive = 1";
        }

        query += " ORDER BY p.Name";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        var products = new List<Product>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.Id, p.Name, p.Description, p.Brand, p.Sku, p.Price, 
                     p.Stock, p.MinStockAlert, p.ImageUrl, p.CategoryId, p.IsActive, 
                     p.CreatedAt, p.UpdatedAt, c.Name as CategoryName
                     FROM Products p
                     LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                     WHERE p.CategoryId = @CategoryId AND p.IsActive = 1
                     ORDER BY p.Name";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CategoryId", categoryId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        var products = new List<Product>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.Id, p.Name, p.Description, p.Brand, p.Sku, p.Price, 
                     p.Stock, p.MinStockAlert, p.ImageUrl, p.CategoryId, p.IsActive, 
                     p.CreatedAt, p.UpdatedAt, c.Name as CategoryName
                     FROM Products p
                     LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                     WHERE p.IsActive = 1 
                     AND (p.Name LIKE @Search OR p.Sku LIKE @Search OR p.Brand LIKE @Search)
                     ORDER BY p.Name";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"INSERT INTO Products (Name, Description, Brand, Sku, Price, Stock, 
                     MinStockAlert, ImageUrl, CategoryId, IsActive, CreatedAt)
                     VALUES (@Name, @Description, @Brand, @Sku, @Price, @Stock, 
                     @MinStockAlert, @ImageUrl, @CategoryId, @IsActive, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Brand", (object?)product.Brand ?? DBNull.Value);
        command.Parameters.AddWithValue("@Sku", (object?)product.Sku ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Stock", product.Stock);
        command.Parameters.AddWithValue("@MinStockAlert", product.MinStockAlert);
        command.Parameters.AddWithValue("@ImageUrl", (object?)product.ImageUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", (object?)product.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", product.IsActive);
        command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        product.Id = newId;

        return product;
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"UPDATE Products SET
                     Name = @Name, Description = @Description, Brand = @Brand, 
                     Sku = @Sku, Price = @Price, MinStockAlert = @MinStockAlert,
                     ImageUrl = @ImageUrl, CategoryId = @CategoryId, 
                     IsActive = @IsActive, UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Brand", (object?)product.Brand ?? DBNull.Value);
        command.Parameters.AddWithValue("@Sku", (object?)product.Sku ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@MinStockAlert", product.MinStockAlert);
        command.Parameters.AddWithValue("@ImageUrl", (object?)product.ImageUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", (object?)product.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", product.IsActive);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Soft delete
        var query = "UPDATE Products SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Stock

    public async Task<bool> UpdateStockAsync(int productId, int quantityChange)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"UPDATE Products SET 
                     Stock = Stock + @QuantityChange, UpdatedAt = @UpdatedAt 
                     WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", productId);
        command.Parameters.AddWithValue("@QuantityChange", quantityChange);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync()
    {
        var products = new List<Product>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"SELECT p.Id, p.Name, p.Description, p.Brand, p.Sku, p.Price, 
                     p.Stock, p.MinStockAlert, p.ImageUrl, p.CategoryId, p.IsActive, 
                     p.CreatedAt, p.UpdatedAt, c.Name as CategoryName
                     FROM Products p
                     LEFT JOIN ProductCategories c ON p.CategoryId = c.Id
                     WHERE p.IsActive = 1 AND p.Stock <= p.MinStockAlert
                     ORDER BY (p.MinStockAlert - p.Stock) DESC";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(MapProductFromReader(reader));
        }

        return products;
    }

    #endregion

    #region Categorías

    public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync(bool includeInactive = false)
    {
        var categories = new List<ProductCategory>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT Id, Name, Description, IsActive, CreatedAt FROM ProductCategories";
        if (!includeInactive)
        {
            query += " WHERE IsActive = 1";
        }
        query += " ORDER BY Name";

        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(new ProductCategory
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetBoolean(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return categories;
    }

    public async Task<ProductCategory?> GetCategoryByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT Id, Name, Description, IsActive, CreatedAt FROM ProductCategories WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ProductCategory
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetBoolean(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public async Task<ProductCategory> CreateCategoryAsync(ProductCategory category)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"INSERT INTO ProductCategories (Name, Description, IsActive, CreatedAt)
                     VALUES (@Name, @Description, @IsActive, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Name", category.Name);
        command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", category.IsActive);
        command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);

        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        category.Id = newId;

        return category;
    }

    public async Task<bool> UpdateCategoryAsync(ProductCategory category)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"UPDATE ProductCategories SET
                     Name = @Name, Description = @Description, IsActive = @IsActive
                     WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", category.Id);
        command.Parameters.AddWithValue("@Name", category.Name);
        command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", category.IsActive);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Soft delete
        var query = "UPDATE ProductCategories SET IsActive = 0 WHERE Id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<int> GetCategoryProductCountAsync(int categoryId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId AND IsActive = 1";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CategoryId", categoryId);

        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    #endregion

    #region Helpers

    private static Product MapProductFromReader(SqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Brand = reader.IsDBNull(reader.GetOrdinal("Brand")) ? null : reader.GetString(reader.GetOrdinal("Brand")),
            Sku = reader.IsDBNull(reader.GetOrdinal("Sku")) ? null : reader.GetString(reader.GetOrdinal("Sku")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
            MinStockAlert = reader.GetInt32(reader.GetOrdinal("MinStockAlert")),
            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName"))
        };
    }

    #endregion
}
