using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Animal
{
    [Key]
    public string AnimalId { get; set; } = Guid.NewGuid().ToString();

    [Required, StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Required]
    public required AnimalState AnimalState { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public required Species Species { get; set; }

    [Required]
    public required SizeType Size { get; set; }

    [Required]
    public required SexType Sex { get; set; }

    [Required, StringLength(50)]
    public required string Colour { get; set; }

    [Required]
    public required DateOnly BirthDate { get; set; }

    [Required]
    public required bool Sterilized { get; set; }

    [Required]
    public required Breed Breed { get; set; }

    [Required, Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000.")]
    public required decimal Cost { get; set; }

    [StringLength(300)]
    public required string? Features { get; set; }

    [Required]
    public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public  DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required, Url, StringLength(500)]
    public required string MainImageUrl { get; set; }
}
