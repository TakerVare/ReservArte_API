using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces
{
    public interface IAppointmentService
    {
        // CRUD básico
        Task<IEnumerable<AppointmentDtoToList>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<AppointmentDtoOut?> GetByIdDetailedAsync(int id);
        Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId);
        Task<bool> DeleteAsync(int id);
        
        // Creación de citas con validaciones
        Task<AppointmentDtoOut?> CreateAsync(AppointmentDtoIn appointmentDto);
        Task<Appointment?> UpdateAsync(int id, Appointment appointment);
        
        // Consultas de agenda
        Task<AgendaResponseDto> GetAgendaAsync(AgendaQueryDto query);
        Task<AvailableSlotsResponseDto> GetAvailableSlotsAsync(AvailableSlotsRequestDto request);
        Task<IEnumerable<AgendaAppointmentDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<AgendaAppointmentDto>> GetByEmployeeIdAsync(int employeeId, DateOnly startDate, DateOnly endDate);
        
        // Gestión de estados
        Task<AppointmentDtoOut?> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto);
        Task<AppointmentCancelResultDto> CancelAsync(int id, AppointmentCancelDto dto);
        Task<AppointmentDtoOut?> RescheduleAsync(int id, AppointmentRescheduleDto dto);
        Task<AppointmentCancelResultDto> MarkAsNoShowAsync(int id);
        
        // Validaciones
        Task<bool> CheckAvailabilityAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null);
        Task<decimal> CalculatePenaltyAsync(int appointmentId);
        
        // Mantener compatibilidad
        Task<IEnumerable<User>> GetCustomerAsync(int customerId);
        Task<IEnumerable<User>> GetEmployeeAsync(int employeeId);
    }
}
