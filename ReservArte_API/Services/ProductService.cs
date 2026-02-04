using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;
using ReservArte_API.Services.Interfaces;

namespace ReservArte_API.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IProductSaleRepository _saleRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IInventoryMovementRepository movementRepository,
        IProductSaleRepository saleRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _movementRepository = movementRepository;
        _saleRepository = saleRepository;
        _logger = logger;
    }

    #region CRUD Productos

    public async Task<CatalogProductDtoOut?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDetailDto(product) : null;
    }

    public async Task<IEnumerable<CatalogProductListDtoOut>> GetAllAsync(bool includeInactive = false)
    {
        var products = await _productRepository.GetAllAsync(includeInactive);
        return products.Select(MapToListDto);
    }

    public async Task<IEnumerable<CatalogProductListDtoOut>> GetByCategoryAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return products.Select(MapToListDto);
    }

    public async Task<IEnumerable<CatalogProductListDtoOut>> SearchAsync(string searchTerm)
    {
        var products = await _productRepository.SearchAsync(searchTerm);
        return products.Select(MapToListDto);
    }

    public async Task<CatalogProductDtoOut> CreateAsync(CatalogProductDtoIn dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Brand = dto.Brand,
            Sku = dto.Sku,
            Price = dto.Price,
            Stock = dto.Stock,
            MinStockAlert = dto.MinStockAlert,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _productRepository.CreateAsync(product);
        
        // Si hay stock inicial, registrar movimiento
        if (dto.Stock > 0)
        {
            await _movementRepository.CreateAsync(new InventoryMovement
            {
                ProductId = created.Id,
                Quantity = dto.Stock,
                MovementType = MovementType.Adjustment,
                Notes = "Stock inicial",
                CreatedAt = DateTime.UtcNow
            });
        }

        return MapToDetailDto(created);
    }

    public async Task<CatalogProductDtoOut?> UpdateAsync(int id, CatalogProductDtoIn dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Brand = dto.Brand;
        existing.Sku = dto.Sku;
        existing.Price = dto.Price;
        existing.MinStockAlert = dto.MinStockAlert;
        existing.ImageUrl = dto.ImageUrl;
        existing.CategoryId = dto.CategoryId;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        var success = await _productRepository.UpdateAsync(existing);
        return success ? MapToDetailDto(existing) : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }

    #endregion

    #region Categorías

    public async Task<IEnumerable<ProductCategoryDtoOut>> GetCategoriesAsync(bool includeInactive = false)
    {
        var categories = await _productRepository.GetCategoriesAsync(includeInactive);
        var result = new List<ProductCategoryDtoOut>();

        foreach (var cat in categories)
        {
            var count = await _productRepository.GetCategoryProductCountAsync(cat.Id);
            result.Add(new ProductCategoryDtoOut
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description,
                IsActive = cat.IsActive,
                ProductCount = count
            });
        }

        return result;
    }

    public async Task<ProductCategoryDtoOut?> GetCategoryByIdAsync(int id)
    {
        var category = await _productRepository.GetCategoryByIdAsync(id);
        if (category == null) return null;

        var count = await _productRepository.GetCategoryProductCountAsync(id);
        return new ProductCategoryDtoOut
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = count
        };
    }

    public async Task<ProductCategoryDtoOut> CreateCategoryAsync(ProductCategoryDtoIn dto)
    {
        var category = new ProductCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _productRepository.CreateCategoryAsync(category);

        return new ProductCategoryDtoOut
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            IsActive = created.IsActive,
            ProductCount = 0
        };
    }

    public async Task<ProductCategoryDtoOut?> UpdateCategoryAsync(int id, ProductCategoryDtoIn dto)
    {
        var existing = await _productRepository.GetCategoryByIdAsync(id);
        if (existing == null) return null;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.IsActive = dto.IsActive;

        var success = await _productRepository.UpdateCategoryAsync(existing);
        if (!success) return null;

        var count = await _productRepository.GetCategoryProductCountAsync(id);
        return new ProductCategoryDtoOut
        {
            Id = existing.Id,
            Name = existing.Name,
            Description = existing.Description,
            IsActive = existing.IsActive,
            ProductCount = count
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _productRepository.DeleteCategoryAsync(id);
    }

    #endregion

    #region Inventario

    public async Task<IEnumerable<InventoryMovementDtoOut>> GetMovementsAsync(InventoryMovementFilterDto filter)
    {
        var movements = await _movementRepository.GetAllAsync(
            filter.ProductId,
            filter.MovementType,
            filter.FromDate,
            filter.ToDate);

        return movements.Select(m => new InventoryMovementDtoOut
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = m.ProductName ?? string.Empty,
            Quantity = m.Quantity,
            MovementType = m.MovementType,
            ReferenceId = m.ReferenceId,
            Notes = m.Notes,
            CreatedByName = m.CreatedByName,
            CreatedAt = m.CreatedAt
        });
    }

    public async Task<InventoryMovementDtoOut> RegisterPurchaseAsync(PurchaseDtoIn dto, int employeeId)
    {
        // Validar producto existe
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Producto con ID {dto.ProductId} no encontrado");
        }

        // Crear movimiento
        var movement = new InventoryMovement
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity, // Positivo para compras
            MovementType = MovementType.Purchase,
            Notes = dto.Notes ?? $"Compra - Coste unitario: {dto.UnitCost?.ToString("C") ?? "N/A"}",
            CreatedBy = employeeId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _movementRepository.CreateAsync(movement);

        // Actualizar stock
        await _productRepository.UpdateStockAsync(dto.ProductId, dto.Quantity);

        _logger.LogInformation("Compra registrada: {Quantity} unidades de {Product}", dto.Quantity, product.Name);

        return new InventoryMovementDtoOut
        {
            Id = created.Id,
            ProductId = created.ProductId,
            ProductName = product.Name,
            Quantity = created.Quantity,
            MovementType = created.MovementType,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<InventoryMovementDtoOut> AdjustStockAsync(StockAdjustmentDtoIn dto, int employeeId)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Producto con ID {dto.ProductId} no encontrado");
        }

        // Validar que no quede negativo
        if (product.Stock + dto.Quantity < 0)
        {
            throw new InvalidOperationException($"El ajuste dejaría el stock en negativo. Stock actual: {product.Stock}");
        }

        var movement = new InventoryMovement
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            MovementType = MovementType.Adjustment,
            Notes = dto.Notes,
            CreatedBy = employeeId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _movementRepository.CreateAsync(movement);
        await _productRepository.UpdateStockAsync(dto.ProductId, dto.Quantity);

        _logger.LogInformation("Ajuste de stock: {Quantity} unidades de {Product}. Motivo: {Notes}", 
            dto.Quantity, product.Name, dto.Notes);

        return new InventoryMovementDtoOut
        {
            Id = created.Id,
            ProductId = created.ProductId,
            ProductName = product.Name,
            Quantity = created.Quantity,
            MovementType = created.MovementType,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<InventoryMovementDtoOut> RegisterWasteAsync(int productId, int quantity, string notes, int employeeId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new KeyNotFoundException($"Producto con ID {productId} no encontrado");
        }

        if (quantity > product.Stock)
        {
            throw new InvalidOperationException($"No hay suficiente stock. Stock actual: {product.Stock}");
        }

        var movement = new InventoryMovement
        {
            ProductId = productId,
            Quantity = -quantity, // Negativo para merma
            MovementType = MovementType.Waste,
            Notes = notes,
            CreatedBy = employeeId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _movementRepository.CreateAsync(movement);
        await _productRepository.UpdateStockAsync(productId, -quantity);

        _logger.LogWarning("Merma registrada: {Quantity} unidades de {Product}. Motivo: {Notes}", 
            quantity, product.Name, notes);

        return new InventoryMovementDtoOut
        {
            Id = created.Id,
            ProductId = created.ProductId,
            ProductName = product.Name,
            Quantity = created.Quantity,
            MovementType = created.MovementType,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<IEnumerable<StockAlertDtoOut>> GetLowStockAlertsAsync()
    {
        var products = await _productRepository.GetLowStockAsync();
        return products.Select(p => new StockAlertDtoOut
        {
            ProductId = p.Id,
            ProductName = p.Name,
            Sku = p.Sku,
            CurrentStock = p.Stock,
            MinStockAlert = p.MinStockAlert
        });
    }

    #endregion

    #region Ventas

    public async Task<ProductSaleDtoOut> CreateSaleAsync(ProductSaleDtoIn dto, int employeeId)
    {
        // Validar productos y stock
        var saleItems = new List<(Product product, int quantity, decimal unitPrice)>();
        
        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Producto con ID {item.ProductId} no encontrado");
            }
            if (!product.IsActive)
            {
                throw new InvalidOperationException($"Producto '{product.Name}' no está activo");
            }
            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente de '{product.Name}'. Disponible: {product.Stock}");
            }
            
            var unitPrice = item.UnitPrice ?? product.Price;
            saleItems.Add((product, item.Quantity, unitPrice));
        }

        // Calcular total
        var totalAmount = saleItems.Sum(x => x.quantity * x.unitPrice);

        // Crear venta
        var sale = new ProductSale
        {
            CustomerId = dto.CustomerId,
            AppointmentId = dto.AppointmentId,
            TotalAmount = totalAmount,
            Status = SaleStatus.Completed,
            PaymentMethod = dto.PaymentMethod,
            Notes = dto.Notes,
            SoldBy = employeeId,
            CreatedAt = DateTime.UtcNow
        };

        var createdSale = await _saleRepository.CreateAsync(sale);

        // Añadir items y actualizar stock
        var itemDtos = new List<ProductSaleItemDtoOut>();
        
        foreach (var (product, quantity, unitPrice) in saleItems)
        {
            var saleItem = new ProductSaleItem
            {
                SaleId = createdSale.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = quantity * unitPrice
            };

            await _saleRepository.AddSaleItemAsync(saleItem);

            // Registrar movimiento de inventario
            await _movementRepository.CreateAsync(new InventoryMovement
            {
                ProductId = product.Id,
                Quantity = -quantity, // Negativo para venta
                MovementType = MovementType.Sale,
                ReferenceId = createdSale.Id,
                Notes = $"Venta #{createdSale.Id}",
                CreatedBy = employeeId,
                CreatedAt = DateTime.UtcNow
            });

            // Actualizar stock
            await _productRepository.UpdateStockAsync(product.Id, -quantity);

            itemDtos.Add(new ProductSaleItemDtoOut
            {
                Id = saleItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = saleItem.Subtotal
            });
        }

        _logger.LogInformation("Venta creada: #{SaleId}, Total: {Total}", createdSale.Id, totalAmount);

        return new ProductSaleDtoOut
        {
            Id = createdSale.Id,
            CustomerId = createdSale.CustomerId,
            AppointmentId = createdSale.AppointmentId,
            TotalAmount = createdSale.TotalAmount,
            Status = createdSale.Status,
            PaymentMethod = createdSale.PaymentMethod,
            Notes = createdSale.Notes,
            SoldBy = createdSale.SoldBy,
            CreatedAt = createdSale.CreatedAt,
            Items = itemDtos
        };
    }

    public async Task<IEnumerable<ProductSaleDtoOut>> GetSalesAsync(ProductSaleFilterDto filter)
    {
        var sales = await _saleRepository.GetAllAsync(
            filter.CustomerId,
            filter.AppointmentId,
            filter.Status,
            filter.FromDate,
            filter.ToDate);

        var result = new List<ProductSaleDtoOut>();
        foreach (var sale in sales)
        {
            var items = await _saleRepository.GetSaleItemsAsync(sale.Id);
            result.Add(new ProductSaleDtoOut
            {
                Id = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.CustomerName,
                AppointmentId = sale.AppointmentId,
                TotalAmount = sale.TotalAmount,
                Status = sale.Status,
                PaymentMethod = sale.PaymentMethod,
                Notes = sale.Notes,
                SoldBy = sale.SoldBy,
                SoldByName = sale.SoldByName,
                CreatedAt = sale.CreatedAt,
                Items = items.Select(i => new ProductSaleItemDtoOut
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            });
        }

        return result;
    }

    public async Task<ProductSaleDtoOut?> GetSaleByIdAsync(int id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        if (sale == null) return null;

        return new ProductSaleDtoOut
        {
            Id = sale.Id,
            CustomerId = sale.CustomerId,
            CustomerName = sale.CustomerName,
            AppointmentId = sale.AppointmentId,
            TotalAmount = sale.TotalAmount,
            Status = sale.Status,
            PaymentMethod = sale.PaymentMethod,
            Notes = sale.Notes,
            SoldBy = sale.SoldBy,
            SoldByName = sale.SoldByName,
            CreatedAt = sale.CreatedAt,
            Items = sale.Items?.Select(i => new ProductSaleItemDtoOut
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList() ?? new()
        };
    }

    public async Task<SalesSummaryDtoOut> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate)
    {
        var (totalSales, totalAmount, itemsSold) = await _saleRepository.GetSalesSummaryAsync(fromDate, toDate);

        return new SalesSummaryDtoOut
        {
            TotalSales = totalSales,
            TotalAmount = totalAmount,
            TotalItemsSold = itemsSold,
            FromDate = fromDate,
            ToDate = toDate
        };
    }

    public async Task<bool> CancelSaleAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null) return false;

        if (sale.Status != SaleStatus.Completed)
        {
            throw new InvalidOperationException("Solo se pueden cancelar ventas completadas");
        }

        // Restaurar stock
        foreach (var item in sale.Items ?? new())
        {
            await _productRepository.UpdateStockAsync(item.ProductId, item.Quantity);

            // Registrar movimiento de devolución
            await _movementRepository.CreateAsync(new InventoryMovement
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity, // Positivo para devolución
                MovementType = MovementType.Return,
                ReferenceId = saleId,
                Notes = $"Cancelación de venta #{saleId}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _saleRepository.UpdateStatusAsync(saleId, SaleStatus.Cancelled);

        _logger.LogInformation("Venta cancelada: #{SaleId}", saleId);

        return true;
    }

    #endregion

    #region Helpers

    private static CatalogProductDtoOut MapToDetailDto(Product product)
    {
        return new CatalogProductDtoOut
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Sku = product.Sku,
            Price = product.Price,
            Stock = product.Stock,
            MinStockAlert = product.MinStockAlert,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = product.CategoryName,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private static CatalogProductListDtoOut MapToListDto(Product product)
    {
        return new CatalogProductListDtoOut
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            Sku = product.Sku,
            Price = product.Price,
            Stock = product.Stock,
            CategoryName = product.CategoryName,
            IsActive = product.IsActive,
            IsLowStock = product.Stock <= product.MinStockAlert
        };
    }

    #endregion
}
