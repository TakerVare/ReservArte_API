using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para gestionar puntos del programa de fidelización.
/// 
/// FINALIDAD:
/// Este DTO permite añadir o canjear puntos de fidelización de un cliente.
/// Los puntos se pueden otorgar por servicios realizados, promociones especiales,
/// o como compensación. También se pueden canjear por descuentos o servicios.
/// 
/// USO:
/// - POST /api/customer/{id}/loyalty/add → Añadir puntos al cliente
/// - POST /api/customer/{id}/loyalty/redeem → Canjear puntos del cliente
/// 
/// VALIDACIONES:
/// - Points es obligatorio y debe ser mayor que 0
/// - Reason es opcional, permite documentar el motivo de la operación
/// 
/// LÓGICA DE NEGOCIO:
/// - Al añadir puntos, se verifica si el cliente debe ser promovido a VIP
/// - Al canjear, se verifica que el cliente tenga suficientes puntos
/// - Los criterios VIP son: ≥10 citas completadas O ≥1000 puntos
/// 
/// EJEMPLO:
/// { "points": 50, "reason": "Servicio de coloración completa" }
/// </summary>
public class CustomerLoyaltyDtoIn
{
    [Required(ErrorMessage = "La cantidad de puntos es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Los puntos deben ser mayor que 0")]
    public int Points { get; set; }

    public string? Reason { get; set; }
}
