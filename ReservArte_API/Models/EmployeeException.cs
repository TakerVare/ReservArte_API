namespace ReservArte_API.Models;

public class EmployeeException
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Reason { get; set; }
    public string Type { get; set; } = EmployeeExceptionType.Other; // Vacation, Sick, Other, etc.
}
