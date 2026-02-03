using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        // CRUD básico
        Task<Appointment?> GetByIdAsync(int id);
        Task<AppointmentDtoOut?> GetByIdDetailedAsync(int id);
        Task<Appointment?> CreateAsync(Appointment appointment);
        Task<Appointment?> UpdateAsync(int id, Appointment appointment);
        Task<bool> DeleteAsync(int id);
        
        // Consultas de agenda
        Task<IEnumerable<AppointmentDtoToList>> GetAllAsync();
        Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AgendaAppointmentDto>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<AgendaAppointmentDto>> GetByEmployeeAsync(int employeeId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<AgendaAppointmentDto>> GetByCustomerAsync(int customerId);
        
        // Validaciones y comprobaciones
        Task<bool> CheckOverlapAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null);
        Task<int> GetCustomerNoShowCountAsync(int customerId);
        
        // Gestión de estados
        Task<bool> UpdateStatusAsync(int id, string status, string? notes = null);
        Task<bool> CancelAsync(int id, string reason, int cancelledById, string cancelledByType);
        
        // Servicios de cita
        Task<bool> AddAppointmentServicesAsync(int appointmentId, List<AppointmentServiceItem> services);
        Task<IEnumerable<AppointmentServiceItem>> GetAppointmentServicesAsync(int appointmentId);
        
        // Usuarios relacionados (mantener compatibilidad)
        Task<IEnumerable<User>> GetCustomerAsync(int customerId);
        Task<IEnumerable<User>> GetEmployeeAsync(int employeeId);
    }
}
