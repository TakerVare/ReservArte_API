using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para bloquear un cliente.
/// 
/// FINALIDAD:
/// Este DTO permite bloquear a un cliente impidiéndole realizar nuevas reservas.
/// El bloqueo se utiliza típicamente para clientes con no-shows reiterados,
/// comportamiento inadecuado, o incumplimiento de políticas del negocio.
/// 
/// USO:
/// - POST /api/customer/{id}/block → Bloquear cliente
/// 
/// VALIDACIONES:
/// - Reason es obligatorio (5-500 caracteres)
/// - El motivo debe ser descriptivo para futuras referencias
/// 
/// EFECTOS DEL BLOQUEO:
/// - El cliente no podrá realizar nuevas reservas
/// - La categoría del cliente cambia automáticamente a "Blocked"
/// - Las reservas existentes NO se cancelan automáticamente
/// - El historial del cliente permanece accesible para consulta
/// 
/// PERMISOS:
/// - Solo administradores pueden bloquear clientes
/// 
/// PARA DESBLOQUEAR:
/// - Usar POST /api/customer/{id}/unblock (no requiere motivo)
/// </summary>
public class CustomerBlockDtoIn
{
    [Required(ErrorMessage = "El motivo del bloqueo es obligatorio")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres")]
    public string Reason { get; set; } = string.Empty;
}
