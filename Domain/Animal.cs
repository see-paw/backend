using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain;

/// <summary>
/// Represents an animal entity within the SeePaw system.
/// </summary>
/// <remarks>
/// Contains identification, biological, descriptive, and relational data 
/// about animals available for adoption or fostering.  
/// This class is mapped to the database and used throughout the application 
/// for persistence and API data exchange.
/// </remarks>
public class Animal
{
    [Key]
    public string AnimalId { get; set; } = Guid.NewGuid().ToString();

    [Required, StringLength(40, MinimumLength = 2)]
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

    [Required, StringLength(40)]
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
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required, Url, StringLength(500)]
    public required string MainImageUrl { get; set; }

    //Foreign Key
    [Required]
    public required string ShelterId { get; set; }

    [JsonIgnore]
    public Shelter? Shelter { get; set; }

    [JsonIgnore]
    public ICollection<Photo>? Images { get; set; }
}
