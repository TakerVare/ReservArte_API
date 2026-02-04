using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace ReservArte_API.Services;

public class ServicePhotoService : IServicePhotoService
{
    private readonly IServicePhotoRepository _photoRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly S3Settings _s3Settings;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<ServicePhotoService> _logger;

    public ServicePhotoService(
        IServicePhotoRepository photoRepository,
        ICustomerRepository customerRepository,
        IAppointmentRepository appointmentRepository,
        IOptions<S3Settings> s3Settings,
        ILogger<ServicePhotoService> logger)
    {
        _photoRepository = photoRepository;
        _customerRepository = customerRepository;
        _appointmentRepository = appointmentRepository;
        _s3Settings = s3Settings.Value;
        _logger = logger;
        
        // Inicializar cliente S3
        if (!string.IsNullOrEmpty(_s3Settings.AccessKey) && !string.IsNullOrEmpty(_s3Settings.SecretKey))
        {
            _s3Client = new AmazonS3Client(
                _s3Settings.AccessKey,
                _s3Settings.SecretKey,
                RegionEndpoint.GetBySystemName(_s3Settings.Region)
            );
        }
        else
        {
            // Cliente por defecto (usará credenciales de ambiente o IAM role)
            _s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(_s3Settings.Region));
        }
    }

    #region Operaciones Principales

    public async Task<ServicePhotoDtoOut> UploadPhotoAsync(
        int appointmentId, 
        Stream imageStream, 
        string fileName, 
        ServicePhotoDtoIn dto, 
        int employeeId)
    {
        // Validar tipo de foto
        if (!PhotoType.IsValid(dto.Type))
        {
            throw new ArgumentException($"Tipo de foto inválido. Debe ser '{PhotoType.Before}' o '{PhotoType.After}'");
        }
        
        // Obtener información de la cita
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            throw new KeyNotFoundException($"Cita con ID {appointmentId} no encontrada");
        }
        
        // Generar clave S3 única
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension)) extension = ".jpg";
        
        var s3Key = $"{appointment.CustomerId}/{appointmentId}/{dto.Type}_{timestamp}{extension}";
        
        // Procesar imagen (aplicar marca de agua si está configurada)
        using var processedStream = await ProcessImageAsync(imageStream);
        
        // Subir a S3
        var putRequest = new PutObjectRequest
        {
            BucketName = _s3Settings.BucketName,
            Key = s3Key,
            InputStream = processedStream,
            ContentType = GetContentType(extension)
        };
        
        try
        {
            await _s3Client.PutObjectAsync(putRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir foto a S3: {S3Key}", s3Key);
            throw new InvalidOperationException("Error al subir la fotografía. Por favor, inténtelo de nuevo.", ex);
        }
        
        // Crear registro en base de datos
        var photo = new ServicePhoto
        {
            AppointmentId = appointmentId,
            Type = dto.Type,
            S3Key = s3Key,
            S3Bucket = _s3Settings.BucketName,
            UploadedBy = employeeId,
            UploadedAt = DateTime.UtcNow,
            IsPublic = dto.IsPublic,
            ExpiresAt = DateTime.UtcNow.AddYears(_s3Settings.ExpirationYears)
        };
        
        var createdPhoto = await _photoRepository.CreateAsync(photo);
        
        // Generar URL pre-firmada
        var url = GeneratePresignedUrl(s3Key);
        
        return MapToDto(createdPhoto, url);
    }

    public async Task<ServicePhotoDtoOut?> GetByIdAsync(int id, int requesterId, string requesterRole)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null) return null;
        
        // Validar acceso
        if (!await CanAccessPhotoAsync(photo, requesterId, requesterRole))
        {
            throw new UnauthorizedAccessException("No tiene permiso para acceder a esta fotografía");
        }
        
        var url = GeneratePresignedUrl(photo.S3Key);
        return MapToDto(photo, url);
    }

    public async Task<IEnumerable<ServicePhotoDtoOut>> GetByAppointmentIdAsync(
        int appointmentId, 
        int requesterId, 
        string requesterRole)
    {
        // Validar acceso a la cita
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return Enumerable.Empty<ServicePhotoDtoOut>();
        
        if (!CanAccessAppointment(appointment, requesterId, requesterRole))
        {
            throw new UnauthorizedAccessException("No tiene permiso para acceder a las fotos de esta cita");
        }
        
        var photos = await _photoRepository.GetByAppointmentIdAsync(appointmentId);
        return photos.Select(p => MapToDto(p, GeneratePresignedUrl(p.S3Key)));
    }

    #endregion

    #region Comparación y Galería

    public async Task<PhotoComparisonDtoOut?> GetComparisonAsync(
        int appointmentId, 
        int requesterId, 
        string requesterRole)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return null;
        
        if (!CanAccessAppointment(appointment, requesterId, requesterRole))
        {
            throw new UnauthorizedAccessException("No tiene permiso para acceder a esta comparación");
        }
        
        var photos = await _photoRepository.GetByAppointmentIdAsync(appointmentId);
        var photoList = photos.ToList();
        
        if (!photoList.Any()) return null;
        
        return new PhotoComparisonDtoOut
        {
            AppointmentId = appointmentId,
            CustomerName = photoList.FirstOrDefault()?.CustomerName,
            AppointmentDate = appointment.AppointmentDate.ToDateTime(appointment.StartTime),
            BeforePhotos = photoList
                .Where(p => p.Type == PhotoType.Before)
                .Select(p => MapToDto(p, GeneratePresignedUrl(p.S3Key)))
                .ToList(),
            AfterPhotos = photoList
                .Where(p => p.Type == PhotoType.After)
                .Select(p => MapToDto(p, GeneratePresignedUrl(p.S3Key)))
                .ToList()
        };
    }

    public async Task<CustomerGalleryDtoOut?> GetCustomerGalleryAsync(
        int customerId, 
        int requesterId, 
        string requesterRole)
    {
        // Validar acceso
        if (!CanAccessCustomerGallery(customerId, requesterId, requesterRole))
        {
            throw new UnauthorizedAccessException("No tiene permiso para acceder a esta galería");
        }
        
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null) return null;
        
        var photos = await _photoRepository.GetByCustomerIdAsync(customerId);
        var photoList = photos.ToList();
        
        // Agrupar por cita
        var groupedByAppointment = photoList
            .GroupBy(p => p.AppointmentId)
            .Select(g => new PhotoComparisonDtoOut
            {
                AppointmentId = g.Key,
                CustomerName = customer.FirstName + " " + customer.LastName,
                BeforePhotos = g
                    .Where(p => p.Type == PhotoType.Before)
                    .Select(p => MapToDto(p, GeneratePresignedUrl(p.S3Key)))
                    .ToList(),
                AfterPhotos = g
                    .Where(p => p.Type == PhotoType.After)
                    .Select(p => MapToDto(p, GeneratePresignedUrl(p.S3Key)))
                    .ToList()
            })
            .ToList();
        
        return new CustomerGalleryDtoOut
        {
            CustomerId = customerId,
            CustomerName = customer.FirstName + " " + customer.LastName,
            Appointments = groupedByAppointment,
            TotalPhotos = photoList.Count
        };
    }

    #endregion

    #region Compartir en Redes

    public async Task<PhotoShareDtoOut?> GenerateShareUrlAsync(
        int photoId, 
        int requesterId, 
        string requesterRole)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null) return null;
        
        // Validar acceso
        if (!await CanAccessPhotoAsync(photo, requesterId, requesterRole))
        {
            throw new UnauthorizedAccessException("No tiene permiso para compartir esta fotografía");
        }
        
        // Verificar si la foto es pública
        if (!photo.IsPublic)
        {
            return new PhotoShareDtoOut
            {
                PhotoId = photoId,
                ShareUrl = string.Empty,
                HasConsent = false,
                ExpiresAt = DateTime.MinValue
            };
        }
        
        // Verificar consentimiento del cliente
        var customerId = await _photoRepository.GetCustomerIdByAppointmentAsync(photo.AppointmentId);
        var hasPhotoConsent = await _customerRepository.HasConsentAsync(customerId, ConsentType.Photos);
        
        if (!hasPhotoConsent)
        {
            return new PhotoShareDtoOut
            {
                PhotoId = photoId,
                ShareUrl = string.Empty,
                HasConsent = false,
                ExpiresAt = DateTime.MinValue
            };
        }
        
        // Generar URL con mayor duración para compartir (24 horas)
        var shareExpiration = DateTime.UtcNow.AddHours(24);
        var shareUrl = GeneratePresignedUrl(photo.S3Key, 1440); // 24 horas en minutos
        
        return new PhotoShareDtoOut
        {
            PhotoId = photoId,
            ShareUrl = shareUrl,
            HasConsent = true,
            ExpiresAt = shareExpiration
        };
    }

    #endregion

    #region Gestión

    public async Task<bool> UpdateVisibilityAsync(
        int photoId, 
        bool isPublic, 
        int requesterId, 
        string requesterRole)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId);
        if (photo == null) return false;
        
        // Solo empleados pueden cambiar visibilidad
        if (requesterRole != Roles.Employee && requesterRole != Roles.Admin)
        {
            throw new UnauthorizedAccessException("Solo el personal puede modificar la visibilidad de las fotos");
        }
        
        photo.IsPublic = isPublic;
        return await _photoRepository.UpdateAsync(photo);
    }

    public async Task<bool> DeleteAsync(int id, int requesterId, string requesterRole)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null) return false;
        
        // Solo empleados pueden eliminar fotos
        if (requesterRole != Roles.Employee && requesterRole != Roles.Admin)
        {
            throw new UnauthorizedAccessException("Solo el personal puede eliminar fotografías");
        }
        
        // Eliminar de S3
        try
        {
            await _s3Client.DeleteObjectAsync(_s3Settings.BucketName, photo.S3Key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar la foto de S3: {S3Key}", photo.S3Key);
        }
        
        // Eliminar de la base de datos
        return await _photoRepository.DeleteAsync(id);
    }

    #endregion

    #region RGPD - Limpieza Automática

    public async Task<int> CleanupExpiredPhotosAsync()
    {
        var expiredPhotos = await _photoRepository.GetExpiredPhotosAsync();
        var deletedCount = 0;
        
        foreach (var photo in expiredPhotos)
        {
            try
            {
                // Eliminar de S3
                await _s3Client.DeleteObjectAsync(_s3Settings.BucketName, photo.S3Key);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al eliminar foto expirada de S3: {S3Key}", photo.S3Key);
            }
        }
        
        // Eliminar registros de la base de datos
        await _photoRepository.DeleteExpiredAsync();
        
        _logger.LogInformation("Limpieza RGPD: {Count} fotos expiradas eliminadas", deletedCount);
        
        return deletedCount;
    }

    #endregion

    #region Helpers Privados

    private async Task<MemoryStream> ProcessImageAsync(Stream inputStream)
    {
        var outputStream = new MemoryStream();
        
        // Si no hay logo configurado, solo copiar el stream
        if (string.IsNullOrEmpty(_s3Settings.WatermarkLogoPath) || !File.Exists(_s3Settings.WatermarkLogoPath))
        {
            await inputStream.CopyToAsync(outputStream);
            outputStream.Position = 0;
            return outputStream;
        }
        
        try
        {
            // Cargar imagen original
            using var image = await Image.LoadAsync<Rgba32>(inputStream);
            
            // Cargar logo
            using var logo = await Image.LoadAsync<Rgba32>(_s3Settings.WatermarkLogoPath);
            
            // Redimensionar logo (máximo 20% del ancho de la imagen)
            var maxLogoWidth = image.Width / 5;
            if (logo.Width > maxLogoWidth)
            {
                var ratio = (float)maxLogoWidth / logo.Width;
                logo.Mutate(x => x.Resize((int)(logo.Width * ratio), (int)(logo.Height * ratio)));
            }
            
            // Aplicar opacidad al logo
            logo.Mutate(x => x.Opacity(_s3Settings.WatermarkOpacity));
            
            // Posicionar en esquina inferior derecha con margen
            var margin = 20;
            var point = new Point(
                image.Width - logo.Width - margin,
                image.Height - logo.Height - margin
            );
            
            // Aplicar marca de agua
            image.Mutate(x => x.DrawImage(logo, point, 1f));
            
            // Guardar resultado
            await image.SaveAsJpegAsync(outputStream);
            outputStream.Position = 0;
            
            return outputStream;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al aplicar marca de agua, guardando imagen sin procesar");
            inputStream.Position = 0;
            await inputStream.CopyToAsync(outputStream);
            outputStream.Position = 0;
            return outputStream;
        }
    }

    private string GeneratePresignedUrl(string s3Key, int? expirationMinutes = null)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3Settings.BucketName,
            Key = s3Key,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes ?? _s3Settings.PresignedUrlExpirationMinutes)
        };
        
        return _s3Client.GetPreSignedURL(request);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private async Task<bool> CanAccessPhotoAsync(ServicePhoto photo, int requesterId, string requesterRole)
    {
        // Empleados y admins siempre pueden ver
        if (requesterRole == Roles.Employee || requesterRole == Roles.Admin)
            return true;
        
        // Clientes solo pueden ver sus propias fotos
        if (requesterRole == Roles.Client)
        {
            var customerId = await _photoRepository.GetCustomerIdByAppointmentAsync(photo.AppointmentId);
            return customerId == requesterId;
        }
        
        return false;
    }

    private static bool CanAccessAppointment(Appointment appointment, int requesterId, string requesterRole)
    {
        // Empleados y admins siempre pueden ver
        if (requesterRole == Roles.Employee || requesterRole == Roles.Admin)
            return true;
        
        // El cliente de la cita puede ver sus fotos
        if (requesterRole == Roles.Client)
            return appointment.CustomerId == requesterId;
        
        return false;
    }

    private static bool CanAccessCustomerGallery(int customerId, int requesterId, string requesterRole)
    {
        // Empleados y admins siempre pueden ver
        if (requesterRole == Roles.Employee || requesterRole == Roles.Admin)
            return true;
        
        // El cliente solo puede ver su propia galería
        if (requesterRole == Roles.Client)
            return customerId == requesterId;
        
        return false;
    }

    private static ServicePhotoDtoOut MapToDto(ServicePhoto photo, string url)
    {
        return new ServicePhotoDtoOut
        {
            Id = photo.Id,
            AppointmentId = photo.AppointmentId,
            Type = photo.Type,
            Url = url,
            UploadedByName = photo.EmployeeName,
            UploadedAt = photo.UploadedAt,
            IsPublic = photo.IsPublic,
            ExpiresAt = photo.ExpiresAt
        };
    }

    #endregion
}
