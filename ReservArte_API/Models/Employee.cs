
namespace ReservArte_API.Models;

public class Employee : User
{
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}   