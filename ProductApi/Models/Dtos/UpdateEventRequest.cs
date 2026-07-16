using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProductApi.Models.Dtos;

public class UpdateEventRequest : IValidatableObject
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int OrganizerId { get; set; }

    public IFormFile? Image { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult("EndDate must be later than StartDate.", new[] { nameof(EndDate) });
        }
    }
}
