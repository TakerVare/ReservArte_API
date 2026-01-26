using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;
public class AppointmentDtoToList
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public string AppointmentDate { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        [Required]
        public string Status { get; set; }
        
}

