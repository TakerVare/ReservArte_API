using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

#region Service DTOs

/// <summary>
/// DTO de entrada para crear o actualizar un servicio.
/// 
/// Finalidad: Recibir los datos necesarios desde el cliente (frontend/app) para crear
/// un nuevo servicio o modificar uno existente en el catálogo de la organización.
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service y PUT /api/Service/{id}
/// para recibir la información del servicio como diseño de cejas, tinte, etc.
/// Incluye validaciones de datos para garantizar la integridad de la información.
/// </summary>
public class ServiceDtoIn
{
    [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Range(5, 480, ErrorMessage = "La duración debe estar entre 5 y 480 minutos")]
    public int DurationMinutes { get; set; }

    [Range(0, 10000, ErrorMessage = "El precio base debe estar entre 0 y 10000")]
    public decimal BasePrice { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequiresAllergyTest { get; set; } = false;

    [Range(0, 168, ErrorMessage = "Las horas de prueba de alergia deben estar entre 0 y 168")]
    public int AllergyTestHoursBefore { get; set; } = 48;
}

/// <summary>
/// DTO de salida con información completa de un servicio.
/// 
/// Finalidad: Devolver al cliente todos los detalles de un servicio específico,
/// incluyendo sus variaciones, precios por nivel de empleado y productos utilizados.
/// 
/// Uso: Se utiliza en el endpoint GET /api/Service/{id} para mostrar la ficha
/// completa de un servicio. Ideal para pantallas de detalle o edición donde
/// se necesita toda la información relacionada con el servicio.
/// </summary>
public class ServiceDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresAllergyTest { get; set; }
    public int AllergyTestHoursBefore { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public List<ServiceVariationDtoOut>? Variations { get; set; }
    public List<ServicePricingDtoOut>? Pricings { get; set; }
    public List<ServiceProductDtoOut>? Products { get; set; }
}

/// <summary>
/// DTO de salida simplificado para listados de servicios.
/// 
/// Finalidad: Proporcionar una versión ligera de los datos del servicio,
/// optimizada para mostrar en listados, catálogos o búsquedas.
/// 
/// Uso: Se utiliza en el endpoint GET /api/Service para obtener el catálogo
/// de servicios. Contiene solo la información esencial para mostrar en
/// tarjetas o filas de una tabla, mejorando el rendimiento de la consulta.
/// </summary>
public class ServiceListDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresAllergyTest { get; set; }
}

#endregion

#region ServiceCategory DTOs

/// <summary>
/// DTO de entrada para crear o actualizar una categoría de servicios.
/// 
/// Finalidad: Recibir los datos necesarios para organizar los servicios en categorías
/// (por ejemplo: "Cejas", "Pestañas", "Facial", etc.).
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service/categories y 
/// PUT /api/Service/categories/{id}. Permite definir el nombre, color para
/// identificación visual y orden de visualización en el catálogo.
/// </summary>
public class ServiceCategoryDtoIn
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
    public string? Description { get; set; }

    [StringLength(7, ErrorMessage = "El color debe ser un código hexadecimal válido")]
    public string? Color { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO de salida con información de una categoría de servicios.
/// 
/// Finalidad: Devolver los datos de una categoría para mostrar en el frontend,
/// incluyendo su color de identificación y orden de visualización.
/// 
/// Uso: Se utiliza en los endpoints GET /api/Service/categories y
/// GET /api/Service/categories/{id} para poblar menús de selección,
/// filtros de búsqueda o para organizar visualmente el catálogo de servicios.
/// </summary>
public class ServiceCategoryDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

#endregion

#region ServiceVariation DTOs

/// <summary>
/// DTO de entrada para crear o actualizar una variación de servicio.
/// 
/// Finalidad: Permitir definir variantes de un mismo servicio que modifican
/// el precio y/o duración base. Por ejemplo: tamaño (pequeño/mediano/grande)
/// o técnica utilizada (manual/con máquina).
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service/{serviceId}/variations y
/// PUT /api/Service/variations/{id}. Los modificadores pueden ser positivos
/// (incrementan) o negativos (reducen) el precio y duración base del servicio.
/// </summary>
public class ServiceVariationDtoIn
{
    [Required(ErrorMessage = "El ID del servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "El nombre de la variación es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Range(-1000, 1000, ErrorMessage = "El modificador de precio debe estar entre -1000 y 1000")]
    public decimal PriceModifier { get; set; } = 0;

    [Range(-120, 120, ErrorMessage = "El modificador de duración debe estar entre -120 y 120 minutos")]
    public int DurationModifier { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO de salida con información de una variación de servicio.
/// 
/// Finalidad: Devolver los datos de una variación para mostrar las opciones
/// disponibles al cliente cuando reserva un servicio.
/// 
/// Uso: Se incluye dentro de ServiceDtoOut y se usa en 
/// GET /api/Service/{serviceId}/variations. Permite al frontend calcular
/// el precio y duración final sumando los modificadores al precio base.
/// </summary>
public class ServiceVariationDtoOut
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
    public int DurationModifier { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

#endregion

#region ServicePricing DTOs

/// <summary>
/// DTO de entrada para establecer el precio de un servicio según nivel de empleado.
/// 
/// Finalidad: Permitir definir precios diferenciados para un mismo servicio
/// dependiendo del nivel de experiencia del empleado que lo realiza
/// (Junior, Senior o Expert).
/// 
/// Uso: Se utiliza en el endpoint PUT /api/Service/{serviceId}/pricing.
/// Si ya existe un precio para ese nivel, se actualiza; si no existe, se crea.
/// Esto permite cobrar más por servicios realizados por empleados más experimentados.
/// </summary>
public class ServicePricingDtoIn
{
    [Required(ErrorMessage = "El ID del servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "El nivel de empleado es obligatorio")]
    public string EmployeeLevel { get; set; } = EmployeeLevels.Junior;

    [Range(0, 10000, ErrorMessage = "El precio debe estar entre 0 y 10000")]
    public decimal Price { get; set; }
}

/// <summary>
/// DTO de salida con información del precio por nivel de empleado.
/// 
/// Finalidad: Devolver los precios configurados para cada nivel de empleado
/// de un servicio específico.
/// 
/// Uso: Se incluye dentro de ServiceDtoOut y se usa en 
/// GET /api/Service/{serviceId}/pricing. Permite al sistema de citas
/// calcular el precio correcto según el empleado asignado.
/// </summary>
public class ServicePricingDtoOut
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string EmployeeLevel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

#endregion

#region Product DTOs

/// <summary>
/// DTO de entrada para crear o actualizar un producto.
/// 
/// Finalidad: Registrar los productos (cosméticos, tintes, herramientas, etc.)
/// que se utilizan en la realización de los servicios.
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service/products y
/// PUT /api/Service/products/{id}. Permite mantener un inventario de productos
/// que luego pueden asociarse a servicios específicos para control de stock
/// y trazabilidad.
/// </summary>
public class ProductDtoIn
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede exceder 300 caracteres")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "La marca no puede exceder 100 caracteres")]
    public string? Brand { get; set; }

    [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres")]
    public string? Sku { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO de salida con información de un producto.
/// 
/// Finalidad: Devolver los datos de un producto para mostrar en listados,
/// fichas de servicio o para gestión de inventario.
/// 
/// Uso: Se utiliza en los endpoints GET /api/Service/products y
/// GET /api/Service/products/{id}. Permite ver qué productos están
/// disponibles para asociar a servicios.
/// </summary>
public class ProductDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Sku { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

#endregion

#region ServiceProduct DTOs

/// <summary>
/// DTO de entrada para vincular un producto a un servicio.
/// 
/// Finalidad: Establecer qué productos se utilizan en la realización de un
/// servicio específico, incluyendo la cantidad aproximada que se consume.
/// 
/// Uso: Se utiliza en el endpoint POST /api/Service/{serviceId}/products.
/// Permite documentar los requisitos materiales de cada servicio, útil para
/// informar al cliente sobre los productos utilizados o para verificar
/// posibles alergias.
/// </summary>
public class ServiceProductDtoIn
{
    [Required(ErrorMessage = "El ID del servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public int ProductId { get; set; }

    [Range(0, 1000, ErrorMessage = "La cantidad usada debe estar entre 0 y 1000")]
    public decimal? QuantityUsed { get; set; }

    [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO de salida con información de la relación servicio-producto.
/// 
/// Finalidad: Mostrar qué productos están asociados a un servicio,
/// incluyendo nombre del producto y cantidad utilizada.
/// 
/// Uso: Se incluye dentro de ServiceDtoOut y se usa en
/// GET /api/Service/{serviceId}/products. Permite mostrar al cliente
/// los productos que se usarán en su tratamiento.
/// </summary>
public class ServiceProductDtoOut
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductBrand { get; set; }
    public decimal? QuantityUsed { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region ServicePackage DTOs

/// <summary>
/// DTO de entrada para crear o actualizar un paquete de servicios.
/// 
/// Finalidad: Permitir agrupar múltiples servicios en un paquete o combo
/// con un precio especial y descuento aplicado.
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service/packages y
/// PUT /api/Service/packages/{id}. Ideal para crear ofertas como
/// "Pack Novia" (cejas + pestañas + maquillaje) o "Mantenimiento Mensual"
/// con descuento por contratar varios servicios juntos.
/// </summary>
public class ServicePackageDtoIn
{
    [Required(ErrorMessage = "El nombre del paquete es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Range(0, 50000, ErrorMessage = "El precio total debe estar entre 0 y 50000")]
    public decimal TotalPrice { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe estar entre 0 y 100")]
    public decimal DiscountPercentage { get; set; } = 0;

    [StringLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO de salida con información completa de un paquete de servicios.
/// 
/// Finalidad: Devolver todos los detalles de un paquete, incluyendo los
/// servicios que contiene, precio original vs precio con descuento, y
/// duración total estimada.
/// 
/// Uso: Se utiliza en el endpoint GET /api/Service/packages/{id} para
/// mostrar la ficha completa del paquete. Incluye el ahorro calculado
/// para destacar el beneficio económico al cliente.
/// </summary>
public class ServicePackageDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal Savings { get; set; }
    public int TotalDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
    public List<ServicePackageItemDtoOut>? Items { get; set; }
}

/// <summary>
/// DTO de salida simplificado para listados de paquetes.
/// 
/// Finalidad: Proporcionar una versión ligera de los datos del paquete,
/// optimizada para mostrar en catálogos o listados de ofertas.
/// 
/// Uso: Se utiliza en el endpoint GET /api/Service/packages para obtener
/// el listado de paquetes disponibles. Incluye el número de servicios
/// y duración total para dar una idea rápida del contenido.
/// </summary>
public class ServicePackageListDtoOut
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int ServiceCount { get; set; }
    public int TotalDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region ServicePackageItem DTOs

/// <summary>
/// DTO de entrada para añadir un servicio a un paquete.
/// 
/// Finalidad: Vincular un servicio existente a un paquete, especificando
/// el orden en que se realizarán los servicios (para servicios secuenciales).
/// 
/// Uso: Se utiliza en el endpoint POST /api/Service/packages/{packageId}/services.
/// El campo Order permite definir la secuencia cuando los servicios deben
/// realizarse en un orden específico (ej: primero limpieza, luego tratamiento).
/// </summary>
public class ServicePackageItemDtoIn
{
    [Required(ErrorMessage = "El ID del paquete es obligatorio")]
    public int ServicePackageId { get; set; }

    [Required(ErrorMessage = "El ID del servicio es obligatorio")]
    public int ServiceId { get; set; }

    [Range(0, 100, ErrorMessage = "El orden debe estar entre 0 y 100")]
    public int Order { get; set; } = 0;
}

/// <summary>
/// DTO de salida con información de un servicio dentro de un paquete.
/// 
/// Finalidad: Mostrar los detalles de cada servicio incluido en un paquete,
/// con su nombre, precio individual y duración.
/// 
/// Uso: Se incluye dentro de ServicePackageDtoOut para detallar el contenido
/// del paquete. Permite al cliente ver exactamente qué servicios recibirá
/// y calcular el ahorro respecto a contratarlos por separado.
/// </summary>
public class ServicePackageItemDtoOut
{
    public int Id { get; set; }
    public int ServicePackageId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public int ServiceDurationMinutes { get; set; }
    public int Order { get; set; }
}

#endregion

#region ServicePromotion DTOs

/// <summary>
/// DTO de entrada para crear o actualizar una promoción de servicio.
/// 
/// Finalidad: Definir promociones temporales con descuentos para servicios
/// o paquetes específicos, con fechas de inicio y fin.
/// 
/// Uso: Se utiliza en los endpoints POST /api/Service/promotions y
/// PUT /api/Service/promotions/{id}. Permite crear ofertas de temporada
/// (ej: "Especial San Valentín") o promociones limitadas. El campo
/// IsSeasonalService indica si es un servicio disponible solo en temporada.
/// </summary>
public class ServicePromotionDtoIn
{
    public int? ServiceId { get; set; }

    public int? ServicePackageId { get; set; }

    [Required(ErrorMessage = "El nombre de la promoción es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Range(0, 100, ErrorMessage = "El porcentaje de descuento debe estar entre 0 y 100")]
    public decimal DiscountPercentage { get; set; } = 0;

    [Range(0, 10000, ErrorMessage = "El monto de descuento debe estar entre 0 y 10000")]
    public decimal? DiscountAmount { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    public DateTime EndDate { get; set; }

    public bool IsSeasonalService { get; set; } = false;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO de salida con información de una promoción de servicio.
/// 
/// Finalidad: Devolver los detalles de una promoción, incluyendo el servicio
/// o paquete al que aplica y si está actualmente vigente.
/// 
/// Uso: Se utiliza en los endpoints GET /api/Service/promotions,
/// GET /api/Service/promotions/{id} y GET /api/Service/{serviceId}/promotions.
/// El campo IsCurrentlyActive indica si la promoción está vigente según
/// la fecha actual, útil para destacar ofertas activas en el frontend.
/// </summary>
public class ServicePromotionDtoOut
{
    public int Id { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? ServicePackageId { get; set; }
    public string? PackageName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public bool IsSeasonalService { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

#endregion
