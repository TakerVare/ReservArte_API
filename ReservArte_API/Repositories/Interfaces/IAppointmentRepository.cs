using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<AppointmentDtoToList>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId);
        Task<IEnumerable<User>> GetCustomerAsync(int customerId);
        Task<IEnumerable<User>> GetEmployeeAsync(int employeeId);
        Task<Appointment?> CreateAsync(Appointment appointment);
        Task<Appointment?> UpdateAsync(int id, Appointment appointment);
        Task<bool> DeleteAsync(int id);
    }
}
