using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Repositories.Interfaces;

public interface IEmployeeRepository
{
    // Employee CRUD
    Task<IEnumerable<EmployeeDtoOut>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> CreateAsync(Employee employee);
    Task<Employee?> UpdateAsync(int id, Employee employee);
    Task<bool> DeleteAsync(int id);

    // Employee Availability
    Task<IEnumerable<EmployeeAvailability>> GetAvailabilityByEmployeeIdAsync(int employeeId);
    Task<EmployeeAvailability?> CreateAvailabilityAsync(EmployeeAvailability availability);
    Task<EmployeeAvailability?> UpdateAvailabilityAsync(int id, EmployeeAvailability availability);
    Task<bool> DeleteAvailabilityAsync(int id);

    // Employee Exceptions (vacations, sick days, etc.)
    Task<IEnumerable<EmployeeException>> GetExceptionsByEmployeeIdAsync(int employeeId);
    Task<EmployeeException?> CreateExceptionAsync(EmployeeException exception);
    Task<EmployeeException?> UpdateExceptionAsync(int id, EmployeeException exception);
    Task<bool> DeleteExceptionAsync(int id);

    // Employee Services
    Task<IEnumerable<EmployeeServiceDto>> GetServicesByEmployeeIdAsync(int employeeId);
    Task<EmployeeServiceLink?> AssignServiceAsync(EmployeeServiceLink employeeService);
    Task<bool> RemoveServiceAsync(int employeeId, int serviceId);
}
