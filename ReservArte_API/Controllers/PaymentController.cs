using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;
using System.Security.Claims;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    #region Consultas

    /// <summary>
    /// Obtiene todos los pagos (paginado)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<PaymentPagedResultDto>> GetAll([FromQuery] PaymentFilterDto filter)
    {
        try
        {
            var result = await _paymentService.GetFilteredAsync(filter);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un pago por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDtoOut>> GetById(int id)
    {
        var payment = await _paymentService.GetByIdDetailedAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = "Pago no encontrado" });
        }
        
        // Verificar acceso: admin/employee pueden ver todos, cliente solo los suyos
        if (!User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Employee))
        {
            var userId = GetCurrentUserId();
            if (payment.CustomerId != userId)
            {
                return Forbid();
            }
        }
        
        return Ok(payment);
    }

    /// <summary>
    /// Obtiene los pagos de un cliente
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<PaymentListDto>>> GetByCustomer(int customerId)
    {
        // Verificar acceso
        if (!User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Employee))
        {
            var userId = GetCurrentUserId();
            if (customerId != userId)
            {
                return Forbid();
            }
        }
        
        var payments = await _paymentService.GetByCustomerIdAsync(customerId);
        return Ok(payments);
    }

    /// <summary>
    /// Obtiene los pagos de una cita
    /// </summary>
    [HttpGet("appointment/{appointmentId}")]
    public async Task<ActionResult<IEnumerable<PaymentListDto>>> GetByAppointment(int appointmentId)
    {
        var payments = await _paymentService.GetByAppointmentIdAsync(appointmentId);
        return Ok(payments);
    }

    /// <summary>
    /// Obtiene estadísticas de pagos de un cliente
    /// </summary>
    [HttpGet("customer/{customerId}/stats")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<CustomerPaymentStatsDto>> GetCustomerStats(int customerId)
    {
        var stats = await _paymentService.GetCustomerStatsAsync(customerId);
        return Ok(stats);
    }

    #endregion

    #region Pagos Manuales

    /// <summary>
    /// Registra un pago manual (efectivo, transferencia, TPV)
    /// </summary>
    [HttpPost("manual")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<PaymentDtoOut>> CreateManualPayment([FromBody] PaymentDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var registeredById = GetCurrentUserId();
            var payment = await _paymentService.CreateManualPaymentAsync(dto, registeredById);
            
            return CreatedAtAction(nameof(GetById), new { id = payment?.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registra un pago manual para una cita específica
    /// </summary>
    [HttpPost("appointment/{appointmentId}/manual")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<PaymentDtoOut>> CreateManualPaymentForAppointment(
        int appointmentId, 
        [FromBody] PaymentDtoIn dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var registeredById = GetCurrentUserId();
            var payment = await _paymentService.CreateManualPaymentForAppointmentAsync(
                appointmentId, dto, registeredById);
            
            return CreatedAtAction(nameof(GetById), new { id = payment?.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Operaciones de Estado

    /// <summary>
    /// Actualiza el estado de un pago
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] PaymentStatusUpdateDto dto)
    {
        try
        {
            var success = await _paymentService.UpdateStatusAsync(id, dto.Status);
            if (!success)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }
            
            var payment = await _paymentService.GetByIdDetailedAsync(id);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Procesa un reembolso manual (sin Redsys)
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<PaymentDtoOut>> ProcessRefund(int id, [FromBody] RefundDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var payment = await _paymentService.ProcessRefundAsync(id, dto.Amount, dto.Notes);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un pago (solo pendientes o fallidos)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _paymentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Redsys - Pre-autorización

    /// <summary>
    /// Crea una pre-autorización para una cita (bloquea importe sin cobrar)
    /// El frontend debe haber obtenido un idOper usando el SDK InSite de Redsys
    /// </summary>
    [HttpPost("preauth")]
    public async Task<ActionResult<RedsysPreAuthResponseDto>> CreatePreAuthorization(
        [FromBody] RedsysPreAuthRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _paymentService.CreatePreAuthorizationAsync(dto);
            
            if (!result.Success)
            {
                return BadRequest(new { 
                    message = result.ResponseMessage ?? "Error en pre-autorización",
                    responseCode = result.ResponseCode,
                    paymentId = result.PaymentId
                });
            }
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Redsys - Confirmación/Captura

    /// <summary>
    /// Confirma (captura) una pre-autorización
    /// Se usa cuando el cliente asiste a la cita
    /// </summary>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<RedsysConfirmResponseDto>> ConfirmPayment(
        int id, 
        [FromBody] RedsysConfirmRequestDto? dto = null)
    {
        try
        {
            var result = await _paymentService.ConfirmPaymentAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(new { 
                    message = result.ResponseMessage ?? "Error en confirmación",
                    responseCode = result.ResponseCode
                });
            }
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Redsys - Cancelación

    /// <summary>
    /// Cancela una pre-autorización (libera el importe bloqueado)
    /// Se usa cuando el cliente cancela con antelación suficiente o paga en efectivo
    /// </summary>
    [HttpPost("{id}/cancel-preauth")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<RedsysCancelResponseDto>> CancelPreAuthorization(
        int id, 
        [FromBody] RedsysCancelRequestDto? dto = null)
    {
        try
        {
            var result = await _paymentService.CancelPreAuthorizationAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(new { 
                    message = result.ResponseMessage ?? "Error en cancelación",
                    responseCode = result.ResponseCode
                });
            }
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Redsys - Reembolso

    /// <summary>
    /// Procesa un reembolso vía Redsys para pagos capturados con tarjeta
    /// </summary>
    [HttpPost("{id}/redsys-refund")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<RedsysRefundResponseDto>> ProcessRedsysRefund(
        int id, 
        [FromBody] RedsysRefundRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _paymentService.ProcessRedsysRefundAsync(id, dto);
            
            if (!result.Success)
            {
                return BadRequest(new { 
                    message = result.ResponseMessage ?? "Error en reembolso",
                    responseCode = result.ResponseCode
                });
            }
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Redsys - Webhook

    /// <summary>
    /// Endpoint para recibir notificaciones de Redsys (webhook)
    /// Este endpoint no requiere autenticación
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromForm] RedsysWebhookDto webhook)
    {
        try
        {
            var success = await _paymentService.ProcessWebhookAsync(webhook);
            
            if (!success)
            {
                // Redsys espera HTTP 200 incluso si hay error, pero logueamos el problema
                return Ok(new { message = "Webhook recibido pero no procesado" });
            }
            
            return Ok(new { message = "OK" });
        }
        catch (Exception)
        {
            // Siempre devolver 200 a Redsys para evitar reintentos
            return Ok(new { message = "Error interno" });
        }
    }

    #endregion

    #region Tarjetas Guardadas

    /// <summary>
    /// Obtiene las tarjetas guardadas de un cliente
    /// </summary>
    [HttpGet("methods/{customerId}")]
    public async Task<ActionResult<IEnumerable<CustomerPaymentMethodDtoOut>>> GetPaymentMethods(int customerId)
    {
        // Verificar acceso
        if (!User.IsInRole(Roles.Admin) && !User.IsInRole(Roles.Employee))
        {
            var userId = GetCurrentUserId();
            if (customerId != userId)
            {
                return Forbid();
            }
        }
        
        var methods = await _paymentService.GetCustomerPaymentMethodsAsync(customerId);
        return Ok(methods);
    }

    /// <summary>
    /// Elimina una tarjeta guardada
    /// </summary>
    [HttpDelete("methods/{customerId}/{paymentMethodId}")]
    public async Task<IActionResult> DeletePaymentMethod(int customerId, int paymentMethodId)
    {
        try
        {
            // Verificar acceso
            if (!User.IsInRole(Roles.Admin))
            {
                var userId = GetCurrentUserId();
                if (customerId != userId)
                {
                    return Forbid();
                }
            }
            
            var deleted = await _paymentService.DeletePaymentMethodAsync(paymentMethodId, customerId);
            if (!deleted)
            {
                return NotFound(new { message = "Método de pago no encontrado" });
            }
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Establece una tarjeta como predeterminada
    /// </summary>
    [HttpPut("methods/{customerId}/{paymentMethodId}/default")]
    public async Task<IActionResult> SetDefaultPaymentMethod(int customerId, int paymentMethodId)
    {
        try
        {
            // Verificar acceso
            if (!User.IsInRole(Roles.Admin))
            {
                var userId = GetCurrentUserId();
                if (customerId != userId)
                {
                    return Forbid();
                }
            }
            
            var success = await _paymentService.SetDefaultPaymentMethodAsync(paymentMethodId, customerId);
            if (!success)
            {
                return NotFound(new { message = "Método de pago no encontrado" });
            }
            
            return Ok(new { message = "Método de pago establecido como predeterminado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Helpers

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("id")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        
        return 0;
    }

    #endregion
}

#region DTOs auxiliares del controlador

/// <summary>
/// DTO para actualizar estado de pago
/// </summary>
public class PaymentStatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO para procesar reembolso
/// </summary>
public class RefundDto
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

#endregion
