using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region Customer CRUD

    public async Task<IEnumerable<CustomerListDtoOut>> GetAllAsync()
    {
        var customers = new List<CustomerListDtoOut>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, FirstName, LastName, Email, Phone, Category, LoyaltyPoints, IsBlocked, CreatedAt 
                         FROM Customers WHERE Rol = @Rol
                         ORDER BY CreatedAt DESC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Rol", Roles.Client);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var firstName = reader.GetString(1);
                        var lastName = reader.GetString(2);
                        customers.Add(new CustomerListDtoOut
                        {
                            Id = reader.GetInt32(0),
                            FullName = $"{firstName} {lastName}".Trim(),
                            Email = reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Category = reader.GetString(5),
                            LoyaltyPoints = reader.GetInt32(6),
                            IsBlocked = reader.GetBoolean(7),
                            CreatedAt = reader.GetDateTime(8).ToString("yyyy-MM-ddTHH:mm:ssZ")
                        });
                    }
                }
            }
        }

        return customers;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Rol, c.ProfileImageUrl,
                                c.BirthDate, c.Category, c.LoyaltyPoints, c.IsBlocked, 
                                c.BlockedReason, c.PreferredContactMethod, c.MarketingConsent, c.CreatedAt
                         FROM Customers c WHERE c.Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Rol = reader.GetString(5),
                            ProfileImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                            BirthDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            Category = reader.GetString(8),
                            LoyaltyPoints = reader.GetInt32(9),
                            IsBlocked = reader.GetBoolean(10),
                            BlockedReason = reader.IsDBNull(11) ? null : reader.GetString(11),
                            PreferredContactMethod = reader.GetString(12),
                            MarketingConsent = reader.GetBoolean(13),
                            CreatedAt = reader.GetDateTime(14)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Rol, c.ProfileImageUrl,
                                c.BirthDate, c.Category, c.LoyaltyPoints, c.IsBlocked, 
                                c.BlockedReason, c.PreferredContactMethod, c.MarketingConsent, c.CreatedAt
                         FROM Customers c WHERE c.Email = @Email AND c.Rol = @Rol";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Rol", Roles.Client);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Rol = reader.GetString(5),
                            ProfileImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6),
                            BirthDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                            Category = reader.GetString(8),
                            LoyaltyPoints = reader.GetInt32(9),
                            IsBlocked = reader.GetBoolean(10),
                            BlockedReason = reader.IsDBNull(11) ? null : reader.GetString(11),
                            PreferredContactMethod = reader.GetString(12),
                            MarketingConsent = reader.GetBoolean(13),
                            CreatedAt = reader.GetDateTime(14)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<Customer?> CreateAsync(Customer customer)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO Customers (FirstName, LastName, Email, Phone, Rol, ProfileImageUrl,
                                BirthDate, Category, LoyaltyPoints, IsBlocked, BlockedReason,
                                PreferredContactMethod, MarketingConsent, CreatedAt)
                         VALUES (@FirstName, @LastName, @Email, @Phone, @Rol, @ProfileImageUrl,
                                @BirthDate, @Category, @LoyaltyPoints, @IsBlocked, @BlockedReason,
                                @PreferredContactMethod, @MarketingConsent, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                command.Parameters.AddWithValue("@LastName", customer.LastName);
                command.Parameters.AddWithValue("@Email", customer.Email);
                command.Parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@Rol", Roles.Client);
                command.Parameters.AddWithValue("@ProfileImageUrl", (object?)customer.ProfileImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", (object?)customer.BirthDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@Category", customer.Category);
                command.Parameters.AddWithValue("@LoyaltyPoints", customer.LoyaltyPoints);
                command.Parameters.AddWithValue("@IsBlocked", customer.IsBlocked);
                command.Parameters.AddWithValue("@BlockedReason", (object?)customer.BlockedReason ?? DBNull.Value);
                command.Parameters.AddWithValue("@PreferredContactMethod", customer.PreferredContactMethod);
                command.Parameters.AddWithValue("@MarketingConsent", customer.MarketingConsent);
                command.Parameters.AddWithValue("@CreatedAt", customer.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                customer.Id = newId;

                return customer;
            }
        }
    }

    public async Task<Customer?> UpdateAsync(int id, Customer customer)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Customers 
                         SET FirstName = @FirstName,
                             LastName = @LastName,
                             Email = @Email,
                             Phone = @Phone,
                             ProfileImageUrl = @ProfileImageUrl,
                             BirthDate = @BirthDate,
                             PreferredContactMethod = @PreferredContactMethod,
                             MarketingConsent = @MarketingConsent
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                command.Parameters.AddWithValue("@LastName", customer.LastName);
                command.Parameters.AddWithValue("@Email", customer.Email);
                command.Parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@ProfileImageUrl", (object?)customer.ProfileImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", (object?)customer.BirthDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@PreferredContactMethod", customer.PreferredContactMethod);
                command.Parameters.AddWithValue("@MarketingConsent", customer.MarketingConsent);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    customer.Id = id;
                    return customer;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM Customers WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Customer Notes

    public async Task<IEnumerable<CustomerNote>> GetNotesByCustomerIdAsync(int customerId)
    {
        var notes = new List<CustomerNote>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT cn.Id, cn.CustomerId, cn.EmployeeId, cn.Note, cn.CreatedAt,
                                CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName
                         FROM CustomerNotes cn
                         LEFT JOIN Employees e ON cn.EmployeeId = e.Id
                         WHERE cn.CustomerId = @CustomerId
                         ORDER BY cn.CreatedAt DESC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        notes.Add(new CustomerNote
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            EmployeeId = reader.GetInt32(2),
                            Note = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            EmployeeName = reader.IsDBNull(5) ? null : reader.GetString(5)
                        });
                    }
                }
            }
        }

        return notes;
    }

    public async Task<CustomerNote?> CreateNoteAsync(CustomerNote note)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO CustomerNotes (CustomerId, EmployeeId, Note, CreatedAt)
                         VALUES (@CustomerId, @EmployeeId, @Note, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", note.CustomerId);
                command.Parameters.AddWithValue("@EmployeeId", note.EmployeeId);
                command.Parameters.AddWithValue("@Note", note.Note);
                command.Parameters.AddWithValue("@CreatedAt", note.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                note.Id = newId;

                return note;
            }
        }
    }

    public async Task<bool> DeleteNoteAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM CustomerNotes WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Customer Allergies

    public async Task<IEnumerable<CustomerAllergy>> GetAllergiesByCustomerIdAsync(int customerId)
    {
        var allergies = new List<CustomerAllergy>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, CustomerId, AllergyDescription, Severity, CreatedAt
                         FROM CustomerAllergies WHERE CustomerId = @CustomerId
                         ORDER BY Severity DESC, CreatedAt DESC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        allergies.Add(new CustomerAllergy
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            AllergyDescription = reader.GetString(2),
                            Severity = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }
        }

        return allergies;
    }

    public async Task<CustomerAllergy?> CreateAllergyAsync(CustomerAllergy allergy)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"INSERT INTO CustomerAllergies (CustomerId, AllergyDescription, Severity, CreatedAt)
                         VALUES (@CustomerId, @AllergyDescription, @Severity, @CreatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", allergy.CustomerId);
                command.Parameters.AddWithValue("@AllergyDescription", allergy.AllergyDescription);
                command.Parameters.AddWithValue("@Severity", allergy.Severity);
                command.Parameters.AddWithValue("@CreatedAt", allergy.CreatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                allergy.Id = newId;

                return allergy;
            }
        }
    }

    public async Task<CustomerAllergy?> UpdateAllergyAsync(int id, CustomerAllergy allergy)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE CustomerAllergies 
                         SET AllergyDescription = @AllergyDescription,
                             Severity = @Severity
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@AllergyDescription", allergy.AllergyDescription);
                command.Parameters.AddWithValue("@Severity", allergy.Severity);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    allergy.Id = id;
                    return allergy;
                }
            }
        }

        return null;
    }

    public async Task<bool> DeleteAllergyAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM CustomerAllergies WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Customer Consents

    public async Task<IEnumerable<CustomerConsent>> GetConsentsByCustomerIdAsync(int customerId)
    {
        var consents = new List<CustomerConsent>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, CustomerId, ConsentType, IsGranted, GrantedAt, RevokedAt
                         FROM CustomerConsents WHERE CustomerId = @CustomerId
                         ORDER BY ConsentType";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        consents.Add(new CustomerConsent
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            ConsentType = reader.GetString(2),
                            IsGranted = reader.GetBoolean(3),
                            GrantedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                            RevokedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                        });
                    }
                }
            }
        }

        return consents;
    }

    public async Task<CustomerConsent?> UpsertConsentAsync(CustomerConsent consent)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            // Check if consent already exists
            var checkQuery = @"SELECT Id FROM CustomerConsents 
                              WHERE CustomerId = @CustomerId AND ConsentType = @ConsentType";

            int? existingId = null;
            using (var checkCommand = new SqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@CustomerId", consent.CustomerId);
                checkCommand.Parameters.AddWithValue("@ConsentType", consent.ConsentType);
                
                var result = await checkCommand.ExecuteScalarAsync();
                if (result != null)
                {
                    existingId = (int)result;
                }
            }

            if (existingId.HasValue)
            {
                // Update existing
                var updateQuery = @"UPDATE CustomerConsents 
                                   SET IsGranted = @IsGranted,
                                       GrantedAt = @GrantedAt,
                                       RevokedAt = @RevokedAt
                                   WHERE Id = @Id";

                using (var updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Id", existingId.Value);
                    updateCommand.Parameters.AddWithValue("@IsGranted", consent.IsGranted);
                    updateCommand.Parameters.AddWithValue("@GrantedAt", consent.IsGranted ? DateTime.UtcNow : (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue("@RevokedAt", !consent.IsGranted ? DateTime.UtcNow : (object)DBNull.Value);

                    await updateCommand.ExecuteNonQueryAsync();
                    consent.Id = existingId.Value;
                }
            }
            else
            {
                // Insert new
                var insertQuery = @"INSERT INTO CustomerConsents (CustomerId, ConsentType, IsGranted, GrantedAt, RevokedAt)
                                   VALUES (@CustomerId, @ConsentType, @IsGranted, @GrantedAt, @RevokedAt);
                                   SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@CustomerId", consent.CustomerId);
                    insertCommand.Parameters.AddWithValue("@ConsentType", consent.ConsentType);
                    insertCommand.Parameters.AddWithValue("@IsGranted", consent.IsGranted);
                    insertCommand.Parameters.AddWithValue("@GrantedAt", consent.IsGranted ? DateTime.UtcNow : (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@RevokedAt", !consent.IsGranted ? DateTime.UtcNow : (object)DBNull.Value);

                    var newId = (int)(await insertCommand.ExecuteScalarAsync() ?? 0);
                    consent.Id = newId;
                }
            }


            return consent;
        }
    }

    public async Task<bool> HasConsentAsync(int customerId, string consentType)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT IsGranted FROM CustomerConsents 
                         WHERE CustomerId = @CustomerId AND ConsentType = @ConsentType";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@ConsentType", consentType);

                var result = await command.ExecuteScalarAsync();
                return result != null && (bool)result;
            }
        }
    }

    #endregion

    #region Customer Payment Methods

    public async Task<IEnumerable<CustomerPaymentMethod>> GetPaymentMethodsByCustomerIdAsync(int customerId)
    {
        var methods = new List<CustomerPaymentMethod>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"SELECT Id, CustomerId, RedsysToken, RedsysCofTxnid, CardLast4, CardBrand, 
                                CardExpiry, IsDefault, CreatedAt, UpdatedAt
                         FROM CustomerPaymentMethods WHERE CustomerId = @CustomerId
                         ORDER BY IsDefault DESC, CreatedAt DESC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        methods.Add(new CustomerPaymentMethod
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            RedsysToken = reader.GetString(2),
                            RedsysCofTxnid = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CardLast4 = reader.GetString(4),
                            CardBrand = reader.GetString(5),
                            CardExpiry = reader.GetString(6),
                            IsDefault = reader.GetBoolean(7),
                            CreatedAt = reader.GetDateTime(8),
                            UpdatedAt = reader.GetDateTime(9)
                        });
                    }
                }
            }
        }

        return methods;
    }

    public async Task<CustomerPaymentMethod?> CreatePaymentMethodAsync(CustomerPaymentMethod method)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // If this is the first card or marked as default, update others
            if (method.IsDefault)
            {
                var resetQuery = "UPDATE CustomerPaymentMethods SET IsDefault = 0 WHERE CustomerId = @CustomerId";
                using (var resetCommand = new SqlCommand(resetQuery, connection))
                {
                    resetCommand.Parameters.AddWithValue("@CustomerId", method.CustomerId);
                    await resetCommand.ExecuteNonQueryAsync();
                }
            }

            var query = @"INSERT INTO CustomerPaymentMethods (CustomerId, RedsysToken, RedsysCofTxnid, CardLast4, 
                                CardBrand, CardExpiry, IsDefault, CreatedAt, UpdatedAt)
                         VALUES (@CustomerId, @RedsysToken, @RedsysCofTxnid, @CardLast4, 
                                @CardBrand, @CardExpiry, @IsDefault, @CreatedAt, @UpdatedAt);
                         SELECT CAST(SCOPE_IDENTITY() as int)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CustomerId", method.CustomerId);
                command.Parameters.AddWithValue("@RedsysToken", method.RedsysToken);
                command.Parameters.AddWithValue("@RedsysCofTxnid", (object?)method.RedsysCofTxnid ?? DBNull.Value);
                command.Parameters.AddWithValue("@CardLast4", method.CardLast4);
                command.Parameters.AddWithValue("@CardBrand", method.CardBrand);
                command.Parameters.AddWithValue("@CardExpiry", method.CardExpiry);
                command.Parameters.AddWithValue("@IsDefault", method.IsDefault);
                command.Parameters.AddWithValue("@CreatedAt", method.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", method.UpdatedAt);

                var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
                method.Id = newId;

                return method;
            }
        }
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Reset all to non-default
            var resetQuery = "UPDATE CustomerPaymentMethods SET IsDefault = 0 WHERE CustomerId = @CustomerId";
            using (var resetCommand = new SqlCommand(resetQuery, connection))
            {
                resetCommand.Parameters.AddWithValue("@CustomerId", customerId);
                await resetCommand.ExecuteNonQueryAsync();
            }

            // Set the specified one as default
            var query = @"UPDATE CustomerPaymentMethods 
                         SET IsDefault = 1, UpdatedAt = @UpdatedAt
                         WHERE Id = @Id AND CustomerId = @CustomerId";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", paymentMethodId);
                command.Parameters.AddWithValue("@CustomerId", customerId);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    public async Task<bool> DeletePaymentMethodAsync(int id)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "DELETE FROM CustomerPaymentMethods WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Customer History

    public async Task<CustomerHistoryDtoOut> GetCustomerHistoryAsync(int customerId)
    {
        var history = new CustomerHistoryDtoOut { CustomerId = customerId };

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Get customer name
            var customerQuery = "SELECT CONCAT(FirstName, ' ', LastName) FROM Customers WHERE Id = @CustomerId";
            using (var customerCommand = new SqlCommand(customerQuery, connection))
            {
                customerCommand.Parameters.AddWithValue("@CustomerId", customerId);
                var name = await customerCommand.ExecuteScalarAsync();
                history.CustomerName = name?.ToString() ?? "";
            }

            // Get appointment statistics
            var statsQuery = @"SELECT 
                                COUNT(*) as TotalAppointments,
                                SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as Completed,
                                SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END) as Cancelled,
                                SUM(CASE WHEN Status = 'NoShow' THEN 1 ELSE 0 END) as NoShows,
                                ISNULL(SUM(CASE WHEN Status = 'Completed' THEN TotalPrice ELSE 0 END), 0) as TotalSpent
                              FROM Appointments WHERE CustomerId = @CustomerId";

            using (var statsCommand = new SqlCommand(statsQuery, connection))
            {
                statsCommand.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await statsCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        history.TotalAppointments = reader.GetInt32(0);
                        history.CompletedAppointments = reader.GetInt32(1);
                        history.CancelledAppointments = reader.GetInt32(2);
                        history.NoShows = reader.GetInt32(3);
                        history.TotalSpent = reader.GetDecimal(4);
                    }
                }
            }

            // Get appointment details
            var appointmentsQuery = @"SELECT a.Id, a.AppointmentDate, a.StartTime, a.EndTime, 
                                            s.Name as ServiceName, CONCAT(e.FirstName, ' ', e.LastName) as EmployeeName,
                                            a.Status, a.TotalPrice
                                     FROM Appointments a
                                     LEFT JOIN Services s ON a.ServiceId = s.Id
                                     LEFT JOIN Employees e ON a.EmployeeId = e.Id
                                     WHERE a.CustomerId = @CustomerId
                                     ORDER BY a.AppointmentDate DESC, a.StartTime DESC";

            using (var appointmentsCommand = new SqlCommand(appointmentsQuery, connection))
            {
                appointmentsCommand.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await appointmentsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        history.Appointments.Add(new AppointmentHistoryItem
                        {
                            AppointmentId = reader.GetInt32(0),
                            Date = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                            StartTime = reader.GetTimeSpan(2).ToString(@"hh\:mm"),
                            EndTime = reader.GetTimeSpan(3).ToString(@"hh\:mm"),
                            ServiceName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            EmployeeName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Status = reader.GetString(6),
                            Price = reader.GetDecimal(7)
                        });
                    }
                }
            }
        }

        return history;
    }

    #endregion

    #region Loyalty Points

    public async Task<bool> AddLoyaltyPointsAsync(int customerId, int points)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "UPDATE Customers SET LoyaltyPoints = LoyaltyPoints + @Points WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", customerId);
                command.Parameters.AddWithValue("@Points", points);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    public async Task<bool> DeductLoyaltyPointsAsync(int customerId, int points)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            // Only deduct if customer has enough points
            var query = @"UPDATE Customers 
                         SET LoyaltyPoints = LoyaltyPoints - @Points 
                         WHERE Id = @Id AND LoyaltyPoints >= @Points";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", customerId);
                command.Parameters.AddWithValue("@Points", points);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Blocking

    public async Task<bool> BlockCustomerAsync(int customerId, string reason)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Customers 
                         SET IsBlocked = 1, 
                             BlockedReason = @Reason,
                             Category = @Category
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", customerId);
                command.Parameters.AddWithValue("@Reason", reason);
                command.Parameters.AddWithValue("@Category", CustomerCategory.Blocked);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    public async Task<bool> UnblockCustomerAsync(int customerId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = @"UPDATE Customers 
                         SET IsBlocked = 0, 
                             BlockedReason = NULL,
                             Category = @Category
                         WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", customerId);
                command.Parameters.AddWithValue("@Category", CustomerCategory.Regular);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    #endregion

    #region Category

    public async Task<bool> UpdateCategoryAsync(int customerId, string category)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            var query = "UPDATE Customers SET Category = @Category WHERE Id = @Id";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", customerId);
                command.Parameters.AddWithValue("@Category", category);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    #endregion
}
