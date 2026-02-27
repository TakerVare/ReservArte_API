using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ICancellationPolicyRepository _cancellationPolicyRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IEmployeeRepository employeeRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository,
        ICancellationPolicyRepository cancellationPolicyRepository)
    {
        _appointmentRepository = appointmentRepository;
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _cancellationPolicyRepository = cancellationPolicyRepository;
    }

    #region CRUD Básico

    public async Task<IEnumerable<AppointmentDtoToList>> GetAllAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdAsync(id);
    }

    public async Task<AppointmentDtoOut?> GetByIdDetailedAsync(int id)
    {
        return await _appointmentRepository.GetByIdDetailedAsync(id);
    }

    public async Task<IEnumerable<AppointmentDtoToList>> GetByUserIdAsync(int userId)
    {
        return await _appointmentRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _appointmentRepository.DeleteAsync(id);
    }

    #endregion

    #region Creación de Citas

    public async Task<AppointmentDtoOut?> CreateAsync(AppointmentDtoIn dto)
    {
        // 1. Verificar que el cliente existe y no está bloqueado
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Cliente no encontrado");
        
        if (customer.IsBlocked)
            throw new InvalidOperationException("El cliente está bloqueado y no puede reservar citas");

        // 2. Verificar que el empleado existe y está activo
        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
        if (employee == null)
            throw new InvalidOperationException("Empleado no encontrado");
        
        if (!employee.IsActive)
            throw new InvalidOperationException("El empleado no está disponible");

        // 3. Calcular duración total y precio de los servicios
        var (totalDuration, totalPrice, serviceItems) = await CalculateServicesAsync(dto.Services);
        
        if (totalDuration == 0)
            throw new InvalidOperationException("Los servicios seleccionados no son válidos");

        // 4. Calcular hora de fin
        var endTime = dto.StartTime.AddMinutes(totalDuration);

        // 5. Verificar disponibilidad del empleado (horario regular)
        var isAvailable = await CheckEmployeeAvailabilityAsync(dto.EmployeeId, dto.AppointmentDate, dto.StartTime, endTime);
        if (!isAvailable)
            throw new InvalidOperationException("El empleado no está disponible en ese horario");

        // 6. Verificar que no hay excepciones (vacaciones, etc.)
        var hasException = await CheckEmployeeExceptionAsync(dto.EmployeeId, dto.AppointmentDate, dto.StartTime, endTime);
        if (hasException)
            throw new InvalidOperationException("El empleado tiene una excepción programada para ese horario");

        // 7. Verificar que no hay solapamiento con otras citas
        var hasOverlap = await _appointmentRepository.CheckOverlapAsync(dto.EmployeeId, dto.AppointmentDate, dto.StartTime, endTime);
        if (hasOverlap)
            throw new InvalidOperationException("Ya existe una cita en ese horario");

        // 8. Crear la cita
        var appointment = new Appointment
        {
            CustomerId = dto.CustomerId,
            EmployeeId = dto.EmployeeId,
            AppointmentDate = dto.AppointmentDate,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Status = Status.Pending,
            TotalPrice = totalPrice,
            DepositAmount = 0, // Se puede configurar según política
            PaymentMethodId = dto.PaymentMethodId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _appointmentRepository.CreateAsync(appointment);
        if (created == null)
            throw new InvalidOperationException("Error al crear la cita");

        // 9. Añadir los servicios a la cita
        await _appointmentRepository.AddAppointmentServicesAsync(created.Id, serviceItems);

        // 10. Retornar la cita detallada
        return await _appointmentRepository.GetByIdDetailedAsync(created.Id);
    }

    public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        return await _appointmentRepository.UpdateAsync(id, appointment);
    }

    #endregion

    #region Consultas de Agenda

    public async Task<AgendaResponseDto> GetAgendaAsync(AgendaQueryDto query)
    {
        IEnumerable<AgendaAppointmentDto> appointments;

        if (query.EmployeeId.HasValue)
        {
            appointments = await _appointmentRepository.GetByEmployeeAsync(
                query.EmployeeId.Value, query.StartDate, query.EndDate);
        }
        else
        {
            appointments = await _appointmentRepository.GetByDateRangeAsync(
                query.StartDate, query.EndDate);
        }

        // Filtrar por estado si se especifica
        if (!string.IsNullOrEmpty(query.Status))
        {
            appointments = appointments.Where(a => a.Status == query.Status);
        }

        // Agrupar por día
        var groupedByDay = appointments
            .GroupBy(a => DateOnly.Parse(a.StartTime.ToString("yyyy-MM-dd")))
            .OrderBy(g => g.Key);

        var days = new List<AgendaDayViewDto>();
        var currentDate = query.StartDate;

        while (currentDate <= query.EndDate)
        {
            var dayAppointments = appointments
                .Where(a => true) // Ya están filtrados
                .ToList();

            // Nota: Como AgendaAppointmentDto no tiene AppointmentDate directamente,
            // se debería incluir o filtrar de otra manera
            
            days.Add(new AgendaDayViewDto
            {
                Date = currentDate,
                DayOfWeek = currentDate.DayOfWeek.ToString(),
                Appointments = dayAppointments
            });

            currentDate = currentDate.AddDays(1);
        }

        // Calcular estadísticas
        var allAppointments = appointments.ToList();
        var stats = new AgendaStatsDto
        {
            TotalAppointments = allAppointments.Count,
            PendingCount = allAppointments.Count(a => a.Status == Status.Pending),
            ConfirmedCount = allAppointments.Count(a => a.Status == Status.Confirmed),
            CompletedCount = allAppointments.Count(a => a.Status == Status.Completed),
            CancelledCount = allAppointments.Count(a => 
                a.Status == Status.CancelledByCustomer || 
                a.Status == Status.CancelledByBusiness || 
                a.Status == Status.Cancelled),
            NoShowCount = allAppointments.Count(a => a.Status == Status.NoShow),
            TotalRevenue = allAppointments
                .Where(a => a.Status == Status.Completed)
                .Sum(a => a.TotalPrice)
        };

        return new AgendaResponseDto
        {
            StartDate = query.StartDate,
            EndDate = query.EndDate,
            ViewType = query.ViewType,
            Days = days,
            Stats = stats
        };
    }

    public async Task<AvailableSlotsResponseDto> GetAvailableSlotsAsync(AvailableSlotsRequestDto request)
    {
        // Obtener información del servicio
        var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
        if (service == null)
            throw new InvalidOperationException("Servicio no encontrado");

        var duration = service.DurationMinutes;

        // Si hay variación, ajustar duración
        if (request.ServiceVariationId.HasValue)
        {
            var variation = await _serviceRepository.GetVariationByIdAsync(request.ServiceVariationId.Value);
            if (variation != null)
            {
                duration += variation.DurationModifier;
            }
        }

        var response = new AvailableSlotsResponseDto
        {
            Date = request.Date,
            ServiceId = request.ServiceId,
            ServiceName = service.Name,
            DurationMinutes = duration,
            EmployeeSlots = new List<EmployeeSlotsDto>()
        };

        // Obtener empleados que pueden realizar el servicio
        var employees = await GetEmployeesForServiceAsync(request.ServiceId, request.EmployeeId);

        foreach (var employee in employees)
        {
            var slots = await GetAvailableSlotsForEmployeeAsync(
                employee.Id, request.Date, duration);

            if (slots.Any())
            {
                response.EmployeeSlots.Add(new EmployeeSlotsDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    Slots = slots.ToList()
                });
            }
        }

        return response;
    }

    public async Task<IEnumerable<AgendaAppointmentDto>> GetByCustomerIdAsync(int customerId, CustomerAppointmentsQueryDto? filter = null)
    {
        return await _appointmentRepository.GetByCustomerAsync(customerId, filter);
    }

    public async Task<IEnumerable<AgendaAppointmentDto>> GetByEmployeeIdAsync(int employeeId, DateOnly startDate, DateOnly endDate)
    {
        return await _appointmentRepository.GetByEmployeeAsync(employeeId, startDate, endDate);
    }

    #endregion

    #region Gestión de Estados

    public async Task<AppointmentDtoOut?> UpdateStatusAsync(int id, AppointmentStatusUpdateDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            return null;

        var newStatus = Status.Normalize(dto.Status);
        if (newStatus == null)
            throw new InvalidOperationException($"Estado de cita no válido: '{dto.Status}'. Valores esperados: pending, confirmed, in_progress, completed, cancelled_by_customer, cancelled_by_business, no_show.");

        // Validar transición de estado
        if (!IsValidStatusTransition(appointment.Status, newStatus))
            throw new InvalidOperationException($"No se puede cambiar de {appointment.Status} a {newStatus}");

        var updated = await _appointmentRepository.UpdateStatusAsync(id, newStatus, dto.Notes);
        if (!updated)
            return null;

        return await _appointmentRepository.GetByIdDetailedAsync(id);
    }

    public async Task<AppointmentCancelResultDto> CancelAsync(int id, AppointmentCancelDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException("Cita no encontrada");

        if (!Status.IsCancellable(appointment.Status))
            throw new InvalidOperationException("La cita no se puede cancelar en su estado actual");

        // Validar tipo de quien cancela
        if (!CancelledByType.IsValid(dto.CancelledByType))
            throw new InvalidOperationException("Tipo de cancelación inválido");

        // Calcular penalización
        var (penaltyApplied, penaltyAmount, penaltyPercentage) = 
            await CalculateCancellationPenaltyAsync(appointment, dto.IsJustified);

        // Cancelar la cita
        var cancelled = await _appointmentRepository.CancelAsync(
            id, dto.Reason, dto.CancelledById, dto.CancelledByType);

        if (!cancelled)
            throw new InvalidOperationException("Error al cancelar la cita");

        var status = dto.CancelledByType == CancelledByType.Customer
            ? Status.CancelledByCustomer
            : Status.CancelledByBusiness;

        var message = penaltyApplied
            ? $"Cita cancelada. Se ha aplicado una penalización del {penaltyPercentage}% ({penaltyAmount:C})"
            : "Cita cancelada sin penalización";

        return new AppointmentCancelResultDto
        {
            AppointmentId = id,
            Status = status,
            CancelledAt = DateTime.UtcNow,
            PenaltyApplied = penaltyApplied,
            PenaltyAmount = penaltyAmount,
            PenaltyPercentage = penaltyPercentage,
            Message = message
        };
    }

    public async Task<AppointmentDtoOut?> RescheduleAsync(int id, AppointmentRescheduleDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException("Cita no encontrada");

        if (Status.IsFinal(appointment.Status))
            throw new InvalidOperationException("No se puede reagendar una cita finalizada");

        var employeeId = dto.NewEmployeeId ?? appointment.EmployeeId;
        var duration = (int)(appointment.EndTime - appointment.StartTime).TotalMinutes;
        var newEndTime = dto.NewStartTime.AddMinutes(duration);

        // Verificar disponibilidad en la nueva fecha/hora
        var isAvailable = await CheckAvailabilityAsync(
            employeeId, dto.NewDate, dto.NewStartTime, newEndTime, id);

        if (!isAvailable)
            throw new InvalidOperationException("El horario solicitado no está disponible");

        // Actualizar cita
        appointment.AppointmentDate = dto.NewDate;
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = newEndTime;
        appointment.EmployeeId = employeeId;
        appointment.Notes = string.IsNullOrEmpty(dto.Reason) 
            ? appointment.Notes 
            : $"{appointment.Notes}\n[Reagendado: {dto.Reason}]";
        appointment.UpdatedAt = DateTime.UtcNow;

        var updated = await _appointmentRepository.UpdateAsync(id, appointment);
        if (updated == null)
            throw new InvalidOperationException("Error al reagendar la cita");

        return await _appointmentRepository.GetByIdDetailedAsync(id);
    }

    public async Task<AppointmentCancelResultDto> MarkAsNoShowAsync(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
            throw new InvalidOperationException("Cita no encontrada");

        if (appointment.Status != Status.Confirmed)
            throw new InvalidOperationException("Solo se pueden marcar como no-show las citas confirmadas");

        // Actualizar estado
        await _appointmentRepository.UpdateStatusAsync(id, Status.NoShow);

        // Verificar si el cliente debe ser bloqueado
        var noShowCount = await _appointmentRepository.GetCustomerNoShowCountAsync(appointment.CustomerId);

        var policy = await _cancellationPolicyRepository.GetActiveAsync();
        var shouldBlock = policy != null && noShowCount >= policy.MaxNoShowsBeforeBlock;

        if (shouldBlock)
        {
            await _customerRepository.BlockCustomerAsync(
                appointment.CustomerId, 
                $"Bloqueado automáticamente por {noShowCount} no-shows");
        }

        // Penalización por no-show (100%)
        var penaltyAmount = appointment.TotalPrice;

        return new AppointmentCancelResultDto
        {
            AppointmentId = id,
            Status = Status.NoShow,
            CancelledAt = DateTime.UtcNow,
            PenaltyApplied = true,
            PenaltyAmount = penaltyAmount,
            PenaltyPercentage = 100,
            Message = shouldBlock 
                ? $"Cliente marcado como no-show. Se ha bloqueado por alcanzar {noShowCount} no-shows."
                : $"Cliente marcado como no-show. Total de no-shows: {noShowCount}"
        };
    }

    #endregion

    #region Validaciones

    public async Task<bool> CheckAvailabilityAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime, int? excludeAppointmentId = null)
    {
        // Verificar horario del empleado
        var isInSchedule = await CheckEmployeeAvailabilityAsync(employeeId, date, startTime, endTime);
        if (!isInSchedule)
            return false;

        // Verificar excepciones
        var hasException = await CheckEmployeeExceptionAsync(employeeId, date, startTime, endTime);
        if (hasException)
            return false;

        // Verificar solapamiento
        var hasOverlap = await _appointmentRepository.CheckOverlapAsync(
            employeeId, date, startTime, endTime, excludeAppointmentId);

        return !hasOverlap;
    }

    public async Task<decimal> CalculatePenaltyAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
            return 0;

        var (_, amount, _) = await CalculateCancellationPenaltyAsync(appointment, false);
        return amount;
    }

    #endregion

    #region Compatibilidad

    public async Task<IEnumerable<User>> GetCustomerAsync(int customerId)
    {
        return await _appointmentRepository.GetCustomerAsync(customerId);
    }

    public async Task<IEnumerable<User>> GetEmployeeAsync(int employeeId)
    {
        return await _appointmentRepository.GetEmployeeAsync(employeeId);
    }

    #endregion

    #region Métodos Privados

    private async Task<(int duration, decimal price, List<AppointmentServiceItem> items)> CalculateServicesAsync(
        List<AppointmentServiceItemDtoIn> serviceDtos)
    {
        var totalDuration = 0;
        var totalPrice = 0m;
        var items = new List<AppointmentServiceItem>();
        var order = 1;

        foreach (var serviceDto in serviceDtos)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(serviceDto.ServiceId);
            if (service == null || !service.IsActive)
                continue;

            var duration = service.DurationMinutes;
            var price = service.BasePrice;

            if (serviceDto.ServiceVariationId.HasValue)
            {
                var variation = await _serviceRepository.GetVariationByIdAsync(serviceDto.ServiceVariationId.Value);
                if (variation != null && variation.IsActive)
                {
                    duration += variation.DurationModifier;
                    price += variation.PriceModifier;
                }
            }

            totalDuration += duration;
            totalPrice += price;

            items.Add(new AppointmentServiceItem
            {
                ServiceId = serviceDto.ServiceId,
                ServiceVariationId = serviceDto.ServiceVariationId,
                Price = price,
                DurationMinutes = duration,
                Order = order++
            });
        }

        return (totalDuration, totalPrice, items);
    }

    private async Task<bool> CheckEmployeeAvailabilityAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var availabilities = await _employeeRepository.GetAvailabilityByEmployeeIdAsync(employeeId);
        
        return availabilities.Any(a => 
            a.DayOfWeek == dayOfWeek &&
            TimeOnly.FromTimeSpan(a.StartTime) <= startTime &&
            TimeOnly.FromTimeSpan(a.EndTime) >= endTime);
    }

    private async Task<bool> CheckEmployeeExceptionAsync(int employeeId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var exceptions = await _employeeRepository.GetExceptionsByEmployeeIdAsync(employeeId);
        var appointmentStart = date.ToDateTime(startTime);
        var appointmentEnd = date.ToDateTime(endTime);

        return exceptions.Any(e =>
            appointmentStart < e.EndDateTime &&
            appointmentEnd > e.StartDateTime);
    }

    private async Task<IEnumerable<Employee>> GetEmployeesForServiceAsync(int serviceId, int? specificEmployeeId)
    {
        // Si se especifica un empleado, solo verificar ese
        if (specificEmployeeId.HasValue)
        {
            var employee = await _employeeRepository.GetByIdAsync(specificEmployeeId.Value);
            if (employee != null && employee.IsActive)
            {
                var employeeServices = await _employeeRepository.GetServicesByEmployeeIdAsync(employee.Id);
                if (employeeServices.Any(s => s.ServiceId == serviceId))
                {
                    return new[] { employee };
                }
            }
            return Enumerable.Empty<Employee>();
        }

        // Obtener todos los empleados que pueden hacer el servicio
        var allEmployees = await _employeeRepository.GetAllAsync();
        var result = new List<Employee>();

        foreach (var empDto in allEmployees)
        {
            var emp = await _employeeRepository.GetByIdAsync(empDto.Id);
            if (emp != null && emp.IsActive)
            {
                var services = await _employeeRepository.GetServicesByEmployeeIdAsync(emp.Id);
                if (services.Any(s => s.ServiceId == serviceId))
                {
                    result.Add(emp);
                }
            }
        }

        return result;
    }

    private async Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsForEmployeeAsync(
        int employeeId, DateOnly date, int durationMinutes)
    {
        var slots = new List<TimeSlotDto>();
        var dayOfWeek = (int)date.DayOfWeek;

        // Obtener horario del empleado para ese día
        var availabilities = await _employeeRepository.GetAvailabilityByEmployeeIdAsync(employeeId);
        var dayAvailability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

        if (dayAvailability == null)
            return slots;

        // Verificar excepciones
        var exceptions = await _employeeRepository.GetExceptionsByEmployeeIdAsync(employeeId);
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        var hasFullDayException = exceptions.Any(e =>
            e.StartDateTime <= dateTime &&
            e.EndDateTime >= dateTime.AddDays(1));

        if (hasFullDayException)
            return slots;

        // Generar slots cada 30 minutos
        var slotInterval = 30;
        var currentTime = TimeOnly.FromTimeSpan(dayAvailability.StartTime);
        var endTime = TimeOnly.FromTimeSpan(dayAvailability.EndTime);

        while (currentTime.AddMinutes(durationMinutes) <= endTime)
        {
            var slotEndTime = currentTime.AddMinutes(durationMinutes);

            // Verificar si no hay solapamiento
            var hasOverlap = await _appointmentRepository.CheckOverlapAsync(
                employeeId, date, currentTime, slotEndTime);

            if (!hasOverlap)
            {
                slots.Add(new TimeSlotDto
                {
                    StartTime = currentTime,
                    EndTime = slotEndTime
                });
            }

            currentTime = currentTime.AddMinutes(slotInterval);
        }

        return slots;
    }

    private async Task<(bool applied, decimal amount, int percentage)> CalculateCancellationPenaltyAsync(
        Appointment appointment, bool isJustified)
    {
        if (isJustified)
            return (false, 0, 0);

        var policy = await _cancellationPolicyRepository.GetActiveAsync();
        if (policy == null || !policy.IsActive)
            return (false, 0, 0);

        // Obtener categoría del cliente para verificar si es VIP
        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId);
        var isVip = customer?.Category == CustomerCategory.VIP;

        var minHours = isVip && policy.VipMinHoursBeforeCancel.HasValue
            ? policy.VipMinHoursBeforeCancel.Value
            : policy.MinHoursBeforeCancel;

        var penaltyPercentage = isVip && policy.VipPenaltyPercentage.HasValue
            ? policy.VipPenaltyPercentage.Value
            : policy.PenaltyPercentage;

        // Calcular tiempo hasta la cita
        var appointmentDateTime = appointment.AppointmentDate.ToDateTime(appointment.StartTime);
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;

        if (hoursUntilAppointment >= minHours)
            return (false, 0, 0);

        var penaltyAmount = appointment.TotalPrice * penaltyPercentage / 100;
        return (true, penaltyAmount, penaltyPercentage);
    }

    private static bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        return (currentStatus, newStatus) switch
        {
            (Status.Pending, Status.Confirmed) => true,
            (Status.Pending, Status.CancelledByCustomer) => true,
            (Status.Pending, Status.CancelledByBusiness) => true,
            (Status.Confirmed, Status.InProgress) => true,
            (Status.Confirmed, Status.CancelledByCustomer) => true,
            (Status.Confirmed, Status.CancelledByBusiness) => true,
            (Status.Confirmed, Status.NoShow) => true,
            (Status.InProgress, Status.Completed) => true,
            _ => false
        };
    }

    #endregion
}
