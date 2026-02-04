using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface ICustomerPaymentMethodRepository
{
    #region CRUD
    
    Task<CustomerPaymentMethod?> GetByIdAsync(int id);
    Task<CustomerPaymentMethod?> CreateAsync(CustomerPaymentMethod paymentMethod);
    Task<bool> DeleteAsync(int id);
    
    #endregion
    
    #region Consultas
    
    /// <summary>
    /// Obtiene todos los métodos de pago de un cliente
    /// </summary>
    Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Obtiene el método de pago por defecto de un cliente
    /// </summary>
    Task<CustomerPaymentMethod?> GetDefaultByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Verifica si un cliente tiene métodos de pago guardados
    /// </summary>
    Task<bool> CustomerHasPaymentMethodsAsync(int customerId);
    
    /// <summary>
    /// Cuenta los métodos de pago de un cliente
    /// </summary>
    Task<int> CountByCustomerIdAsync(int customerId);
    
    #endregion
    
    #region Operaciones
    
    /// <summary>
    /// Establece un método de pago como predeterminado
    /// </summary>
    Task<bool> SetAsDefaultAsync(int id, int customerId);
    
    /// <summary>
    /// Elimina todos los métodos de pago de un cliente
    /// </summary>
    Task<int> DeleteAllByCustomerIdAsync(int customerId);
    
    #endregion
}
