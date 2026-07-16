using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "User";

    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
