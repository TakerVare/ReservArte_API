using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;

namespace ReservArte_API.Services.Interfaces;

public interface IEmployeeService
{
    // Employee CRUD
    Task<IEnumerable<EmployeeDtoOut>> GetAllAsync();
    Task<EmployeeDtoOut?> GetByIdAsync(int id);
    Task<EmployeeDtoOut?> CreateAsync(EmployeeDtoIn employeeDto);
    Task<EmployeeDtoOut?> UpdateAsync(int id, EmployeeDtoIn employeeDto);
    Task<bool> DeleteAsync(int id);

    // Employee Availability
    Task<IEnumerable<EmployeeAvailabilityDto>> GetAvailabilityByEmployeeIdAsync(int employeeId);
    Task<EmployeeAvailabilityDto?> CreateAvailabilityAsync(int employeeId, EmployeeAvailabilityDto availabilityDto);
    Task<EmployeeAvailabilityDto?> UpdateAvailabilityAsync(int id, EmployeeAvailabilityDto availabilityDto);
    Task<bool> DeleteAvailabilityAsync(int id);

    // Employee Exceptions
    Task<IEnumerable<EmployeeExceptionDto>> GetExceptionsByEmployeeIdAsync(int employeeId);
    Task<EmployeeExceptionDto?> CreateExceptionAsync(int employeeId, EmployeeExceptionDto exceptionDto);
    Task<EmployeeExceptionDto?> UpdateExceptionAsync(int id, EmployeeExceptionDto exceptionDto);
    Task<bool> DeleteExceptionAsync(int id);

    // Employee Services
    Task<IEnumerable<EmployeeServiceDto>> GetServicesByEmployeeIdAsync(int employeeId);
    Task<EmployeeServiceDto?> AssignServiceAsync(int employeeId, EmployeeServiceDto serviceDto);
    Task<bool> RemoveServiceAsync(int employeeId, int serviceId);
}
