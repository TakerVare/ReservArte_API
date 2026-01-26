using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;
public class AppointmentDtoNoShowPenalization
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public string AppointmentDate { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
}

