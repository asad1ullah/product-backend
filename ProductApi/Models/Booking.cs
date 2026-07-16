using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models;

public class Booking
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    [Range(1, 100)]
    public int NumberOfSeats { get; set; } = 1;

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
