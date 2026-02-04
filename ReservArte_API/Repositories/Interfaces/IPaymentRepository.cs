using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IPaymentRepository
{
    #region CRUD Básico
    
    Task<Payment?> GetByIdAsync(int id);
    Task<PaymentDtoOut?> GetByIdDetailedAsync(int id);
    Task<Payment?> CreateAsync(Payment payment);
    Task<Payment?> UpdateAsync(int id, Payment payment);
    Task<bool> DeleteAsync(int id);
    
    #endregion
    
    #region Consultas
    
    Task<IEnumerable<PaymentListDto>> GetAllAsync();
    Task<PaymentPagedResultDto> GetFilteredAsync(PaymentFilterDto filter);
    Task<IEnumerable<PaymentListDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<PaymentListDto>> GetByAppointmentIdAsync(int appointmentId);
    Task<Payment?> GetByRedsysOrderNumberAsync(string orderNumber);
    
    #endregion
    
    #region Operaciones de Estado
    
    Task<bool> UpdateStatusAsync(int id, string status, string? redsysResponse = null);
    Task<bool> UpdateRefundAsync(int id, decimal refundAmount);
    
    #endregion
    
    #region Estadísticas
    
    /// <summary>
    /// Obtiene el total de pagos capturados de un cliente
    /// </summary>
    Task<decimal> GetCustomerTotalSpentAsync(int customerId);
    
    /// <summary>
    /// Obtiene el número de pagos de un cliente
    /// </summary>
    Task<int> GetCustomerPaymentCountAsync(int customerId);
    
    /// <summary>
    /// Genera número de pedido único para Redsys
    /// Formato: YYYYMMDD + 4 dígitos secuenciales
    /// </summary>
    Task<string> GenerateOrderNumberAsync();
    
    #endregion
}
