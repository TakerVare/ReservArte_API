namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información de una nota interna de cliente.
/// 
/// FINALIDAD:
/// Este DTO se utiliza para devolver las notas internas asociadas a un cliente.
/// Incluye información sobre quién creó la nota y cuándo, permitiendo un
/// seguimiento del historial de observaciones del personal.
/// 
/// USO:
/// - GET /api/customer/{id}/notes → Obtener todas las notas de un cliente
/// - POST /api/customer/{id}/notes → Respuesta tras crear una nota
/// 
/// CAMPOS INCLUIDOS:
/// - Identificación de la nota (Id, CustomerId)
/// - Autor de la nota (EmployeeId, EmployeeName)
/// - Contenido y fecha (Note, CreatedAt)
/// </summary>
public class CustomerNoteDtoOut
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string Note { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
