
namespace ReservArte_API.Models;

public class Appointment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }

    public DateOnly AppointmentDate  { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }   
    public string Status { get; set; }
    public decimal TotalPrice { get; set; }


}   