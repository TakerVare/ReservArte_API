namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información de un método de pago (tarjeta enmascarada).
/// 
/// FINALIDAD:
/// Este DTO devuelve información segura sobre las tarjetas guardadas del cliente.
/// Solo muestra datos parciales que permiten identificar la tarjeta sin exponer
/// información sensible.
/// 
/// USO:
/// - GET /api/customer/{id}/payment-methods → Listar tarjetas guardadas
/// - POST /api/customer/{id}/payment-methods → Respuesta tras guardar tarjeta
/// 
/// CAMPOS MOSTRADOS:
/// - CardLast4: Últimos 4 dígitos (ej: "4532")
/// - CardBrand: Marca de la tarjeta (ej: "Visa", "Mastercard")
/// - CardExpiry: Fecha de expiración formateada como MM/AA
/// - IsDefault: Indica si es la tarjeta predeterminada para cargos
/// - IsExpired: Indica si la tarjeta ya ha caducado
/// 
/// SEGURIDAD:
/// - NO se devuelve el token de Redsys ni datos sensibles
/// - El cliente puede identificar sus tarjetas por los últimos 4 dígitos
/// </summary>
public class CustomerPaymentMethodDtoOut
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CardLast4 { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardExpiry { get; set; } = string.Empty; // Formato MM/AA
    public bool IsDefault { get; set; }
    public bool IsExpired { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
