using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface ICustomerRepository
{
    #region Customer CRUD
    
    Task<IEnumerable<CustomerListDtoOut>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> CreateAsync(Customer customer);
    Task<Customer?> UpdateAsync(int id, Customer customer);
    Task<bool> DeleteAsync(int id);
    
    #endregion

    #region Customer Notes
    
    Task<IEnumerable<CustomerNote>> GetNotesByCustomerIdAsync(int customerId);
    Task<CustomerNote?> CreateNoteAsync(CustomerNote note);
    Task<bool> DeleteNoteAsync(int id);
    
    #endregion

    #region Customer Allergies
    
    Task<IEnumerable<CustomerAllergy>> GetAllergiesByCustomerIdAsync(int customerId);
    Task<CustomerAllergy?> CreateAllergyAsync(CustomerAllergy allergy);
    Task<CustomerAllergy?> UpdateAllergyAsync(int id, CustomerAllergy allergy);
    Task<bool> DeleteAllergyAsync(int id);
    
    #endregion

    #region Customer Consents
    
    Task<IEnumerable<CustomerConsent>> GetConsentsByCustomerIdAsync(int customerId);
    Task<CustomerConsent?> UpsertConsentAsync(CustomerConsent consent);
    
    #endregion

    #region Customer Payment Methods
    
    Task<IEnumerable<CustomerPaymentMethod>> GetPaymentMethodsByCustomerIdAsync(int customerId);
    Task<CustomerPaymentMethod?> CreatePaymentMethodAsync(CustomerPaymentMethod method);
    Task<bool> SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId);
    Task<bool> DeletePaymentMethodAsync(int id);
    
    #endregion

    #region Customer History
    
    Task<CustomerHistoryDtoOut> GetCustomerHistoryAsync(int customerId);
    
    #endregion

    #region Loyalty Points
    
    Task<bool> AddLoyaltyPointsAsync(int customerId, int points);
    Task<bool> DeductLoyaltyPointsAsync(int customerId, int points);
    
    #endregion

    #region Blocking
    
    Task<bool> BlockCustomerAsync(int customerId, string reason);
    Task<bool> UnblockCustomerAsync(int customerId);
    
    #endregion

    #region Category
    
    Task<bool> UpdateCategoryAsync(int customerId, string category);
    
    #endregion
}
