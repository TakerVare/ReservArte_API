using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Sube una imagen a Cloudinary.
    /// </summary>
    /// <param name="file">Archivo de imagen (JPEG, PNG, GIF, WebP)</param>
    /// <returns>URL segura de la imagen subida</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "El archivo está vacío" });

        // Validar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Tipo de archivo no permitido. Use JPEG, PNG, GIF o WebP" });
        }

        // Validar tamaño máximo (10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "El archivo es demasiado grande. Máximo 10MB" });
        }

        _logger.LogInformation($"Cargando imagen: {file.FileName} - Tamaño: {file.Length} bytes");

        try
        {
            var imageUrl = await _imageService.UploadImageAsync(file);

            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogError($"Error al cargar la imagen: {file.FileName}");
                return BadRequest(new { message = "Error al cargar la imagen" });
            }

            _logger.LogInformation($"Imagen cargada exitosamente: {imageUrl}");
            return Ok(new { url = imageUrl, message = "Imagen cargada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al cargar la imagen: {ex.Message}");
            return StatusCode(500, new { message = "Error interno al procesar la imagen", error = ex.Message });
        }
    }

    /// <summary>
    /// Sube una imagen sin autenticación (para procesos públicos).
    /// </summary>
    [HttpPost("upload/public")]
    [Consumes("multipart/form-data")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadImagePublic(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "El archivo está vacío" });

        // Validar tipo de archivo
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Tipo de archivo no permitido. Use JPEG, PNG, GIF o WebP" });
        }

        // Validar tamaño máximo (10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return BadRequest(new { message = "El archivo es demasiado grande. Máximo 10MB" });
        }

        _logger.LogInformation($"Cargando imagen (anónimo): {file.FileName}");

        try
        {
            var imageUrl = await _imageService.UploadImageAsync(file);

            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogError($"Error al cargar la imagen: {file.FileName}");
                return BadRequest(new { message = "Error al cargar la imagen" });
            }

            return Ok(new { url = imageUrl, message = "Imagen cargada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Excepción al cargar la imagen: {ex.Message}");
            return StatusCode(500, new { message = "Error interno al procesar la imagen", error = ex.Message });
        }
    }
}
