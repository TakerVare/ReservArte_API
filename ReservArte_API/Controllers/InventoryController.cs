using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IProductService _productService;

    public InventoryController(IProductService productService)
    {
        _productService = productService;
    }

    #region Movimientos de Inventario

    /// <summary>
    /// Obtiene el historial de movimientos de inventario.
    /// </summary>
    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<InventoryMovementDtoOut>>> GetMovements(
        [FromQuery] int? productId,
        [FromQuery] string? movementType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var filter = new InventoryMovementFilterDto
        {
            ProductId = productId,
            MovementType = movementType,
            FromDate = fromDate,
            ToDate = toDate
        };

        var movements = await _productService.GetMovementsAsync(filter);
        return Ok(movements);
    }

    /// <summary>
    /// Registra una compra/entrada de stock.
    /// </summary>
    [HttpPost("purchase")]
    public async Task<ActionResult<InventoryMovementDtoOut>> RegisterPurchase([FromBody] PurchaseDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employeeId = GetEmployeeId();
        if (employeeId == null)
        {
            return Unauthorized(new { message = "No se pudo identificar al empleado" });
        }

        try
        {
            var movement = await _productService.RegisterPurchaseAsync(dto, employeeId.Value);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registra un ajuste manual de inventario.
    /// </summary>
    [HttpPost("adjustment")]
    public async Task<ActionResult<InventoryMovementDtoOut>> AdjustStock([FromBody] StockAdjustmentDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employeeId = GetEmployeeId();
        if (employeeId == null)
        {
            return Unauthorized(new { message = "No se pudo identificar al empleado" });
        }

        try
        {
            var movement = await _productService.AdjustStockAsync(dto, employeeId.Value);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registra una merma o pérdida de producto.
    /// </summary>
    [HttpPost("waste")]
    public async Task<ActionResult<InventoryMovementDtoOut>> RegisterWaste([FromBody] WasteDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employeeId = GetEmployeeId();
        if (employeeId == null)
        {
            return Unauthorized(new { message = "No se pudo identificar al empleado" });
        }

        try
        {
            var movement = await _productService.RegisterWasteAsync(dto.ProductId, dto.Quantity, dto.Notes, employeeId.Value);
            return Ok(movement);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Ventas

    /// <summary>
    /// Crea una nueva venta de productos.
    /// </summary>
    [HttpPost("sales")]
    public async Task<ActionResult<ProductSaleDtoOut>> CreateSale([FromBody] ProductSaleDtoIn dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employeeId = GetEmployeeId();
        if (employeeId == null)
        {
            return Unauthorized(new { message = "No se pudo identificar al empleado" });
        }

        try
        {
            var sale = await _productService.CreateSaleAsync(dto, employeeId.Value);
            return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, sale);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de ventas.
    /// </summary>
    [HttpGet("sales")]
    public async Task<ActionResult<IEnumerable<ProductSaleDtoOut>>> GetSales(
        [FromQuery] int? customerId,
        [FromQuery] int? appointmentId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var filter = new ProductSaleFilterDto
        {
            CustomerId = customerId,
            AppointmentId = appointmentId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        var sales = await _productService.GetSalesAsync(filter);
        return Ok(sales);
    }

    /// <summary>
    /// Obtiene una venta por su ID.
    /// </summary>
    [HttpGet("sales/{id}")]
    public async Task<ActionResult<ProductSaleDtoOut>> GetSaleById(int id)
    {
        var sale = await _productService.GetSaleByIdAsync(id);
        if (sale == null)
        {
            return NotFound(new { message = "Venta no encontrada" });
        }
        return Ok(sale);
    }

    /// <summary>
    /// Obtiene un resumen de ventas.
    /// </summary>
    [HttpGet("sales/summary")]
    public async Task<ActionResult<SalesSummaryDtoOut>> GetSalesSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var summary = await _productService.GetSalesSummaryAsync(fromDate, toDate);
        return Ok(summary);
    }

    /// <summary>
    /// Cancela una venta y restaura el stock.
    /// </summary>
    [HttpPost("sales/{id}/cancel")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CancelSale(int id)
    {
        try
        {
            var cancelled = await _productService.CancelSaleAsync(id);
            if (!cancelled)
            {
                return NotFound(new { message = "Venta no encontrada" });
            }
            return Ok(new { message = "Venta cancelada correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Helpers

    private int? GetEmployeeId()
    {
        var idClaim = User.FindFirst("id")?.Value;
        if (int.TryParse(idClaim, out int employeeId))
        {
            return employeeId;
        }
        return null;
    }

    #endregion
}

/// <summary>
/// DTO para registrar merma.
/// </summary>
public class WasteDtoIn
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Notes { get; set; } = string.Empty;
}
