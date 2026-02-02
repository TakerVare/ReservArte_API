using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de entrada para crear una nota interna sobre un cliente.
/// 
/// FINALIDAD:
/// Este DTO permite al personal del negocio añadir notas privadas sobre un cliente.
/// Las notas son visibles solo para empleados y administradores, nunca para el cliente.
/// Se pueden usar para registrar preferencias, observaciones, incidencias, etc.
/// 
/// USO:
/// - POST /api/customer/{id}/notes → Crear nueva nota para un cliente
/// 
/// VALIDACIONES:
/// - Note es obligatorio (1-2000 caracteres)
/// 
/// NOTA:
/// El EmployeeId se obtiene automáticamente del token JWT del usuario autenticado,
/// no es necesario enviarlo en el cuerpo de la petición.
/// </summary>
public class CustomerNoteDtoIn
{
    [Required(ErrorMessage = "La nota es obligatoria")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "La nota debe tener entre 1 y 2000 caracteres")]
    public string Note { get; set; } = string.Empty;
}
