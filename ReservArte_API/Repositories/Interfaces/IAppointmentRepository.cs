using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        // CRUD básico. isActive: true = solo activas, false = solo inactivas, null = todas (por defecto true).
        Task<Appointment?> GetByIdAsync(int id, bool? isActive = true);
        Task<AppointmentDtoOut?> GetByIdDetailedAsync(int id, bool? isActive = true);
        Task<Appointment?> CreateAsync(Appointment appointment);
        Task<Appointment?> UpdateAsync(int id, Appointment appointment);
        Task<bool> DeleteAsync(int id);
        
        // Consultas de agenda. isActive: true = solo activas, false = solo inactivas, null = todas (por defecto true).
        Task<IEnumerable<AppointmentDtoToList>> GetAllAsync(bool? isActive = true);
        Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId, bool? isActive = true);
        Task<IEnumerable<AgendaAppointmentDto>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, bool? isActive = true);
        Task<IEnumerable<AgendaAppointmentDto>> GetByEmployeeAsync(int employeeId, DateOnly startDate, DateOnly endDate, bool? isActive = true);
        Task<IEnumerable<AgendaAppointmentDto>> GetByCustomerAsync(int customerId, CustomerAppointmentsQueryDto? filter = null);
        
        /// <summary>Obtiene citas cuya fecha/hora de inicio esté dentro de la ventana [from, to] para recordatorios.</summary>
        Task<IEnumerable<Appointment>> GetAppointmentsInDateTimeWindowAsync(DateTime from, DateTime to, bool? isActive = true);
        
        // Validaciones y comprobaciones
        Task<bool> CheckOverlapAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null);
        Task<int> GetCustomerNoShowCountAsync(int customerId, bool? isActive = true);
        
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
