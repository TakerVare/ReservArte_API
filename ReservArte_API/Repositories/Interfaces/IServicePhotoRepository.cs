using ReservArte_API.Models;

namespace ReservArte_API.Repositories.Interfaces;

public interface IServicePhotoRepository
{
    // CRUD básico
    Task<ServicePhoto?> GetByIdAsync(int id);
    Task<ServicePhoto> CreateAsync(ServicePhoto photo);
    Task<bool> UpdateAsync(ServicePhoto photo);
    Task<bool> DeleteAsync(int id);
    
    // Consultas por relación
    Task<IEnumerable<ServicePhoto>> GetByAppointmentIdAsync(int appointmentId);
    Task<IEnumerable<ServicePhoto>> GetByCustomerIdAsync(int customerId);
    
    // RGPD - Expiración automática
    Task<IEnumerable<ServicePhoto>> GetExpiredPhotosAsync();
    Task<int> DeleteExpiredAsync();
    
    // Auxiliares
    Task<int> GetCustomerIdByAppointmentAsync(int appointmentId);
}
