using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IImageService _imageService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, IImageService imageService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _imageService = imageService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("nameid")?.Value
                 ?? User.FindFirst("id")?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }

    #region Customer CRUD

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<CustomerListDtoOut>>> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID.
    /// Admin/Employee: cualquier cliente. Client: solo el suyo propio.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDtoOut>> GetById(int id)
    {
        // Si es cliente, solo puede ver su propio perfil
        if (User.IsInRole(Roles.Client))
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue || currentUserId.Value != id)
                return Forbid();
        }

        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerDtoOut>> Create([FromBody] CustomerDtoIn customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdCustomer = await _customerService.CreateAsync(customerDto);
        if (createdCustomer == null)
            return BadRequest(new { message = "Error al crear el cliente" });

        return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
    }

    /// <summary>
    /// Update an existing customer.
    /// Admin/Employee: cualquier cliente. Client: solo el suyo propio.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDtoOut>> Update(int id, [FromBody] CustomerDtoIn customerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Si es cliente, solo puede editar su propio perfil
        if (User.IsInRole(Roles.Client))
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue || currentUserId.Value != id)
                return Forbid();
        }

        var updatedCustomer = await _customerService.UpdateAsync(id, customerDto);
        if (updatedCustomer == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(updatedCustomer);
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Cliente no encontrado" });

        return NoContent();
    }

    #endregion

    #region Customer Notes

    /// <summary>
    /// Get notes for a customer
    /// </summary>
    [HttpGet("{id}/notes")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<CustomerNoteDtoOut>>> GetNotes(int id)
    {
        var notes = await _customerService.GetNotesByCustomerIdAsync(id);
        return Ok(notes);
    }

    /// <summary>
    /// Add a note to a customer
    /// </summary>
    [HttpPost("{id}/notes")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerNoteDtoOut>> CreateNote(int id, [FromBody] CustomerNoteDtoIn noteDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var employeeIdClaim = User.FindFirst("id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(employeeIdClaim, out int employeeId))
            return Unauthorized(new { message = "No se pudo identificar al empleado" });

        var createdNote = await _customerService.CreateNoteAsync(id, employeeId, noteDto);
        if (createdNote == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return CreatedAtAction(nameof(GetNotes), new { id }, createdNote);
    }

    /// <summary>
    /// Delete a customer note
    /// </summary>
    [HttpDelete("notes/{noteId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> DeleteNote(int noteId)
    {
        var deleted = await _customerService.DeleteNoteAsync(noteId);
        if (!deleted)
            return NotFound(new { message = "Nota no encontrada" });

        return NoContent();
    }

    #endregion

    #region Customer Allergies

    [HttpGet("{id}/allergies")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<CustomerAllergyDtoOut>>> GetAllergies(int id)
    {
        var allergies = await _customerService.GetAllergiesByCustomerIdAsync(id);
        return Ok(allergies);
    }

    [HttpPost("{id}/allergies")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerAllergyDtoOut>> CreateAllergy(int id, [FromBody] CustomerAllergyDtoIn allergyDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdAllergy = await _customerService.CreateAllergyAsync(id, allergyDto);
        if (createdAllergy == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return CreatedAtAction(nameof(GetAllergies), new { id }, createdAllergy);
    }

    [HttpPut("allergies/{allergyId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerAllergyDtoOut>> UpdateAllergy(int allergyId, [FromBody] CustomerAllergyDtoIn allergyDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedAllergy = await _customerService.UpdateAllergyAsync(allergyId, allergyDto);
        if (updatedAllergy == null)
            return NotFound(new { message = "Alergia no encontrada" });

        return Ok(updatedAllergy);
    }

    [HttpDelete("allergies/{allergyId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> DeleteAllergy(int allergyId)
    {
        var deleted = await _customerService.DeleteAllergyAsync(allergyId);
        if (!deleted)
            return NotFound(new { message = "Alergia no encontrada" });

        return NoContent();
    }

    #endregion

    #region Customer Consents

    [HttpGet("{id}/consents")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<IEnumerable<CustomerConsentDtoOut>>> GetConsents(int id)
    {
        var consents = await _customerService.GetConsentsByCustomerIdAsync(id);
        return Ok(consents);
    }

    [HttpPut("{id}/consents")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerConsentDtoOut>> UpdateConsent(int id, [FromBody] CustomerConsentDtoIn consentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedConsent = await _customerService.UpdateConsentAsync(id, consentDto);
        if (updatedConsent == null)
            return NotFound(new { message = "Cliente no encontrado o tipo de consentimiento inválido" });

        return Ok(updatedConsent);
    }

    #endregion

    #region Customer Payment Methods

    [HttpGet("{id}/payment-methods")]
    [Authorize(Policy = "AdminOrEmployeeOrClientOwnCustomer")]
    public async Task<ActionResult<IEnumerable<CustomerPaymentMethodDtoOut>>> GetPaymentMethods(int id)
    {
        var methods = await _customerService.GetPaymentMethodsByCustomerIdAsync(id);
        return Ok(methods);
    }

    [HttpPost("{id}/payment-methods")]
    [Authorize(Policy = "AdminOrEmployeeOrClientOwnCustomer")]
    public async Task<ActionResult<CustomerPaymentMethodDtoOut>> CreatePaymentMethod(int id, [FromBody] CustomerPaymentMethodDtoIn methodDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdMethod = await _customerService.CreatePaymentMethodAsync(id, methodDto);
        if (createdMethod == null)
            return BadRequest(new { message = "Cliente no encontrado o no ha dado consentimiento para guardar tarjetas" });

        return CreatedAtAction(nameof(GetPaymentMethods), new { id }, createdMethod);
    }

    [HttpPut("{id}/payment-methods/{paymentMethodId}/default")]
    [Authorize(Policy = "AdminOrEmployeeOrClientOwnCustomer")]
    public async Task<IActionResult> SetDefaultPaymentMethod(int id, int paymentMethodId)
    {
        var success = await _customerService.SetDefaultPaymentMethodAsync(id, paymentMethodId);
        if (!success)
            return NotFound(new { message = "Método de pago no encontrado" });

        return Ok(new { message = "Método de pago establecido como predeterminado" });
    }

    [HttpDelete("payment-methods/{paymentMethodId}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> DeletePaymentMethod(int paymentMethodId)
    {
        var deleted = await _customerService.DeletePaymentMethodAsync(paymentMethodId);
        if (!deleted)
            return NotFound(new { message = "Método de pago no encontrado" });

        return NoContent();
    }

    #endregion

    #region Customer History

    [HttpGet("{id}/history")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerHistoryDtoOut>> GetHistory(int id)
    {
        var history = await _customerService.GetCustomerHistoryAsync(id);
        if (history == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(history);
    }

    #endregion

    #region Loyalty Points

    [HttpGet("{id}/loyalty")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<object>> GetLoyaltyPoints(int id)
    {
        var points = await _customerService.GetLoyaltyPointsAsync(id);
        return Ok(new { customerId = id, loyaltyPoints = points });
    }

    [HttpPost("{id}/loyalty/add")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> AddLoyaltyPoints(int id, [FromBody] CustomerLoyaltyDtoIn loyaltyDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _customerService.AddLoyaltyPointsAsync(id, loyaltyDto);
        if (!success)
            return NotFound(new { message = "Cliente no encontrado" });

        var newPoints = await _customerService.GetLoyaltyPointsAsync(id);
        return Ok(new { message = "Puntos añadidos correctamente", loyaltyPoints = newPoints });
    }

    [HttpPost("{id}/loyalty/redeem")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> RedeemLoyaltyPoints(int id, [FromBody] CustomerLoyaltyDtoIn loyaltyDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _customerService.RedeemLoyaltyPointsAsync(id, loyaltyDto);
        if (!success)
            return BadRequest(new { message = "Cliente no encontrado o puntos insuficientes" });

        var newPoints = await _customerService.GetLoyaltyPointsAsync(id);
        return Ok(new { message = "Puntos canjeados correctamente", loyaltyPoints = newPoints });
    }

    #endregion

    #region Blocking

    [HttpPost("{id}/block")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> BlockCustomer(int id, [FromBody] CustomerBlockDtoIn blockDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _customerService.BlockCustomerAsync(id, blockDto);
        if (!success)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(new { message = "Cliente bloqueado correctamente" });
    }

    [HttpPost("{id}/unblock")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UnblockCustomer(int id)
    {
        var success = await _customerService.UnblockCustomerAsync(id);
        if (!success)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(new { message = "Cliente desbloqueado correctamente" });
    }

    #endregion

    #region Category

    [HttpPut("{id}/category")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] string category)
    {
        var success = await _customerService.UpdateCategoryAsync(id, category);
        if (!success)
            return BadRequest(new { message = "Cliente no encontrado o categoría inválida" });

        return Ok(new { message = "Categoría actualizada correctamente" });
    }

    #endregion

    #region Profile Image

    [HttpPost("{id}/profile-image")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadProfileImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "El archivo está vacío" });

        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Cliente no encontrado" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(new { message = "Tipo de archivo no permitido. Use JPEG, PNG, GIF o WebP" });

        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return BadRequest(new { message = "El archivo es demasiado grande. Máximo 5MB" });

        try
        {
            var imageUrl = await _imageService.UploadImageAsync(file);
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest(new { message = "Error al cargar la imagen" });

            var updatedCustomer = await _customerService.UpdateProfileImageAsync(id, imageUrl);
            if (updatedCustomer == null)
                return BadRequest(new { message = "Error al actualizar el perfil del cliente" });

            return Ok(new { message = "Foto de perfil cargada exitosamente", customer = updatedCustomer });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al cargar foto de perfil para cliente {id}: {ex.Message}");
            return StatusCode(500, new { message = "Error interno al procesar la imagen", error = ex.Message });
        }
    }

    #endregion
}