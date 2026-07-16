using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
