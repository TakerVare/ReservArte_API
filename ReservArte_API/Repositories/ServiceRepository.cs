using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly string _connectionString;

    public ServiceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region Service CRUD

    public async Task<IEnumerable<ServiceListDtoOut>> GetAllServicesAsync(int? categoryId = null, bool? isActive = null)
    {
        var services = new List<ServiceListDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT s.Id, s.Name, s.Description, s.DurationMinutes, s.BasePrice, 
                                sc.Name as CategoryName, s.ImageUrl, s.IsActive, s.RequiresAllergyTest
                         FROM Services s
                         LEFT JOIN ServiceCategories sc ON s.CategoryId = sc.Id
                         WHERE 1=1";

            if (categoryId.HasValue)
                query += " AND s.CategoryId = @CategoryId";
            if (isActive.HasValue)
                query += " AND s.IsActive = @IsActive";

            query += " ORDER BY sc.DisplayOrder, s.Name";

            using (var command = new SqlCommand(query, connection))
            {
                if (categoryId.HasValue)
                    command.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                if (isActive.HasValue)
                    command.Parameters.AddWithValue("@IsActive", isActive.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        services.Add(new ServiceListDtoOut
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DurationMinutes = reader.GetInt32(3),
                            BasePrice = reader.GetDecimal(4),
                            CategoryName = reader.IsDBNull(5) ? null : reader.GetString(5),
                            ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                            IsActive = reader.GetBoolean(7),
                            RequiresAllergyTest = reader.GetBoolean(8)
                        });
                    }
                }
            }
        }

        return services;
    }

    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT s.Id, s.Name, s.Description, s.DurationMinutes, 
                                s.BasePrice, s.CategoryId, s.ImageUrl, s.IsActive, s.RequiresAllergyTest,
                                s.AllergyTestHoursBefore, s.CreatedAt, s.UpdatedAt, sc.Name as CategoryName
                         FROM Services s
                         LEFT JOIN ServiceCategories sc ON s.CategoryId = sc.Id
                         WHERE s.Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Service
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            DurationMinutes = reader.GetInt32(3),
                            BasePrice = reader.GetDecimal(4),
                            CategoryId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                            IsActive = reader.GetBoolean(7),
                            RequiresAllergyTest = reader.GetBoolean(8),
                            AllergyTestHoursBefore = reader.GetInt32(9),
                            CreatedAt = reader.GetDateTime(10),
                            UpdatedAt = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                            CategoryName = reader.IsDBNull(12) ? null : reader.GetString(12)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<Service?> CreateServiceAsync(Service service)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO Services (Name, Description, DurationMinutes, BasePrice, 
                                CategoryId, ImageUrl, IsActive, RequiresAllergyTest, AllergyTestHoursBefore, CreatedAt)
                         VALUES (@Name, @Description, @DurationMinutes, @BasePrice, 
                                @CategoryId, @ImageUrl, @IsActive, @RequiresAllergyTest, @AllergyTestHoursBefore, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", service.Name);
                command.Parameters.AddWithValue("@Description", (object?)service.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@DurationMinutes", service.DurationMinutes);
                command.Parameters.AddWithValue("@BasePrice", service.BasePrice);
                command.Parameters.AddWithValue("@CategoryId", (object?)service.CategoryId ?? DBNull.Value);
                command.Parameters.AddWithValue("@ImageUrl", (object?)service.ImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", service.IsActive);
                command.Parameters.AddWithValue("@RequiresAllergyTest", service.RequiresAllergyTest);
                command.Parameters.AddWithValue("@AllergyTestHoursBefore", service.AllergyTestHoursBefore);
                command.Parameters.AddWithValue("@CreatedAt", service.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                service.Id = newId;

                return service;
            }
        }
    }

    public async Task<Service?> UpdateServiceAsync(int id, Service service)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Services 
                         SET Name = @Name,
                             Description = @Description,
                             DurationMinutes = @DurationMinutes,
                             BasePrice = @BasePrice,
                             CategoryId = @CategoryId,
                             ImageUrl = @ImageUrl,
                             IsActive = @IsActive,
                             RequiresAllergyTest = @RequiresAllergyTest,
                             AllergyTestHoursBefore = @AllergyTestHoursBefore,
                             UpdatedAt = @UpdatedAt
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", service.Name);
                command.Parameters.AddWithValue("@Description", (object?)service.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@DurationMinutes", service.DurationMinutes);
                command.Parameters.AddWithValue("@BasePrice", service.BasePrice);
                command.Parameters.AddWithValue("@CategoryId", (object?)service.CategoryId ?? DBNull.Value);
                command.Parameters.AddWithValue("@ImageUrl", (object?)service.ImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", service.IsActive);
                command.Parameters.AddWithValue("@RequiresAllergyTest", service.RequiresAllergyTest);
                command.Parameters.AddWithValue("@AllergyTestHoursBefore", service.AllergyTestHoursBefore);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    service.Id = id;
                    return service;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM Services WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServiceCategory CRUD

    public async Task<IEnumerable<ServiceCategoryDtoOut>> GetAllCategoriesAsync()
    {
        var categories = new List<ServiceCategoryDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, Name, Description, Color, DisplayOrder, IsActive, CreatedAt
                         FROM ServiceCategories
                         ORDER BY DisplayOrder, Name";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new ServiceCategoryDtoOut
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Color = reader.IsDBNull(3) ? null : reader.GetString(3),
                            DisplayOrder = reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return categories;
    }

    public async Task<ServiceCategory?> GetCategoryByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, Name, Description, Color, DisplayOrder, IsActive, CreatedAt
                         FROM ServiceCategories WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ServiceCategory
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Color = reader.IsDBNull(3) ? null : reader.GetString(3),
                            DisplayOrder = reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<ServiceCategory?> CreateCategoryAsync(ServiceCategory category)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServiceCategories (Name, Description, Color, DisplayOrder, IsActive, CreatedAt)
                         VALUES (@Name, @Description, @Color, @DisplayOrder, @IsActive, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", category.Name);
                command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Color", (object?)category.Color ?? DBNull.Value);
                command.Parameters.AddWithValue("@DisplayOrder", category.DisplayOrder);
                command.Parameters.AddWithValue("@IsActive", category.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                category.Id = newId;

                return category;
            }
        }
    }

    public async Task<ServiceCategory?> UpdateCategoryAsync(int id, ServiceCategory category)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE ServiceCategories 
                         SET Name = @Name,
                             Description = @Description,
                             Color = @Color,
                             DisplayOrder = @DisplayOrder,
                             IsActive = @IsActive
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", category.Name);
                command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Color", (object?)category.Color ?? DBNull.Value);
                command.Parameters.AddWithValue("@DisplayOrder", category.DisplayOrder);
                command.Parameters.AddWithValue("@IsActive", category.IsActive);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    category.Id = id;
                    return category;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServiceCategories WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServiceVariation

    public async Task<IEnumerable<ServiceVariation>> GetVariationsByServiceIdAsync(int serviceId)
    {
        var variations = new List<ServiceVariation>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, ServiceId, Name, PriceModifier, DurationModifier, IsActive, CreatedAt
                         FROM ServiceVariations WHERE ServiceId = @ServiceId
                         ORDER BY Name";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", serviceId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        variations.Add(new ServiceVariation
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            PriceModifier = reader.GetDecimal(3),
                            DurationModifier = reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6)
                        });
                    }
                }
            }
        }

        return variations;
    }

    public async Task<ServiceVariation?> GetVariationByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, ServiceId, Name, PriceModifier, DurationModifier, IsActive, CreatedAt
                         FROM ServiceVariations WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ServiceVariation
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            PriceModifier = reader.GetDecimal(3),
                            DurationModifier = reader.GetInt32(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<ServiceVariation?> CreateVariationAsync(ServiceVariation variation)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServiceVariations (ServiceId, Name, PriceModifier, DurationModifier, IsActive, CreatedAt)
                         VALUES (@ServiceId, @Name, @PriceModifier, @DurationModifier, @IsActive, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", variation.ServiceId);
                command.Parameters.AddWithValue("@Name", variation.Name);
                command.Parameters.AddWithValue("@PriceModifier", variation.PriceModifier);
                command.Parameters.AddWithValue("@DurationModifier", variation.DurationModifier);
                command.Parameters.AddWithValue("@IsActive", variation.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", variation.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                variation.Id = newId;

                return variation;
            }
        }
    }

    public async Task<ServiceVariation?> UpdateVariationAsync(int id, ServiceVariation variation)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE ServiceVariations 
                         SET Name = @Name,
                             PriceModifier = @PriceModifier,
                             DurationModifier = @DurationModifier,
                             IsActive = @IsActive
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", variation.Name);
                command.Parameters.AddWithValue("@PriceModifier", variation.PriceModifier);
                command.Parameters.AddWithValue("@DurationModifier", variation.DurationModifier);
                command.Parameters.AddWithValue("@IsActive", variation.IsActive);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    variation.Id = id;
                    return variation;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteVariationAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServiceVariations WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServicePricing

    public async Task<IEnumerable<ServicePricing>> GetPricingsByServiceIdAsync(int serviceId)
    {
        var pricings = new List<ServicePricing>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, ServiceId, EmployeeLevel, Price, CreatedAt
                         FROM ServicePricings WHERE ServiceId = @ServiceId
                         ORDER BY CASE EmployeeLevel 
                                    WHEN 'Junior' THEN 1 
                                    WHEN 'Senior' THEN 2 
                                    WHEN 'Expert' THEN 3 
                                  END";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", serviceId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pricings.Add(new ServicePricing
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.GetInt32(1),
                            EmployeeLevel = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }
        }

        return pricings;
    }

    public async Task<ServicePricing?> UpsertPricingAsync(ServicePricing pricing)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if pricing already exists for this service and level
            var checkQuery = @"SELECT Id FROM ServicePricings 
                              WHERE ServiceId = @ServiceId AND EmployeeLevel = @EmployeeLevel";

            int? existingId = null;
            using (var checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@ServiceId", pricing.ServiceId);
                checkCommand.Parameters.AddWithValue("@EmployeeLevel", pricing.EmployeeLevel);

                var result = await checkCommand.ExecuteScalarAsync();
                if (result != null)
                {
                    existingId = (int)result;
                }
            }

            if (existingId.HasValue)
            {
                // Update existing
                var updateQuery = @"UPDATE ServicePricings SET Price = @Price WHERE Id = @Id";

                using (var updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Id", existingId.Value);
                    updateCommand.Parameters.AddWithValue("@Price", pricing.Price);

                    await updateCommand.ExecuteNonQueryAsync();
                    pricing.Id = existingId.Value;
                }
            }
            else
            {
                // Insert new
                var insertQuery = @"INSERT INTO ServicePricings (ServiceId, EmployeeLevel, Price, CreatedAt)
                                   VALUES (@ServiceId, @EmployeeLevel, @Price, @CreatedAt);
                                   SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@ServiceId", pricing.ServiceId);
                    insertCommand.Parameters.AddWithValue("@EmployeeLevel", pricing.EmployeeLevel);
                    insertCommand.Parameters.AddWithValue("@Price", pricing.Price);
                    insertCommand.Parameters.AddWithValue("@CreatedAt", pricing.CreatedAt);

                    var newId = (int)(await insertCommand.ExecuteScalarAsync() ?? 0);
                    pricing.Id = newId;
                }
            }

            return pricing;
        }
    }

    public async Task<bool> DeletePricingAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServicePricings WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Product CRUD

    public async Task<IEnumerable<ProductDtoOut>> GetAllProductsAsync()
    {
        var products = new List<ProductDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, Name, Description, Brand, Sku, IsActive, CreatedAt
                         FROM Products
                         ORDER BY Name";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new ProductDtoOut
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Brand = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Sku = reader.IsDBNull(4) ? null : reader.GetString(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, Name, Description, Brand, Sku, IsActive, CreatedAt
                         FROM Products WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Brand = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Sku = reader.IsDBNull(4) ? null : reader.GetString(4),
                            IsActive = reader.GetBoolean(5),
                            CreatedAt = reader.GetDateTime(6)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<Product?> CreateProductAsync(Product product)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO Products (Name, Description, Brand, Sku, IsActive, CreatedAt)
                         VALUES (@Name, @Description, @Brand, @Sku, @IsActive, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Brand", (object?)product.Brand ?? DBNull.Value);
                command.Parameters.AddWithValue("@Sku", (object?)product.Sku ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", product.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                product.Id = newId;

                return product;
            }
        }
    }

    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Products 
                         SET Name = @Name,
                             Description = @Description,
                             Brand = @Brand,
                             Sku = @Sku,
                             IsActive = @IsActive
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Brand", (object?)product.Brand ?? DBNull.Value);
                command.Parameters.AddWithValue("@Sku", (object?)product.Sku ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", product.IsActive);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    product.Id = id;
                    return product;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM Products WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServiceProduct

    public async Task<IEnumerable<ServiceProduct>> GetProductsByServiceIdAsync(int serviceId)
    {
        var products = new List<ServiceProduct>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT sp.Id, sp.ServiceId, sp.ProductId, sp.QuantityUsed, sp.Notes,
                                p.Name as ProductName, p.Brand as ProductBrand
                         FROM ServiceProducts sp
                         INNER JOIN Products p ON sp.ProductId = p.Id
                         WHERE sp.ServiceId = @ServiceId
                         ORDER BY p.Name";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", serviceId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        products.Add(new ServiceProduct
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.GetInt32(1),
                            ProductId = reader.GetInt32(2),
                            QuantityUsed = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                            Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                            ProductName = reader.GetString(5),
                            ProductBrand = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }
        }

        return products;
    }

    public async Task<ServiceProduct?> CreateServiceProductAsync(ServiceProduct serviceProduct)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServiceProducts (ServiceId, ProductId, QuantityUsed, Notes)
                         VALUES (@ServiceId, @ProductId, @QuantityUsed, @Notes);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", serviceProduct.ServiceId);
                command.Parameters.AddWithValue("@ProductId", serviceProduct.ProductId);
                command.Parameters.AddWithValue("@QuantityUsed", (object?)serviceProduct.QuantityUsed ?? DBNull.Value);
                command.Parameters.AddWithValue("@Notes", (object?)serviceProduct.Notes ?? DBNull.Value);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                serviceProduct.Id = newId;

                return serviceProduct;
            }
        }
    }

    public async Task<bool> DeleteServiceProductAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServiceProducts WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServicePackage CRUD

    public async Task<IEnumerable<ServicePackageListDtoOut>> GetAllPackagesAsync(bool? isActive = null)
    {
        var packages = new List<ServicePackageListDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT sp.Id, sp.Name, sp.Description, sp.TotalPrice, sp.DiscountPercentage, 
                                sp.ImageUrl, sp.IsActive,
                                COUNT(spi.Id) as ServiceCount,
                                ISNULL(SUM(s.DurationMinutes), 0) as TotalDuration
                         FROM ServicePackages sp
                         LEFT JOIN ServicePackageItems spi ON sp.Id = spi.ServicePackageId
                         LEFT JOIN Services s ON spi.ServiceId = s.Id
                         WHERE 1=1";

            if (isActive.HasValue)
                query += " AND sp.IsActive = @IsActive";

            query += @" GROUP BY sp.Id, sp.Name, sp.Description, sp.TotalPrice, sp.DiscountPercentage, 
                               sp.ImageUrl, sp.IsActive
                        ORDER BY sp.Name";

            using (var command = new SqlCommand(query, connection))
            {
                if (isActive.HasValue)
                    command.Parameters.AddWithValue("@IsActive", isActive.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        packages.Add(new ServicePackageListDtoOut
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            TotalPrice = reader.GetDecimal(3),
                            DiscountPercentage = reader.GetDecimal(4),
                            ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                            IsActive = reader.GetBoolean(6),
                            ServiceCount = reader.GetInt32(7),
                            TotalDurationMinutes = reader.GetInt32(8)
                        });
                    }
                }
            }
        }

        return packages;
    }

    public async Task<ServicePackage?> GetPackageByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, Name, Description, TotalPrice, DiscountPercentage, 
                                ImageUrl, IsActive, CreatedAt, UpdatedAt
                         FROM ServicePackages WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ServicePackage
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            TotalPrice = reader.GetDecimal(3),
                            DiscountPercentage = reader.GetDecimal(4),
                            ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                            IsActive = reader.GetBoolean(6),
                            CreatedAt = reader.GetDateTime(7),
                            UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<ServicePackage?> CreatePackageAsync(ServicePackage package)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServicePackages (Name, Description, TotalPrice, 
                                DiscountPercentage, ImageUrl, IsActive, CreatedAt)
                         VALUES (@Name, @Description, @TotalPrice, 
                                @DiscountPercentage, @ImageUrl, @IsActive, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", package.Name);
                command.Parameters.AddWithValue("@Description", (object?)package.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@TotalPrice", package.TotalPrice);
                command.Parameters.AddWithValue("@DiscountPercentage", package.DiscountPercentage);
                command.Parameters.AddWithValue("@ImageUrl", (object?)package.ImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", package.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", package.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                package.Id = newId;

                return package;
            }
        }
    }

    public async Task<ServicePackage?> UpdatePackageAsync(int id, ServicePackage package)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE ServicePackages 
                         SET Name = @Name,
                             Description = @Description,
                             TotalPrice = @TotalPrice,
                             DiscountPercentage = @DiscountPercentage,
                             ImageUrl = @ImageUrl,
                             IsActive = @IsActive,
                             UpdatedAt = @UpdatedAt
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", package.Name);
                command.Parameters.AddWithValue("@Description", (object?)package.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@TotalPrice", package.TotalPrice);
                command.Parameters.AddWithValue("@DiscountPercentage", package.DiscountPercentage);
                command.Parameters.AddWithValue("@ImageUrl", (object?)package.ImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", package.IsActive);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    package.Id = id;
                    return package;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServicePackages WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServicePackageItem

    public async Task<IEnumerable<ServicePackageItem>> GetItemsByPackageIdAsync(int packageId)
    {
        var items = new List<ServicePackageItem>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT spi.Id, spi.ServicePackageId, spi.ServiceId, spi.[Order],
                                s.Name as ServiceName, s.BasePrice as ServicePrice, s.DurationMinutes
                         FROM ServicePackageItems spi
                         INNER JOIN Services s ON spi.ServiceId = s.Id
                         WHERE spi.ServicePackageId = @PackageId
                         ORDER BY spi.[Order]";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PackageId", packageId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new ServicePackageItem
                        {
                            Id = reader.GetInt32(0),
                            ServicePackageId = reader.GetInt32(1),
                            ServiceId = reader.GetInt32(2),
                            Order = reader.GetInt32(3),
                            ServiceName = reader.GetString(4),
                            ServicePrice = reader.GetDecimal(5),
                            ServiceDurationMinutes = reader.GetInt32(6)
                        });
                    }
                }
            }
        }

        return items;
    }

    public async Task<ServicePackageItem?> CreatePackageItemAsync(ServicePackageItem item)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServicePackageItems (ServicePackageId, ServiceId, [Order])
                         VALUES (@ServicePackageId, @ServiceId, @Order);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServicePackageId", item.ServicePackageId);
                command.Parameters.AddWithValue("@ServiceId", item.ServiceId);
                command.Parameters.AddWithValue("@Order", item.Order);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                item.Id = newId;

                return item;
            }
        }
    }

    public async Task<ServicePackageItem?> UpdatePackageItemOrderAsync(int id, int order)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "UPDATE ServicePackageItems SET [Order] = @Order WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Order", order);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return new ServicePackageItem { Id = id, Order = order };
                }
            }
        }

        return null;
    }

    public async Task<bool> DeletePackageItemAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServicePackageItems WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region ServicePromotion CRUD

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetAllPromotionsAsync(bool? activeOnly = null)
    {
        var promotions = new List<ServicePromotionDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT p.Id, p.ServiceId, s.Name as ServiceName, 
                                p.ServicePackageId, sp.Name as PackageName,
                                p.Name, p.Description, p.DiscountPercentage, p.DiscountAmount,
                                p.StartDate, p.EndDate, p.IsSeasonalService, p.IsActive, p.CreatedAt
                         FROM ServicePromotions p
                         LEFT JOIN Services s ON p.ServiceId = s.Id
                         LEFT JOIN ServicePackages sp ON p.ServicePackageId = sp.Id
                         WHERE 1=1";

            if (activeOnly == true)
                query += " AND p.IsActive = 1 AND GETUTCDATE() BETWEEN p.StartDate AND p.EndDate";

            query += " ORDER BY p.StartDate DESC";

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var startDate = reader.GetDateTime(9);
                        var endDate = reader.GetDateTime(10);
                        var isActive = reader.GetBoolean(12);

                        promotions.Add(new ServicePromotionDtoOut
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                            ServiceName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ServicePackageId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            PackageName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Name = reader.GetString(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DiscountPercentage = reader.GetDecimal(7),
                            DiscountAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                            StartDate = startDate.ToString("yyyy-MM-dd"),
                            EndDate = endDate.ToString("yyyy-MM-dd"),
                            IsSeasonalService = reader.GetBoolean(11),
                            IsActive = isActive,
                            IsCurrentlyActive = isActive && DateTime.UtcNow >= startDate && DateTime.UtcNow <= endDate,
                            CreatedAt = reader.GetDateTime(13).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return promotions;
    }

    public async Task<ServicePromotion?> GetPromotionByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT p.Id, p.ServiceId, s.Name as ServiceName, 
                                p.ServicePackageId, sp.Name as PackageName,
                                p.Name, p.Description, p.DiscountPercentage, p.DiscountAmount,
                                p.StartDate, p.EndDate, p.IsSeasonalService, p.IsActive, p.CreatedAt
                         FROM ServicePromotions p
                         LEFT JOIN Services s ON p.ServiceId = s.Id
                         LEFT JOIN ServicePackages sp ON p.ServicePackageId = sp.Id
                         WHERE p.Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ServicePromotion
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                            ServiceName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ServicePackageId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            PackageName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Name = reader.GetString(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DiscountPercentage = reader.GetDecimal(7),
                            DiscountAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                            StartDate = reader.GetDateTime(9),
                            EndDate = reader.GetDateTime(10),
                            IsSeasonalService = reader.GetBoolean(11),
                            IsActive = reader.GetBoolean(12),
                            CreatedAt = reader.GetDateTime(13)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<ServicePromotion?> CreatePromotionAsync(ServicePromotion promotion)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO ServicePromotions (ServiceId, ServicePackageId, Name, 
                                Description, DiscountPercentage, DiscountAmount, StartDate, EndDate, 
                                IsSeasonalService, IsActive, CreatedAt)
                         VALUES (@ServiceId, @ServicePackageId, @Name, 
                                @Description, @DiscountPercentage, @DiscountAmount, @StartDate, @EndDate, 
                                @IsSeasonalService, @IsActive, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", (object?)promotion.ServiceId ?? DBNull.Value);
                command.Parameters.AddWithValue("@ServicePackageId", (object?)promotion.ServicePackageId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Name", promotion.Name);
                command.Parameters.AddWithValue("@Description", (object?)promotion.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@DiscountPercentage", promotion.DiscountPercentage);
                command.Parameters.AddWithValue("@DiscountAmount", (object?)promotion.DiscountAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", promotion.StartDate);
                command.Parameters.AddWithValue("@EndDate", promotion.EndDate);
                command.Parameters.AddWithValue("@IsSeasonalService", promotion.IsSeasonalService);
                command.Parameters.AddWithValue("@IsActive", promotion.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", promotion.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                promotion.Id = newId;

                return promotion;
            }
        }
    }

    public async Task<ServicePromotion?> UpdatePromotionAsync(int id, ServicePromotion promotion)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE ServicePromotions 
                         SET ServiceId = @ServiceId,
                             ServicePackageId = @ServicePackageId,
                             Name = @Name,
                             Description = @Description,
                             DiscountPercentage = @DiscountPercentage,
                             DiscountAmount = @DiscountAmount,
                             StartDate = @StartDate,
                             EndDate = @EndDate,
                             IsSeasonalService = @IsSeasonalService,
                             IsActive = @IsActive
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ServiceId", (object?)promotion.ServiceId ?? DBNull.Value);
                command.Parameters.AddWithValue("@ServicePackageId", (object?)promotion.ServicePackageId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Name", promotion.Name);
                command.Parameters.AddWithValue("@Description", (object?)promotion.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@DiscountPercentage", promotion.DiscountPercentage);
                command.Parameters.AddWithValue("@DiscountAmount", (object?)promotion.DiscountAmount ?? DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", promotion.StartDate);
                command.Parameters.AddWithValue("@EndDate", promotion.EndDate);
                command.Parameters.AddWithValue("@IsSeasonalService", promotion.IsSeasonalService);
                command.Parameters.AddWithValue("@IsActive", promotion.IsActive);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    promotion.Id = id;
                    return promotion;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM ServicePromotions WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForServiceAsync(int serviceId)
    {
        var promotions = new List<ServicePromotionDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT p.Id, p.ServiceId, s.Name as ServiceName, 
                                p.ServicePackageId, sp.Name as PackageName,
                                p.Name, p.Description, p.DiscountPercentage, p.DiscountAmount,
                                p.StartDate, p.EndDate, p.IsSeasonalService, p.IsActive, p.CreatedAt
                         FROM ServicePromotions p
                         LEFT JOIN Services s ON p.ServiceId = s.Id
                         LEFT JOIN ServicePackages sp ON p.ServicePackageId = sp.Id
                         WHERE p.ServiceId = @ServiceId 
                           AND p.IsActive = 1 
                           AND GETUTCDATE() BETWEEN p.StartDate AND p.EndDate";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ServiceId", serviceId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        promotions.Add(new ServicePromotionDtoOut
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                            ServiceName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ServicePackageId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            PackageName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Name = reader.GetString(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DiscountPercentage = reader.GetDecimal(7),
                            DiscountAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                            StartDate = reader.GetDateTime(9).ToString("yyyy-MM-dd"),
                            EndDate = reader.GetDateTime(10).ToString("yyyy-MM-dd"),
                            IsSeasonalService = reader.GetBoolean(11),
                            IsActive = reader.GetBoolean(12),
                            IsCurrentlyActive = true,
                            CreatedAt = reader.GetDateTime(13).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return promotions;
    }

    public async Task<IEnumerable<ServicePromotionDtoOut>> GetActivePromotionsForPackageAsync(int packageId)
    {
        var promotions = new List<ServicePromotionDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT p.Id, p.ServiceId, s.Name as ServiceName, 
                                p.ServicePackageId, sp.Name as PackageName,
                                p.Name, p.Description, p.DiscountPercentage, p.DiscountAmount,
                                p.StartDate, p.EndDate, p.IsSeasonalService, p.IsActive, p.CreatedAt
                         FROM ServicePromotions p
                         LEFT JOIN Services s ON p.ServiceId = s.Id
                         LEFT JOIN ServicePackages sp ON p.ServicePackageId = sp.Id
                         WHERE p.ServicePackageId = @PackageId 
                           AND p.IsActive = 1 
                           AND GETUTCDATE() BETWEEN p.StartDate AND p.EndDate";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PackageId", packageId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        promotions.Add(new ServicePromotionDtoOut
                        {
                            Id = reader.GetInt32(0),
                            ServiceId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                            ServiceName = reader.IsDBNull(2) ? null : reader.GetString(2),
                            ServicePackageId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            PackageName = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Name = reader.GetString(5),
                            Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DiscountPercentage = reader.GetDecimal(7),
                            DiscountAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                            StartDate = reader.GetDateTime(9).ToString("yyyy-MM-dd"),
                            EndDate = reader.GetDateTime(10).ToString("yyyy-MM-dd"),
                            IsSeasonalService = reader.GetBoolean(11),
                            IsActive = reader.GetBoolean(12),
                            IsCurrentlyActive = true,
                            CreatedAt = reader.GetDateTime(13).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return promotions;
    }

    #endregion
}
