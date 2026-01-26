
namespace ReservArte_API.Models.DTOs;
public class EmployeeExceptionDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime StartDateTime  { get; set; } // Specific date and time when the exception starts
    public DateTime EndDateTime  { get; set; }   // Specific date and time when the exception ends
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Reason { get; set; }

    
}