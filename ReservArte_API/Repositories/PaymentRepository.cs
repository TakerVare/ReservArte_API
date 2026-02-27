using Microsoft.Data.SqlClient;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;

    public PaymentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    #region CRUD Básico

    public async Task<Payment?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, AppointmentId, CustomerId, Amount, Currency, 
                     PaymentMethodType, Status, RedsysOrderNumber, RedsysAuthCode,
                     RedsysResponse, RedsysTransactionType, RedsysCardNumber,
                     CustomerPaymentMethodId, ProcessedAt, RefundedAmount, RefundedAt,
                     Metadata, Notes, RegisteredById, CreatedAt, UpdatedAt
                     FROM Payments WHERE Id = @Id AND IsActive = 1";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPaymentFromReader(reader);
        }
        
        return null;
    }

    public async Task<PaymentDtoOut?> GetByIdDetailedAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT p.Id, p.AppointmentId, p.CustomerId, p.Amount, p.Currency,
                     p.PaymentMethodType, p.Status, p.RedsysOrderNumber, p.RedsysAuthCode,
                     p.RedsysResponse, p.RedsysTransactionType, p.RedsysCardNumber,
                     p.CustomerPaymentMethodId, p.ProcessedAt, p.RefundedAmount, p.RefundedAt,
                     p.Metadata, p.Notes, p.RegisteredById, p.CreatedAt, p.UpdatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName,
                     a.AppointmentDate,
                     cpm.CardLast4, cpm.CardBrand,
                     u.FirstName + ' ' + u.LastName as RegisteredByName
                     FROM Payments p
                     INNER JOIN Customers c ON p.CustomerId = c.Id
                     LEFT JOIN Appointments a ON p.AppointmentId = a.Id
                     LEFT JOIN CustomerPaymentMethods cpm ON p.CustomerPaymentMethodId = cpm.Id
                     LEFT JOIN Users u ON p.RegisteredById = u.Id
                     WHERE p.Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPaymentDtoOutFromReader(reader);
        }
        
        return null;
    }

    public async Task<Payment?> CreateAsync(Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"INSERT INTO Payments (AppointmentId, CustomerId, Amount, Currency,
                     PaymentMethodType, Status, RedsysOrderNumber, RedsysAuthCode,
                     RedsysResponse, RedsysTransactionType, RedsysCardNumber,
                     CustomerPaymentMethodId, ProcessedAt, RefundedAmount, RefundedAt,
                     Metadata, Notes, RegisteredById, IsActive, CreatedAt)
                     VALUES (@AppointmentId, @CustomerId, @Amount, @Currency,
                     @PaymentMethodType, @Status, @RedsysOrderNumber, @RedsysAuthCode,
                     @RedsysResponse, @RedsysTransactionType, @RedsysCardNumber,
                     @CustomerPaymentMethodId, @ProcessedAt, @RefundedAmount, @RefundedAt,
                     @Metadata, @Notes, @RegisteredById, 1, @CreatedAt);
                     SELECT CAST(SCOPE_IDENTITY() as int)";
        
        using var command = new SqlCommand(query, connection);
        AddPaymentParameters(command, payment);
        
        var newId = (int)(await command.ExecuteScalarAsync() ?? 0);
        payment.Id = newId;
        
        return payment;
    }

    public async Task<Payment?> UpdateAsync(int id, Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE Payments SET
                     AppointmentId = @AppointmentId,
                     CustomerId = @CustomerId,
                     Amount = @Amount,
                     Currency = @Currency,
                     PaymentMethodType = @PaymentMethodType,
                     Status = @Status,
                     RedsysOrderNumber = @RedsysOrderNumber,
                     RedsysAuthCode = @RedsysAuthCode,
                     RedsysResponse = @RedsysResponse,
                     RedsysTransactionType = @RedsysTransactionType,
                     RedsysCardNumber = @RedsysCardNumber,
                     CustomerPaymentMethodId = @CustomerPaymentMethodId,
                     ProcessedAt = @ProcessedAt,
                     RefundedAmount = @RefundedAmount,
                     RefundedAt = @RefundedAt,
                     Metadata = @Metadata,
                     Notes = @Notes,
                     RegisteredById = @RegisteredById,
                     UpdatedAt = @UpdatedAt
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        AddPaymentParameters(command, payment);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        
        if (rowsAffected > 0)
        {
            payment.Id = id;
            return payment;
        }
        
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = "UPDATE Payments SET IsActive = 0 WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Consultas

    public async Task<IEnumerable<PaymentListDto>> GetAllAsync()
    {
        var payments = new List<PaymentListDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT p.Id, p.AppointmentId, p.CustomerId, p.Amount, p.Currency,
                     p.PaymentMethodType, p.Status, p.RedsysOrderNumber,
                     p.ProcessedAt, p.CreatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM Payments p
                     INNER JOIN Customers c ON p.CustomerId = c.Id
                     WHERE p.IsActive = 1
                     ORDER BY p.CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            payments.Add(MapPaymentListDtoFromReader(reader));
        }
        
        return payments;
    }

    public async Task<PaymentPagedResultDto> GetFilteredAsync(PaymentFilterDto filter)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var whereClause = BuildWhereClause(filter);
        var orderClause = BuildOrderClause(filter);
        
        // Query para contar total
        var countQuery = $@"SELECT COUNT(1), ISNULL(SUM(Amount), 0)
                           FROM Payments p
                           INNER JOIN Customers c ON p.CustomerId = c.Id
                           {whereClause}";
        
        using var countCommand = new SqlCommand(countQuery, connection);
        AddFilterParameters(countCommand, filter);
        
        int totalCount = 0;
        decimal totalAmount = 0;
        
        using (var countReader = await countCommand.ExecuteReaderAsync())
        {
            if (await countReader.ReadAsync())
            {
                totalCount = countReader.GetInt32(0);
                totalAmount = countReader.GetDecimal(1);
            }
        }
        
        // Query paginada
        var offset = (filter.Page - 1) * filter.PageSize;
        var query = $@"SELECT p.Id, p.AppointmentId, p.CustomerId, p.Amount, p.Currency,
                      p.PaymentMethodType, p.Status, p.RedsysOrderNumber,
                      p.ProcessedAt, p.CreatedAt,
                      c.FirstName + ' ' + c.LastName as CustomerName
                      FROM Payments p
                      INNER JOIN Customers c ON p.CustomerId = c.Id
                      {whereClause}
                      {orderClause}
                      OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        using var command = new SqlCommand(query, connection);
        AddFilterParameters(command, filter);
        command.Parameters.AddWithValue("@Offset", offset);
        command.Parameters.AddWithValue("@PageSize", filter.PageSize);
        
        var payments = new List<PaymentListDto>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            payments.Add(MapPaymentListDtoFromReader(reader));
        }
        
        return new PaymentPagedResultDto
        {
            Items = payments,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalAmount = totalAmount
        };
    }

    public async Task<IEnumerable<PaymentListDto>> GetByCustomerIdAsync(int customerId)
    {
        var payments = new List<PaymentListDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT p.Id, p.AppointmentId, p.CustomerId, p.Amount, p.Currency,
                     p.PaymentMethodType, p.Status, p.RedsysOrderNumber,
                     p.ProcessedAt, p.CreatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM Payments p
                     INNER JOIN Customers c ON p.CustomerId = c.Id
                     WHERE p.CustomerId = @CustomerId AND p.IsActive = 1
                     ORDER BY p.CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapPaymentListDtoFromReader(reader));
        }
        
        return payments;
    }

    public async Task<IEnumerable<PaymentListDto>> GetByAppointmentIdAsync(int appointmentId)
    {
        var payments = new List<PaymentListDto>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT p.Id, p.AppointmentId, p.CustomerId, p.Amount, p.Currency,
                     p.PaymentMethodType, p.Status, p.RedsysOrderNumber,
                     p.ProcessedAt, p.CreatedAt,
                     c.FirstName + ' ' + c.LastName as CustomerName
                     FROM Payments p
                     INNER JOIN Customers c ON p.CustomerId = c.Id
                     WHERE p.AppointmentId = @AppointmentId AND p.IsActive = 1
                     ORDER BY p.CreatedAt DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@AppointmentId", appointmentId);
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapPaymentListDtoFromReader(reader));
        }
        
        return payments;
    }

    public async Task<Payment?> GetByRedsysOrderNumberAsync(string orderNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT Id, AppointmentId, CustomerId, Amount, Currency, 
                     PaymentMethodType, Status, RedsysOrderNumber, RedsysAuthCode,
                     RedsysResponse, RedsysTransactionType, RedsysCardNumber,
                     CustomerPaymentMethodId, ProcessedAt, RefundedAmount, RefundedAt,
                     Metadata, Notes, RegisteredById, CreatedAt, UpdatedAt
                     FROM Payments WHERE RedsysOrderNumber = @OrderNumber";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@OrderNumber", orderNumber);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPaymentFromReader(reader);
        }
        
        return null;
    }

    #endregion

    #region Operaciones de Estado

    public async Task<bool> UpdateStatusAsync(int id, string status, string? redsysResponse = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE Payments SET 
                     Status = @Status,
                     RedsysResponse = CASE WHEN @RedsysResponse IS NOT NULL THEN @RedsysResponse ELSE RedsysResponse END,
                     ProcessedAt = CASE WHEN @Status IN ('captured', 'failed', 'cancelled') AND ProcessedAt IS NULL THEN @Now ELSE ProcessedAt END,
                     UpdatedAt = @Now
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@RedsysResponse", (object?)redsysResponse ?? DBNull.Value);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateRefundAsync(int id, decimal refundAmount)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"UPDATE Payments SET 
                     RefundedAmount = RefundedAmount + @RefundAmount,
                     RefundedAt = @Now,
                     Status = CASE 
                         WHEN RefundedAmount + @RefundAmount >= Amount THEN @RefundedStatus
                         ELSE @PartialStatus
                     END,
                     UpdatedAt = @Now
                     WHERE Id = @Id";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@RefundAmount", refundAmount);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow);
        command.Parameters.AddWithValue("@RefundedStatus", PaymentStatus.Refunded);
        command.Parameters.AddWithValue("@PartialStatus", PaymentStatus.PartiallyRefunded);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    #endregion

    #region Estadísticas

    public async Task<decimal> GetCustomerTotalSpentAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT ISNULL(SUM(Amount - RefundedAmount), 0)
                     FROM Payments 
                     WHERE CustomerId = @CustomerId 
                     AND Status = @CapturedStatus";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@CapturedStatus", PaymentStatus.Captured);
        
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? (decimal)result : 0;
    }

    public async Task<int> GetCustomerPaymentCountAsync(int customerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"SELECT COUNT(1) FROM Payments 
                     WHERE CustomerId = @CustomerId 
                     AND Status = @CapturedStatus";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@CapturedStatus", PaymentStatus.Captured);
        
        return (int)(await command.ExecuteScalarAsync() ?? 0);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
        
        // Obtener el último número del día
        var query = @"SELECT TOP 1 RedsysOrderNumber 
                     FROM Payments 
                     WHERE RedsysOrderNumber LIKE @Prefix + '%'
                     ORDER BY RedsysOrderNumber DESC";
        
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Prefix", datePrefix);
        
        var lastOrder = await command.ExecuteScalarAsync() as string;
        
        int sequence = 1;
        if (!string.IsNullOrEmpty(lastOrder) && lastOrder.Length == 12)
        {
            if (int.TryParse(lastOrder.Substring(8), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }
        
        // Formato: YYYYMMDD + 4 dígitos (12 caracteres total para Redsys)
        return $"{datePrefix}{sequence:D4}";
    }

    #endregion

    #region Helpers

    private static void AddPaymentParameters(SqlCommand command, Payment payment)
    {
        command.Parameters.AddWithValue("@AppointmentId", (object?)payment.AppointmentId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CustomerId", payment.CustomerId);
        command.Parameters.AddWithValue("@Amount", payment.Amount);
        command.Parameters.AddWithValue("@Currency", payment.Currency);
        command.Parameters.AddWithValue("@PaymentMethodType", payment.PaymentMethodType);
        command.Parameters.AddWithValue("@Status", payment.Status);
        command.Parameters.AddWithValue("@RedsysOrderNumber", (object?)payment.RedsysOrderNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysAuthCode", (object?)payment.RedsysAuthCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysResponse", (object?)payment.RedsysResponse ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysTransactionType", (object?)payment.RedsysTransactionType ?? DBNull.Value);
        command.Parameters.AddWithValue("@RedsysCardNumber", (object?)payment.RedsysCardNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@CustomerPaymentMethodId", (object?)payment.CustomerPaymentMethodId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ProcessedAt", (object?)payment.ProcessedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@RefundedAmount", payment.RefundedAmount);
        command.Parameters.AddWithValue("@RefundedAt", (object?)payment.RefundedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@Metadata", (object?)payment.Metadata ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)payment.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@RegisteredById", (object?)payment.RegisteredById ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", payment.CreatedAt);
    }

    private static Payment MapPaymentFromReader(SqlDataReader reader)
    {
        var payment = new Payment
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Currency = reader.GetString(reader.GetOrdinal("Currency")),
            PaymentMethodType = reader.GetString(reader.GetOrdinal("PaymentMethodType")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            RefundedAmount = reader.GetDecimal(reader.GetOrdinal("RefundedAmount")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var appointmentIdIdx = reader.GetOrdinal("AppointmentId");
        if (!reader.IsDBNull(appointmentIdIdx))
            payment.AppointmentId = reader.GetInt32(appointmentIdIdx);
        
        var redsysOrderIdx = reader.GetOrdinal("RedsysOrderNumber");
        if (!reader.IsDBNull(redsysOrderIdx))
            payment.RedsysOrderNumber = reader.GetString(redsysOrderIdx);
        
        var redsysAuthIdx = reader.GetOrdinal("RedsysAuthCode");
        if (!reader.IsDBNull(redsysAuthIdx))
            payment.RedsysAuthCode = reader.GetString(redsysAuthIdx);
        
        var redsysRespIdx = reader.GetOrdinal("RedsysResponse");
        if (!reader.IsDBNull(redsysRespIdx))
            payment.RedsysResponse = reader.GetString(redsysRespIdx);
        
        var redsysTxTypeIdx = reader.GetOrdinal("RedsysTransactionType");
        if (!reader.IsDBNull(redsysTxTypeIdx))
            payment.RedsysTransactionType = reader.GetString(redsysTxTypeIdx);
        
        var redsysCardIdx = reader.GetOrdinal("RedsysCardNumber");
        if (!reader.IsDBNull(redsysCardIdx))
            payment.RedsysCardNumber = reader.GetString(redsysCardIdx);
        
        var paymentMethodIdx = reader.GetOrdinal("CustomerPaymentMethodId");
        if (!reader.IsDBNull(paymentMethodIdx))
            payment.CustomerPaymentMethodId = reader.GetInt32(paymentMethodIdx);
        
        var processedAtIdx = reader.GetOrdinal("ProcessedAt");
        if (!reader.IsDBNull(processedAtIdx))
            payment.ProcessedAt = reader.GetDateTime(processedAtIdx);
        
        var refundedAtIdx = reader.GetOrdinal("RefundedAt");
        if (!reader.IsDBNull(refundedAtIdx))
            payment.RefundedAt = reader.GetDateTime(refundedAtIdx);
        
        var metadataIdx = reader.GetOrdinal("Metadata");
        if (!reader.IsDBNull(metadataIdx))
            payment.Metadata = reader.GetString(metadataIdx);
        
        var notesIdx = reader.GetOrdinal("Notes");
        if (!reader.IsDBNull(notesIdx))
            payment.Notes = reader.GetString(notesIdx);
        
        var registeredByIdx = reader.GetOrdinal("RegisteredById");
        if (!reader.IsDBNull(registeredByIdx))
            payment.RegisteredById = reader.GetInt32(registeredByIdx);
        
        var updatedAtIdx = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(updatedAtIdx))
            payment.UpdatedAt = reader.GetDateTime(updatedAtIdx);
        
        return payment;
    }

    private static PaymentDtoOut MapPaymentDtoOutFromReader(SqlDataReader reader)
    {
        var dto = new PaymentDtoOut
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Currency = reader.GetString(reader.GetOrdinal("Currency")),
            PaymentMethodType = reader.GetString(reader.GetOrdinal("PaymentMethodType")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            RefundedAmount = reader.GetDecimal(reader.GetOrdinal("RefundedAmount")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        // Campos nullables
        var appointmentIdIdx = reader.GetOrdinal("AppointmentId");
        if (!reader.IsDBNull(appointmentIdIdx))
            dto.AppointmentId = reader.GetInt32(appointmentIdIdx);
        
        var customerNameIdx = reader.GetOrdinal("CustomerName");
        if (!reader.IsDBNull(customerNameIdx))
            dto.CustomerName = reader.GetString(customerNameIdx);
        
        var appointmentDateIdx = reader.GetOrdinal("AppointmentDate");
        if (!reader.IsDBNull(appointmentDateIdx))
            dto.AppointmentDate = reader.GetDateTime(appointmentDateIdx).ToString("yyyy-MM-dd");
        
        var redsysOrderIdx = reader.GetOrdinal("RedsysOrderNumber");
        if (!reader.IsDBNull(redsysOrderIdx))
            dto.RedsysOrderNumber = reader.GetString(redsysOrderIdx);
        
        var redsysAuthIdx = reader.GetOrdinal("RedsysAuthCode");
        if (!reader.IsDBNull(redsysAuthIdx))
            dto.RedsysAuthCode = reader.GetString(redsysAuthIdx);
        
        var redsysRespIdx = reader.GetOrdinal("RedsysResponse");
        if (!reader.IsDBNull(redsysRespIdx))
            dto.RedsysResponse = reader.GetString(redsysRespIdx);
        
        var redsysTxTypeIdx = reader.GetOrdinal("RedsysTransactionType");
        if (!reader.IsDBNull(redsysTxTypeIdx))
            dto.RedsysTransactionType = reader.GetString(redsysTxTypeIdx);
        
        var redsysCardIdx = reader.GetOrdinal("RedsysCardNumber");
        if (!reader.IsDBNull(redsysCardIdx))
            dto.RedsysCardNumber = reader.GetString(redsysCardIdx);
        
        var paymentMethodIdx = reader.GetOrdinal("CustomerPaymentMethodId");
        if (!reader.IsDBNull(paymentMethodIdx))
            dto.CustomerPaymentMethodId = reader.GetInt32(paymentMethodIdx);
        
        var cardLast4Idx = reader.GetOrdinal("CardLast4");
        if (!reader.IsDBNull(cardLast4Idx))
            dto.CardLast4 = reader.GetString(cardLast4Idx);
        
        var cardBrandIdx = reader.GetOrdinal("CardBrand");
        if (!reader.IsDBNull(cardBrandIdx))
            dto.CardBrand = reader.GetString(cardBrandIdx);
        
        var processedAtIdx = reader.GetOrdinal("ProcessedAt");
        if (!reader.IsDBNull(processedAtIdx))
            dto.ProcessedAt = reader.GetDateTime(processedAtIdx);
        
        var refundedAtIdx = reader.GetOrdinal("RefundedAt");
        if (!reader.IsDBNull(refundedAtIdx))
            dto.RefundedAt = reader.GetDateTime(refundedAtIdx);
        
        var notesIdx = reader.GetOrdinal("Notes");
        if (!reader.IsDBNull(notesIdx))
            dto.Notes = reader.GetString(notesIdx);
        
        var registeredByIdx = reader.GetOrdinal("RegisteredById");
        if (!reader.IsDBNull(registeredByIdx))
            dto.RegisteredById = reader.GetInt32(registeredByIdx);
        
        var registeredByNameIdx = reader.GetOrdinal("RegisteredByName");
        if (!reader.IsDBNull(registeredByNameIdx))
            dto.RegisteredByName = reader.GetString(registeredByNameIdx);
        
        var updatedAtIdx = reader.GetOrdinal("UpdatedAt");
        if (!reader.IsDBNull(updatedAtIdx))
            dto.UpdatedAt = reader.GetDateTime(updatedAtIdx);
        
        return dto;
    }

    private static PaymentListDto MapPaymentListDtoFromReader(SqlDataReader reader)
    {
        var dto = new PaymentListDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Currency = reader.GetString(reader.GetOrdinal("Currency")),
            PaymentMethodType = reader.GetString(reader.GetOrdinal("PaymentMethodType")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
        
        var appointmentIdIdx = reader.GetOrdinal("AppointmentId");
        if (!reader.IsDBNull(appointmentIdIdx))
            dto.AppointmentId = reader.GetInt32(appointmentIdIdx);
        
        var customerNameIdx = reader.GetOrdinal("CustomerName");
        if (!reader.IsDBNull(customerNameIdx))
            dto.CustomerName = reader.GetString(customerNameIdx);
        
        var redsysOrderIdx = reader.GetOrdinal("RedsysOrderNumber");
        if (!reader.IsDBNull(redsysOrderIdx))
            dto.RedsysOrderNumber = reader.GetString(redsysOrderIdx);
        
        var processedAtIdx = reader.GetOrdinal("ProcessedAt");
        if (!reader.IsDBNull(processedAtIdx))
            dto.ProcessedAt = reader.GetDateTime(processedAtIdx);
        
        return dto;
    }

    private static string BuildWhereClause(PaymentFilterDto filter)
    {
        var conditions = new List<string> { "p.IsActive = 1" };
        
        if (filter.StartDate.HasValue)
            conditions.Add("CAST(p.CreatedAt AS DATE) >= @StartDate");
        
        if (filter.EndDate.HasValue)
            conditions.Add("CAST(p.CreatedAt AS DATE) <= @EndDate");
        
        if (!string.IsNullOrEmpty(filter.Status))
            conditions.Add("p.Status = @Status");
        
        if (!string.IsNullOrEmpty(filter.PaymentMethodType))
            conditions.Add("p.PaymentMethodType = @PaymentMethodType");
        
        if (filter.CustomerId.HasValue)
            conditions.Add("p.CustomerId = @CustomerId");
        
        if (filter.AppointmentId.HasValue)
            conditions.Add("p.AppointmentId = @AppointmentId");
        
        if (!string.IsNullOrEmpty(filter.RedsysOrderNumber))
            conditions.Add("p.RedsysOrderNumber LIKE @RedsysOrderNumber");
        
        if (filter.MinAmount.HasValue)
            conditions.Add("p.Amount >= @MinAmount");
        
        if (filter.MaxAmount.HasValue)
            conditions.Add("p.Amount <= @MaxAmount");
        
        return "WHERE " + string.Join(" AND ", conditions);
    }

    private static string BuildOrderClause(PaymentFilterDto filter)
    {
        var column = filter.OrderBy switch
        {
            "Amount" => "p.Amount",
            "Status" => "p.Status",
            "PaymentMethodType" => "p.PaymentMethodType",
            "ProcessedAt" => "p.ProcessedAt",
            _ => "p.CreatedAt"
        };
        
        var direction = filter.OrderDescending ? "DESC" : "ASC";
        return $"ORDER BY {column} {direction}";
    }

    private static void AddFilterParameters(SqlCommand command, PaymentFilterDto filter)
    {
        if (filter.StartDate.HasValue)
            command.Parameters.AddWithValue("@StartDate", filter.StartDate.Value.ToDateTime(TimeOnly.MinValue));
        
        if (filter.EndDate.HasValue)
            command.Parameters.AddWithValue("@EndDate", filter.EndDate.Value.ToDateTime(TimeOnly.MaxValue));
        
        if (!string.IsNullOrEmpty(filter.Status))
            command.Parameters.AddWithValue("@Status", filter.Status);
        
        if (!string.IsNullOrEmpty(filter.PaymentMethodType))
            command.Parameters.AddWithValue("@PaymentMethodType", filter.PaymentMethodType);
        
        if (filter.CustomerId.HasValue)
            command.Parameters.AddWithValue("@CustomerId", filter.CustomerId.Value);
        
        if (filter.AppointmentId.HasValue)
            command.Parameters.AddWithValue("@AppointmentId", filter.AppointmentId.Value);
        
        if (!string.IsNullOrEmpty(filter.RedsysOrderNumber))
            command.Parameters.AddWithValue("@RedsysOrderNumber", $"%{filter.RedsysOrderNumber}%");
        
        if (filter.MinAmount.HasValue)
            command.Parameters.AddWithValue("@MinAmount", filter.MinAmount.Value);
        
        if (filter.MaxAmount.HasValue)
            command.Parameters.AddWithValue("@MaxAmount", filter.MaxAmount.Value);
    }

    #endregion
}
