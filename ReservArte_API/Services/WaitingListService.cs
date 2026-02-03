using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class WaitingListService : IWaitingListService
{
    private readonly IWaitingListRepository _waitingListRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;

    public WaitingListService(
        IWaitingListRepository waitingListRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository)
    {
        _waitingListRepository = waitingListRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<WaitingListDtoOut?> GetByIdAsync(int id)
    {
        return await _waitingListRepository.GetByIdDetailedAsync(id);
    }

    public async Task<IEnumerable<WaitingListDtoOut>> GetByCustomerAsync(int customerId)
    {
        return await _waitingListRepository.GetByCustomerAsync(customerId);
    }

    public async Task<IEnumerable<WaitingListDtoOut>> GetAllAsync()
    {
        return await _waitingListRepository.GetAllAsync();
    }

    public async Task<WaitingListDtoOut?> AddToWaitingListAsync(WaitingListDtoIn dto)
    {
        // Validar cliente
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new InvalidOperationException("Cliente no encontrado");

        if (customer.IsBlocked)
            throw new InvalidOperationException("El cliente está bloqueado");

        // Validar servicio
        var service = await _serviceRepository.GetServiceByIdAsync(dto.ServiceId);
        if (service == null)
            throw new InvalidOperationException("Servicio no encontrado");

        // Validar rango de fechas
        if (dto.DateRangeStart >= dto.DateRangeEnd)
            throw new InvalidOperationException("El rango de fechas es inválido");

        if (dto.DateRangeStart < DateTime.UtcNow.Date)
            throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado");

        // Calcular prioridad
        var priority = await _waitingListRepository.CalculatePriorityAsync(dto.CustomerId);

        var waitingList = new WaitingList
        {
            CustomerId = dto.CustomerId,
            ServiceId = dto.ServiceId,
            PreferredEmployeeId = dto.PreferredEmployeeId,
            PreferredDate = dto.PreferredDate,
            DateRangeStart = dto.DateRangeStart,
            DateRangeEnd = dto.DateRangeEnd,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _waitingListRepository.CreateAsync(waitingList);
        if (created == null)
            throw new InvalidOperationException("Error al añadir a la lista de espera");

        return await _waitingListRepository.GetByIdDetailedAsync(created.Id);
    }

    public async Task<bool> RemoveFromWaitingListAsync(int id)
    {
        return await _waitingListRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<WaitingListDtoOut>> NotifyMatchingCustomersAsync(
        int serviceId, DateTime date, int? employeeId = null)
    {
        // Obtener clientes que coinciden con el slot liberado
        var matchingEntries = await _waitingListRepository.GetMatchingForSlotAsync(
            serviceId, date, employeeId);

        var notified = new List<WaitingListDtoOut>();

        foreach (var entry in matchingEntries)
        {
            // Marcar como notificado
            var marked = await _waitingListRepository.MarkNotifiedAsync(entry.Id);
            if (marked)
            {
                notified.Add(entry);
                
                // Aquí se podría integrar con un servicio de notificaciones
                // para enviar email/SMS al cliente
            }
        }

        return notified;
    }
}
