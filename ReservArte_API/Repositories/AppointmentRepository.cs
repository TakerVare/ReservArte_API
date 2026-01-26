using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using System.Net.Http.Json;


namespace ReservArte_API.Repositories
{
     public class AppointmentRepository : IAppointmentRepository
    {
        private readonly HttpClient _httpClient;
        private const string Endpoint = "appointment";
        public AppointmentRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<AppointmentDtoToList>> GetAllAsync()
        {
            // var response = await _httpClient.GetAsync(Endpoint);
            // response.EnsureSuccessStatusCode();
            // return await response.Content.ReadFromJsonAsync<IEnumerable<AppointmentDtoToList>>();
            return await _httpClient.GetFromJsonAsync<IEnumerable<AppointmentDtoToList>>(Endpoint) ?? [];
        }
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{Endpoint}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Appointment>();
            }
            return null;
        }
        public async Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<AppointmentDtoToList>>($"{Endpoint}/user/{userId}") ?? [];
        }
        public async Task<IEnumerable<User>> GetCustomerAsync(int customerId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<User>>($"{Endpoint}/customer/{customerId}") ?? [];
        }
        public async Task<IEnumerable<User>> GetEmployeeAsync(int employeeId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<User>>($"{Endpoint}/employee/{employeeId}") ?? [];
        }
        public async Task<Appointment?> CreateAsync(Appointment appointment)
        {
            var response = await _httpClient.PostAsJsonAsync(Endpoint, appointment);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Appointment>();
            }
            return null;
        }
        public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
        {
            var response = await _httpClient.PutAsJsonAsync($"{Endpoint}/{id}", appointment);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Appointment>();
            }
            return null;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{Endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}