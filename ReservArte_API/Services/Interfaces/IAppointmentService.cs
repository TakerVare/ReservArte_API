using System.Security.Claims;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories;
using ReservArte_API.Services;

namespace ReservArte_API.Services.Interfaces
{
    public interface IAppointmentService
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
