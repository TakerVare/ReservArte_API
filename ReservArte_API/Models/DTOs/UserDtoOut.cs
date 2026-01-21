using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReservArte_API.Models.DTOs;

public class UserDtoOut
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
}

