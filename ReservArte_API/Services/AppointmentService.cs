using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories;
using ReservArte_API.Services.Interfaces;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IConfiguration _configuration;
        private readonly IAppointmentRepository _repository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _repository = appointmentRepository;
        }

        public async Task<IEnumerable<AppointmentDtoToList>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }
        public async Task<IEnumerable<User>> GetCustomerAsync(int customerId)
        {
            return await  _repository.GetCustomerAsync(customerId);
        }
        public async Task<IEnumerable<User>> GetEmployeeAsync(int employeeId)
        {
            return await _repository.GetEmployeeAsync(employeeId);
        }
        public async Task<Appointment?> CreateAsync(Appointment appointment)
        {
            return await _repository.CreateAsync(appointment);
        }
        public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
        {
            return await _repository.UpdateAsync(id, appointment);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}






        