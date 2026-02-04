/*
 * PaymentFilterDto.cs
 * ===================
 * 
 * PROPÓSITO:
 * DTO para filtrar y paginar listados de pagos.
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: GET /api/payment (query parameters)
 * - PaymentService.cs: GetFilteredAsync()
 * - PaymentRepository.cs: GetFilteredAsync()
 * 
 * PARA QUÉ SE USA:
 * - Filtrar pagos por fecha, estado, método de pago, cliente
 * - Paginación de resultados
 * - Búsqueda avanzada de pagos
 * - Generación de reportes (futuro)
 */

namespace ReservArte_API.Models.DTOs;

public class PaymentFilterDto
{
    /// <summary>
    /// Fecha inicio del rango (inclusive)
    /// </summary>
    public DateOnly? StartDate { get; set; }
    
    /// <summary>
    /// Fecha fin del rango (inclusive)
    /// </summary>
    public DateOnly? EndDate { get; set; }
    
    /// <summary>
    /// Filtrar por estado del pago
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Filtrar por método de pago
    /// </summary>
    public string? PaymentMethodType { get; set; }
    
    /// <summary>
    /// Filtrar por cliente
    /// </summary>
    public int? CustomerId { get; set; }
    
    /// <summary>
    /// Filtrar por cita
    /// </summary>
    public int? AppointmentId { get; set; }
    
    /// <summary>
    /// Buscar por número de pedido Redsys
    /// </summary>
    public string? RedsysOrderNumber { get; set; }
    
    /// <summary>
    /// Importe mínimo
    /// </summary>
    public decimal? MinAmount { get; set; }
    
    /// <summary>
    /// Importe máximo
    /// </summary>
    public decimal? MaxAmount { get; set; }
    
    /// <summary>
    /// Número de página (1-indexed)
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Elementos por página
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Campo por el que ordenar
    /// </summary>
    public string OrderBy { get; set; } = "CreatedAt";
    
    /// <summary>
    /// Orden descendente
    /// </summary>
    public bool OrderDescending { get; set; } = true;
}

/// <summary>
/// Respuesta paginada de pagos
/// </summary>
public class PaymentPagedResultDto
{
    public IEnumerable<PaymentListDto> Items { get; set; } = new List<PaymentListDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    
    /// <summary>
    /// Suma total de los pagos filtrados (para resumen)
    /// </summary>
    public decimal TotalAmount { get; set; }
}
