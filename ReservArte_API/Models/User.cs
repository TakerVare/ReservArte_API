
namespace ReservArte_API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public string Rol { get; set; }
    public string Phone  { get; set; }
    public string? ProfileImageUrl  { get; set; }


}   