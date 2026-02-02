namespace ReservArte_API.Models.DTOs;

/// <summary>
/// DTO de salida con información resumida de un cliente para listados.
/// 
/// FINALIDAD:
/// Este DTO se utiliza para devolver un resumen de los clientes cuando se
/// solicita un listado. Contiene solo los campos esenciales para mostrar
/// en una tabla o lista, optimizando el rendimiento y la transferencia de datos.
/// 
/// USO:
/// - GET /api/customer → Obtener listado de todos los clientes
/// - GET /api/customer?organizationId={id} → Filtrar por organización
/// 
/// CAMPOS INCLUIDOS:
/// - Identificación básica (Id, FullName, Email, Phone)
/// - Estado del cliente (Category, IsBlocked)
/// - Programa de fidelización (LoyaltyPoints)
/// - Fecha de registro (CreatedAt)
/// </summary>
public class CustomerListDtoOut
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Category { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
    public bool IsBlocked { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
