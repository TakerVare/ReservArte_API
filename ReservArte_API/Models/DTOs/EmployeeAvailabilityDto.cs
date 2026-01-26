
namespace ReservArte_API.Models.DTOs;
public class EmployeeAvailabilityDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring  { get; set; }  // Indicates if the availability is recurring weekly
    
}