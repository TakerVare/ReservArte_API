using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class ServicePhotoController : ControllerBase
{
    private readonly IServicePhotoService _photoService;

    public ServicePhotoController(IServicePhotoService photoService)
    {
        _photoService = photoService;
    }

    #region Photo Upload

    /// <summary>
    /// Sube una fotografía para una cita (antes o después del servicio).
    /// Solo empleados pueden subir fotos.
    /// </summary>
    [HttpPost("appointments/{appointmentId}/photos")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<ActionResult<ServicePhotoDtoOut>> UploadPhoto(
        int appointmentId,
        [FromForm] IFormFile file,
        [FromForm] string type,
        [FromForm] bool isPublic = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No se ha proporcionado ningún archivo" });
        }
        
        // Validar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Tipo de archivo no permitido. Use JPEG, PNG, GIF o WebP" });
        }
        
        // Validar tamaño (máximo 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "El archivo es demasiado grande. Máximo 10MB" });
        }
        
        // Obtener ID del empleado
        var employeeIdClaim = User.FindFirst("id")?.Value;
        if (!int.TryParse(employeeIdClaim, out int employeeId))
        {
            return Unauthorized(new { message = "No se pudo identificar al empleado" });
        }
        
        var dto = new ServicePhotoDtoIn
        {
            Type = type,
            IsPublic = isPublic
        };
        
        try
        {
            using var stream = file.OpenReadStream();
            var photo = await _photoService.UploadPhotoAsync(
                appointmentId, 
                stream, 
                file.FileName, 
                dto, 
                employeeId);
            
            return CreatedAtAction(nameof(GetPhoto), new { id = photo.Id }, photo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    #endregion

    #region Photo Retrieval

    /// <summary>
    /// Obtiene una foto por su ID.
    /// </summary>
    [HttpGet("photos/{id}")]
    public async Task<ActionResult<ServicePhotoDtoOut>> GetPhoto(int id)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var photo = await _photoService.GetByIdAsync(id, requesterId, requesterRole);
            if (photo == null)
            {
                return NotFound(new { message = "Fotografía no encontrada" });
            }
            return Ok(photo);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene todas las fotos de una cita.
    /// </summary>
    [HttpGet("appointments/{appointmentId}/photos")]
    public async Task<ActionResult<IEnumerable<ServicePhotoDtoOut>>> GetPhotosByAppointment(int appointmentId)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var photos = await _photoService.GetByAppointmentIdAsync(appointmentId, requesterId, requesterRole);
            return Ok(photos);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene la comparación antes/después de una cita.
    /// </summary>
    [HttpGet("appointments/{appointmentId}/photos/comparison")]
    public async Task<ActionResult<PhotoComparisonDtoOut>> GetComparison(int appointmentId)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var comparison = await _photoService.GetComparisonAsync(appointmentId, requesterId, requesterRole);
            if (comparison == null)
            {
                return NotFound(new { message = "No hay fotos para esta cita" });
            }
            return Ok(comparison);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    #endregion

    #region Customer Gallery

    /// <summary>
    /// Obtiene la galería privada de un cliente.
    /// Solo el propio cliente o el personal pueden acceder.
    /// </summary>
    [HttpGet("customers/{customerId}/gallery")]
    public async Task<ActionResult<CustomerGalleryDtoOut>> GetCustomerGallery(int customerId)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var gallery = await _photoService.GetCustomerGalleryAsync(customerId, requesterId, requesterRole);
            if (gallery == null)
            {
                return NotFound(new { message = "Cliente no encontrado" });
            }
            return Ok(gallery);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    #endregion

    #region Sharing

    /// <summary>
    /// Genera una URL para compartir una foto en redes sociales.
    /// Requiere que la foto sea pública y que el cliente haya dado consentimiento.
    /// </summary>
    [HttpPost("photos/{id}/share")]
    public async Task<ActionResult<PhotoShareDtoOut>> GenerateShareUrl(int id)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var shareResult = await _photoService.GenerateShareUrlAsync(id, requesterId, requesterRole);
            if (shareResult == null)
            {
                return NotFound(new { message = "Fotografía no encontrada" });
            }
            
            if (!shareResult.HasConsent)
            {
                return BadRequest(new 
                { 
                    message = "No se puede compartir esta foto. Requiere que la foto sea pública y consentimiento del cliente.",
                    hasConsent = false
                });
            }
            
            return Ok(shareResult);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    #endregion

    #region Photo Management

    /// <summary>
    /// Actualiza la visibilidad de una foto.
    /// Solo empleados pueden cambiar la visibilidad.
    /// </summary>
    [HttpPatch("photos/{id}/visibility")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> UpdateVisibility(int id, [FromBody] ServicePhotoUpdateDto dto)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var success = await _photoService.UpdateVisibilityAsync(id, dto.IsPublic, requesterId, requesterRole);
            if (!success)
            {
                return NotFound(new { message = "Fotografía no encontrada" });
            }
            return Ok(new { message = "Visibilidad actualizada correctamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Elimina una foto.
    /// Solo empleados pueden eliminar fotos.
    /// </summary>
    [HttpDelete("photos/{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var (requesterId, requesterRole) = GetRequesterInfo();
        
        try
        {
            var success = await _photoService.DeleteAsync(id, requesterId, requesterRole);
            if (!success)
            {
                return NotFound(new { message = "Fotografía no encontrada" });
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    #endregion

    #region Helpers

    private (int requesterId, string requesterRole) GetRequesterInfo()
    {
        var idClaim = User.FindFirst("id")?.Value;
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                     ?? User.FindFirst("role")?.Value;
        
        int.TryParse(idClaim, out int requesterId);
        
        return (requesterId, roleClaim ?? string.Empty);
    }

    #endregion
}
