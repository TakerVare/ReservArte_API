using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _repository = customerRepository;
    }

    #region Customer CRUD

    public async Task<IEnumerable<CustomerListDtoOut>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<CustomerDtoOut?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null) return null;

        return MapToCustomerDtoOut(customer);
    }

    public async Task<CustomerDtoOut?> CreateAsync(CustomerDtoIn customerDto)
    {
        // Validate contact method
        if (!ContactMethod.IsValid(customerDto.PreferredContactMethod))
        {
            customerDto.PreferredContactMethod = ContactMethod.Email;
        }

        var customer = new Customer
        {
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            ProfileImageUrl = customerDto.ProfileImageUrl,
            BirthDate = customerDto.BirthDate,
            PreferredContactMethod = customerDto.PreferredContactMethod,
            MarketingConsent = customerDto.MarketingConsent,
            Category = CustomerCategory.New,
            LoyaltyPoints = 0,
            IsBlocked = false,
            Rol = Roles.Client,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(customer);
        if (created == null) return null;

        return MapToCustomerDtoOut(created);
    }

    public async Task<CustomerDtoOut?> UpdateAsync(int id, CustomerDtoIn customerDto)
    {
        var existingCustomer = await _repository.GetByIdAsync(id);
        if (existingCustomer == null) return null;

        // Validate contact method
        if (!ContactMethod.IsValid(customerDto.PreferredContactMethod))
        {
            customerDto.PreferredContactMethod = ContactMethod.Email;
        }

        existingCustomer.FirstName = customerDto.FirstName;
        existingCustomer.LastName = customerDto.LastName;
        existingCustomer.Email = customerDto.Email;
        existingCustomer.Phone = customerDto.Phone;
        existingCustomer.ProfileImageUrl = customerDto.ProfileImageUrl;
        existingCustomer.BirthDate = customerDto.BirthDate;
        existingCustomer.PreferredContactMethod = customerDto.PreferredContactMethod;
        existingCustomer.MarketingConsent = customerDto.MarketingConsent;

        var updated = await _repository.UpdateAsync(id, existingCustomer);
        if (updated == null) return null;

        return MapToCustomerDtoOut(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    #endregion

    #region Customer Notes

    public async Task<IEnumerable<CustomerNoteDtoOut>> GetNotesByCustomerIdAsync(int customerId)
    {
        var notes = await _repository.GetNotesByCustomerIdAsync(customerId);
        return notes.Select(MapToNoteDtoOut);
    }

    public async Task<CustomerNoteDtoOut?> CreateNoteAsync(int customerId, int employeeId, CustomerNoteDtoIn noteDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return null;

        var note = new CustomerNote
        {
            CustomerId = customerId,
            EmployeeId = employeeId,
            Note = noteDto.Note,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateNoteAsync(note);
        if (created == null) return null;

        return MapToNoteDtoOut(created);
    }

    public async Task<bool> DeleteNoteAsync(int id)
    {
        return await _repository.DeleteNoteAsync(id);
    }

    #endregion

    #region Customer Allergies

    public async Task<IEnumerable<CustomerAllergyDtoOut>> GetAllergiesByCustomerIdAsync(int customerId)
    {
        var allergies = await _repository.GetAllergiesByCustomerIdAsync(customerId);
        return allergies.Select(MapToAllergyDtoOut);
    }

    public async Task<CustomerAllergyDtoOut?> CreateAllergyAsync(int customerId, CustomerAllergyDtoIn allergyDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return null;

        // Validate severity
        if (!AllergySeverity.IsValid(allergyDto.Severity))
        {
            allergyDto.Severity = AllergySeverity.Low;
        }

        var allergy = new CustomerAllergy
        {
            CustomerId = customerId,
            AllergyDescription = allergyDto.AllergyDescription,
            Severity = allergyDto.Severity,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAllergyAsync(allergy);
        if (created == null) return null;

        return MapToAllergyDtoOut(created);
    }

    public async Task<CustomerAllergyDtoOut?> UpdateAllergyAsync(int id, CustomerAllergyDtoIn allergyDto)
    {
        // Validate severity
        if (!AllergySeverity.IsValid(allergyDto.Severity))
        {
            allergyDto.Severity = AllergySeverity.Low;
        }

        var allergy = new CustomerAllergy
        {
            Id = id,
            AllergyDescription = allergyDto.AllergyDescription,
            Severity = allergyDto.Severity
        };

        var updated = await _repository.UpdateAllergyAsync(id, allergy);
        if (updated == null) return null;

        return MapToAllergyDtoOut(updated);
    }

    public async Task<bool> DeleteAllergyAsync(int id)
    {
        return await _repository.DeleteAllergyAsync(id);
    }

    #endregion

    #region Customer Consents

    public async Task<IEnumerable<CustomerConsentDtoOut>> GetConsentsByCustomerIdAsync(int customerId)
    {
        var consents = await _repository.GetConsentsByCustomerIdAsync(customerId);
        return consents.Select(MapToConsentDtoOut);
    }

    public async Task<CustomerConsentDtoOut?> UpdateConsentAsync(int customerId, CustomerConsentDtoIn consentDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return null;

        // Validate consent type
        if (!ConsentType.IsValid(consentDto.ConsentType))
        {
            return null;
        }

        var consent = new CustomerConsent
        {
            CustomerId = customerId,
            ConsentType = consentDto.ConsentType,
            IsGranted = consentDto.IsGranted,
            GrantedAt = consentDto.IsGranted ? DateTime.UtcNow : null,
            RevokedAt = !consentDto.IsGranted ? DateTime.UtcNow : null
        };

        var updated = await _repository.UpsertConsentAsync(consent);
        if (updated == null) return null;

        return MapToConsentDtoOut(updated);
    }

    #endregion

    #region Customer Payment Methods

    public async Task<IEnumerable<CustomerPaymentMethodDtoOut>> GetPaymentMethodsByCustomerIdAsync(int customerId)
    {
        var methods = await _repository.GetPaymentMethodsByCustomerIdAsync(customerId);
        return methods.Select(MapToPaymentMethodDtoOut);
    }

    public async Task<CustomerPaymentMethodDtoOut?> CreatePaymentMethodAsync(int customerId, CustomerPaymentMethodDtoIn methodDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return null;

        // Check if customer has SavedCards consent
        var consents = await _repository.GetConsentsByCustomerIdAsync(customerId);
        var savedCardsConsent = consents.FirstOrDefault(c => c.ConsentType == ConsentType.SavedCards);
        if (savedCardsConsent == null || !savedCardsConsent.IsGranted)
        {
            // Customer hasn't granted consent to save cards
            return null;
        }

        var method = new CustomerPaymentMethod
        {
            CustomerId = customerId,
            RedsysToken = methodDto.RedsysToken,
            RedsysCofTxnid = methodDto.RedsysCofTxnid,
            CardLast4 = methodDto.CardLast4,
            CardBrand = methodDto.CardBrand,
            CardExpiry = methodDto.CardExpiry,
            IsDefault = methodDto.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreatePaymentMethodAsync(method);
        if (created == null) return null;

        return MapToPaymentMethodDtoOut(created);
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(int customerId, int paymentMethodId)
    {
        return await _repository.SetDefaultPaymentMethodAsync(customerId, paymentMethodId);
    }

    public async Task<bool> DeletePaymentMethodAsync(int id)
    {
        return await _repository.DeletePaymentMethodAsync(id);
    }

    #endregion

    #region Customer History

    public async Task<CustomerHistoryDtoOut?> GetCustomerHistoryAsync(int customerId)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return null;

        return await _repository.GetCustomerHistoryAsync(customerId);
    }

    #endregion

    #region Loyalty Points

    public async Task<bool> AddLoyaltyPointsAsync(int customerId, CustomerLoyaltyDtoIn loyaltyDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return false;

        var result = await _repository.AddLoyaltyPointsAsync(customerId, loyaltyDto.Points);

        // Check if customer should be upgraded to VIP
        if (result)
        {
            await CheckAndUpdateCategoryAsync(customerId);
        }

        return result;
    }

    public async Task<bool> RedeemLoyaltyPointsAsync(int customerId, CustomerLoyaltyDtoIn loyaltyDto)
    {
        // Verify customer exists
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null) return false;

        // Check if customer has enough points
        if (customer.LoyaltyPoints < loyaltyDto.Points)
        {
            return false;
        }

        return await _repository.DeductLoyaltyPointsAsync(customerId, loyaltyDto.Points);
    }

    public async Task<int> GetLoyaltyPointsAsync(int customerId)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        return customer?.LoyaltyPoints ?? 0;
    }

    #endregion

    #region Blocking

    public async Task<bool> BlockCustomerAsync(int customerId, CustomerBlockDtoIn blockDto)
    {
        return await _repository.BlockCustomerAsync(customerId, blockDto.Reason);
    }

    public async Task<bool> UnblockCustomerAsync(int customerId)
    {
        return await _repository.UnblockCustomerAsync(customerId);
    }

    #endregion

    #region Category

    public async Task<bool> UpdateCategoryAsync(int customerId, string category)
    {
        // Validate category
        if (!CustomerCategory.IsValid(category))
        {
            return false;
        }

        return await _repository.UpdateCategoryAsync(customerId, category);
    }

    #endregion

    #region Private Helpers

    private async Task CheckAndUpdateCategoryAsync(int customerId)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer == null || customer.IsBlocked) return;

        // Get history for VIP calculation
        var history = await _repository.GetCustomerHistoryAsync(customerId);

        // VIP criteria: more than 10 completed appointments OR more than 1000 loyalty points
        if (history.CompletedAppointments >= 10 || customer.LoyaltyPoints >= 1000)
        {
            if (customer.Category != CustomerCategory.VIP)
            {
                await _repository.UpdateCategoryAsync(customerId, CustomerCategory.VIP);
            }
        }
        else if (history.CompletedAppointments >= 1)
        {
            if (customer.Category == CustomerCategory.New)
            {
                await _repository.UpdateCategoryAsync(customerId, CustomerCategory.Regular);
            }
        }
    }

    private static CustomerDtoOut MapToCustomerDtoOut(Customer customer)
    {
        return new CustomerDtoOut
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            ProfileImageUrl = customer.ProfileImageUrl,
            BirthDate = customer.BirthDate?.ToString("yyyy-MM-dd"),
            Category = customer.Category,
            LoyaltyPoints = customer.LoyaltyPoints,
            IsBlocked = customer.IsBlocked,
            BlockedReason = customer.BlockedReason,
            PreferredContactMethod = customer.PreferredContactMethod,
            MarketingConsent = customer.MarketingConsent,
            CreatedAt = customer.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static CustomerNoteDtoOut MapToNoteDtoOut(CustomerNote note)
    {
        return new CustomerNoteDtoOut
        {
            Id = note.Id,
            CustomerId = note.CustomerId,
            EmployeeId = note.EmployeeId,
            EmployeeName = note.EmployeeName,
            Note = note.Note,
            CreatedAt = note.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static CustomerAllergyDtoOut MapToAllergyDtoOut(CustomerAllergy allergy)
    {
        return new CustomerAllergyDtoOut
        {
            Id = allergy.Id,
            CustomerId = allergy.CustomerId,
            AllergyDescription = allergy.AllergyDescription,
            Severity = allergy.Severity,
            CreatedAt = allergy.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static CustomerConsentDtoOut MapToConsentDtoOut(CustomerConsent consent)
    {
        return new CustomerConsentDtoOut
        {
            Id = consent.Id,
            CustomerId = consent.CustomerId,
            ConsentType = consent.ConsentType,
            IsGranted = consent.IsGranted,
            GrantedAt = consent.GrantedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            RevokedAt = consent.RevokedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    private static CustomerPaymentMethodDtoOut MapToPaymentMethodDtoOut(CustomerPaymentMethod method)
    {
        return new CustomerPaymentMethodDtoOut
        {
            Id = method.Id,
            CustomerId = method.CustomerId,
            CardLast4 = method.CardLast4,
            CardBrand = method.CardBrand,
            CardExpiry = method.FormattedExpiry,
            IsDefault = method.IsDefault,
            IsExpired = method.IsExpired,
            CreatedAt = method.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    #endregion
}
