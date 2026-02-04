/*
 * CustomerPaymentMethodDtoOut.cs
 * ==============================
 * 
 * PROPÓSITO:
 * DTO de salida para métodos de pago guardados del cliente (tarjetas tokenizadas).
 * 
 * DÓNDE SE USA:
 * - PaymentController.cs: GET /api/payment/methods/{customerId}
 * - CustomerPaymentMethodRepository.cs: GetByCustomerIdAsync()
 * 
 * PARA QUÉ SE USA:
 * - Listar tarjetas guardadas de un cliente
 * - Mostrar información segura de tarjetas (últimos 4 dígitos, marca, expiración)
 * - Seleccionar tarjeta guardada para realizar pagos
 * - No expone el token real de Redsys por seguridad
 */

namespace ReservArte_API.Models.DTOs;

public class CustomerPaymentMethodDtoOut
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Últimos 4 dígitos de la tarjeta
    /// </summary>
    public string CardLast4 { get; set; } = string.Empty;
    
    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, etc.)
    /// </summary>
    public string CardBrand { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de expiración formateada (MM/AA)
    /// </summary>
    public string CardExpiry { get; set; } = string.Empty;
    
    /// <summary>
    /// Si es el método de pago por defecto
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Si la tarjeta está caducada
    /// </summary>
    public bool IsExpired { get; set; }
    
    /// <summary>
    /// Descripción para mostrar en UI (ej: "Visa •••• 4242")
    /// </summary>
    public string DisplayName => $"{CardBrand} •••• {CardLast4}";
    
    public DateTime CreatedAt { get; set; }
}
