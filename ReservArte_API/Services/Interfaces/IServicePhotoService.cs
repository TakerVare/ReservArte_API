using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IServicePhotoService
{
    // Operaciones principales
    Task<ServicePhotoDtoOut> UploadPhotoAsync(int appointmentId, Stream imageStream, string fileName, ServicePhotoDtoIn dto, int employeeId);
    Task<ServicePhotoDtoOut?> GetByIdAsync(int id, int requesterId, string requesterRole);
    Task<IEnumerable<ServicePhotoDtoOut>> GetByAppointmentIdAsync(int appointmentId, int requesterId, string requesterRole);
    
    // Comparación y galería
    Task<PhotoComparisonDtoOut?> GetComparisonAsync(int appointmentId, int requesterId, string requesterRole);
    Task<CustomerGalleryDtoOut?> GetCustomerGalleryAsync(int customerId, int requesterId, string requesterRole);
    
    // Compartir en redes
    Task<PhotoShareDtoOut?> GenerateShareUrlAsync(int photoId, int requesterId, string requesterRole);
    
    // Gestión
    Task<bool> UpdateVisibilityAsync(int photoId, bool isPublic, int requesterId, string requesterRole);
    Task<bool> DeleteAsync(int id, int requesterId, string requesterRole);
    
    // RGPD - Limpieza automática
    Task<int> CleanupExpiredPhotosAsync();
}
