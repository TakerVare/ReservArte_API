using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository, IConfiguration configuration)
    {
        _repository = employeeRepository;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    #region Employee CRUD

    public async Task<IEnumerable<EmployeeDtoOut>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<EmployeeDtoOut?> GetByIdAsync(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return null;

        return MapToEmployeeDtoOut(employee);
    }

    public async Task<EmployeeDtoOut?> CreateAsync(EmployeeDtoIn employeeDto)
    {
        var password = employeeDto.Password ?? _configuration["DefaultNewUserPassword"] ?? "ChangeMe123!";
        var userId = await _userRepository.CreateUserAsync(
            employeeDto.FirstName,
            employeeDto.LastName,
            employeeDto.Email,
            password,
            Roles.Employee,
            employeeDto.Phone,
            employeeDto.ProfileImageUrl);

        var employee = new Employee
        {
            Id = userId,
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
            Phone = employeeDto.Phone,
            ProfileImageUrl = employeeDto.ProfileImageUrl,
            HireDate = employeeDto.HireDate,
            IsActive = employeeDto.IsActive,
            Rol = Roles.Employee
        };

        var created = await _repository.CreateAsync(employee);
        if (created == null) return null;

        return MapToEmployeeDtoOut(created);
    }

    public async Task<EmployeeDtoOut?> UpdateAsync(int id, EmployeeDtoIn employeeDto)
    {
        var existingEmployee = await _repository.GetByIdAsync(id);
        if (existingEmployee == null) return null;

        existingEmployee.FirstName = employeeDto.FirstName;
        existingEmployee.LastName = employeeDto.LastName;
        existingEmployee.Email = employeeDto.Email;
        existingEmployee.Phone = employeeDto.Phone;
        existingEmployee.ProfileImageUrl = employeeDto.ProfileImageUrl;
        existingEmployee.HireDate = employeeDto.HireDate;
        existingEmployee.IsActive = employeeDto.IsActive;

        var updated = await _repository.UpdateAsync(id, existingEmployee);
        if (updated == null) return null;

        return MapToEmployeeDtoOut(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    #endregion

    #region Employee Availability

    public async Task<IEnumerable<EmployeeAvailabilityDto>> GetAvailabilityByEmployeeIdAsync(int employeeId)
    {
        var availabilities = await _repository.GetAvailabilityByEmployeeIdAsync(employeeId);
        return availabilities.Select(MapToAvailabilityDto);
    }

    public async Task<EmployeeAvailabilityDto?> CreateAvailabilityAsync(int employeeId, EmployeeAvailabilityDto availabilityDto)
    {
        // Verify employee exists
        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null) return null;

        var availability = new EmployeeAvailability
        {
            EmployeeId = employeeId,
            DayOfWeek = availabilityDto.DayOfWeek,
            StartTime = availabilityDto.StartTime,
            EndTime = availabilityDto.EndTime,
            IsRecurring = availabilityDto.IsRecurring
        };

        var created = await _repository.CreateAvailabilityAsync(availability);
        if (created == null) return null;

        return MapToAvailabilityDto(created);
    }

    public async Task<EmployeeAvailabilityDto?> UpdateAvailabilityAsync(int id, EmployeeAvailabilityDto availabilityDto)
    {
        var availability = new EmployeeAvailability
        {
            Id = id,
            EmployeeId = availabilityDto.EmployeeId,
            DayOfWeek = availabilityDto.DayOfWeek,
            StartTime = availabilityDto.StartTime,
            EndTime = availabilityDto.EndTime,
            IsRecurring = availabilityDto.IsRecurring
        };

        var updated = await _repository.UpdateAvailabilityAsync(id, availability);
        if (updated == null) return null;

        return MapToAvailabilityDto(updated);
    }

    public async Task<bool> DeleteAvailabilityAsync(int id)
    {
        return await _repository.DeleteAvailabilityAsync(id);
    }

    #endregion

    #region Employee Exceptions

    public async Task<IEnumerable<EmployeeExceptionDto>> GetExceptionsByEmployeeIdAsync(int employeeId)
    {
        var exceptions = await _repository.GetExceptionsByEmployeeIdAsync(employeeId);
        return exceptions.Select(MapToExceptionDto);
    }

    public async Task<EmployeeExceptionDto?> CreateExceptionAsync(int employeeId, EmployeeExceptionDto exceptionDto)
    {
        // Verify employee exists
        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null) return null;

        var exception = new EmployeeException
        {
            EmployeeId = employeeId,
            StartDateTime = exceptionDto.StartDateTime,
            EndDateTime = exceptionDto.EndDateTime,
            Reason = exceptionDto.Reason,
            Type = ValidateExceptionType(exceptionDto.Reason) // Use reason to infer type if needed
        };

        var created = await _repository.CreateExceptionAsync(exception);
        if (created == null) return null;

        return MapToExceptionDto(created);
    }

    public async Task<EmployeeExceptionDto?> UpdateExceptionAsync(int id, EmployeeExceptionDto exceptionDto)
    {
        var exception = new EmployeeException
        {
            Id = id,
            EmployeeId = exceptionDto.EmployeeId,
            StartDateTime = exceptionDto.StartDateTime,
            EndDateTime = exceptionDto.EndDateTime,
            Reason = exceptionDto.Reason,
            Type = ValidateExceptionType(exceptionDto.Reason)
        };

        var updated = await _repository.UpdateExceptionAsync(id, exception);
        if (updated == null) return null;

        return MapToExceptionDto(updated);
    }

    public async Task<bool> DeleteExceptionAsync(int id)
    {
        return await _repository.DeleteExceptionAsync(id);
    }

    #endregion

    #region Employee Services

    public async Task<IEnumerable<EmployeeServiceDto>> GetServicesByEmployeeIdAsync(int employeeId)
    {
        return await _repository.GetServicesByEmployeeIdAsync(employeeId);
    }

    public async Task<EmployeeServiceDto?> AssignServiceAsync(int employeeId, EmployeeServiceDto serviceDto)
    {
        // Verify employee exists
        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee == null) return null;

        var employeeService = new EmployeeServiceLink
        {
            EmployeeId = employeeId,
            ServiceId = serviceDto.ServiceId,
            ProficiencyLevel = serviceDto.ProficiencyLevel
        };

        var assigned = await _repository.AssignServiceAsync(employeeService);
        if (assigned == null) return null;

        serviceDto.EmployeeId = employeeId;
        return serviceDto;
    }

    public async Task<bool> RemoveServiceAsync(int employeeId, int serviceId)
    {
        return await _repository.RemoveServiceAsync(employeeId, serviceId);
    }

    #endregion

    #region Private Helpers

    private static EmployeeDtoOut MapToEmployeeDtoOut(Employee employee)
    {
        return new EmployeeDtoOut
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            ProfileImageUrl = employee.ProfileImageUrl,
            HireDate = employee.HireDate?.ToString("yyyy-MM-dd"),
            IsActive = employee.IsActive
        };
    }

    private static EmployeeAvailabilityDto MapToAvailabilityDto(EmployeeAvailability availability)
    {
        return new EmployeeAvailabilityDto
        {
            Id = availability.Id,
            EmployeeId = availability.EmployeeId,
            DayOfWeek = availability.DayOfWeek,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            IsRecurring = availability.IsRecurring
        };
    }

    private static EmployeeExceptionDto MapToExceptionDto(EmployeeException exception)
    {
        return new EmployeeExceptionDto
        {
            Id = exception.Id,
            EmployeeId = exception.EmployeeId,
            StartDateTime = exception.StartDateTime,
            EndDateTime = exception.EndDateTime,
            StartTime = exception.StartDateTime.TimeOfDay,
            EndTime = exception.EndDateTime.TimeOfDay,
            Reason = exception.Reason
        };
    }

    private static string ValidateExceptionType(string? reason)
    {
        // Default to Other if no specific type is provided
        if (string.IsNullOrWhiteSpace(reason))
            return EmployeeExceptionType.Other;

        var lowerReason = reason.ToLowerInvariant();
        
        if (lowerReason.Contains("vacacion") || lowerReason.Contains("vacation"))
            return EmployeeExceptionType.Vacation;
        
        if (lowerReason.Contains("enferm") || lowerReason.Contains("sick") || lowerReason.Contains("baja"))
            return EmployeeExceptionType.Sick;

        return EmployeeExceptionType.Other;
    }

    #endregion
}
