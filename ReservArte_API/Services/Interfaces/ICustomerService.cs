using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface ICustomerService
{
    #region Customer CRUD
    
    Task<IEnumerable<CustomerListDtoOut>> GetAllAsync(int? organizationId = null);
    Task<CustomerDtoOut?> GetByIdAsync(int id);
    Task<CustomerDtoOut?> CreateAsync(CustomerDtoIn customerDto);
    Task<CustomerDtoOut?> UpdateAsync(int id, CustomerDtoIn customerDto);
    Task<bool> DeleteAsync(int id);
    
    #endregion

    #region Customer Notes
    
    Task<IEnumerable<CustomerNoteDtoOut>> GetNotesByCustomerIdAsync(int customerId);
    Task<CustomerNoteDtoOut?> CreateNoteAsync(int customerId, int employeeId, CustomerNoteDtoIn noteDto);
    Task<bool> DeleteNoteAsync(int id);
    
    #endregion

    #region Customer Allergies
    
    Task<IEnumerable<CustomerAllergyDtoOut>> GetAllergiesByCustomerIdAsync(int customerId);
    Task<CustomerAllergyDtoOut?> CreateAllergyAsync(int customerId, CustomerAllergyDtoIn allergyDto);
    Task<CustomerAllergyDtoOut?> UpdateAllergyAsync(int id, CustomerAllergyDtoIn allergyDto);
    Task<bool> DeleteAllergyAsync(int id);
    
    #endregion

    #region Customer Consents
    
    Task<IEnumerable<CustomerConsentDtoOut>> GetConsentsByCustomerIdAsync(int customerId);
    Task<CustomerConsentDtoOut?> UpdateConsentAsync(int customerId, CustomerConsentDtoIn consentDto);
    
    #endregion

    #region Customer Payment Methods
    
    Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetPaymentMethodsByCustomerIdAsync(int customerId);
    Task<CustomerPaymentMethodDtoOut?> CreatePaymentMethodAsync(int customerId, CustomerPaymentMethodDtoIn methodDto);
    Task<bool> SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId);
    Task<bool> DeletePaymentMethodAsync(int id);
    
    #endregion

    #region Customer History
    
    Task<CustomerHistoryDtoOut?> GetCustomerHistoryAsync(int customerId);
    
    #endregion

    #region Loyalty Points
    
    Task<bool> AddLoyaltyPointsAsync(int customerId, CustomerLoyaltyDtoIn loyaltyDto);
    Task<bool> RedeemLoyaltyPointsAsync(int customerId, CustomerLoyaltyDtoIn loyaltyDto);
    Task<int> GetLoyaltyPointsAsync(int customerId);
    
    #endregion

    #region Blocking
    
    Task<bool> BlockCustomerAsync(int customerId, CustomerBlockDtoIn blockDto);
    Task<bool> UnblockCustomerAsync(int customerId);
    
    #endregion

    #region Category
    
    Task<bool> UpdateCategoryAsync(int customerId, string category);
    
    #endregion
}
