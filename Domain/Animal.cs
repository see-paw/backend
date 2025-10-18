using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using System.Text.Json.Serialization;


namespace Domain;

/// <summary>
/// Represents an animal within the system, including its biological attributes,
/// adoption details, shelter association, and ownership information.
/// </summary>

public class Animal
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AnimalState AnimalState { get; set; } = AnimalState.Available;

    [StringLength(250)]
    public string? Description { get; set; }

    [Required]
    public Species Species { get; set; }

    [Required]
    public SizeType Size { get; set; }

    [Required]
    public SexType Sex { get; set; }

    [Required]
    [StringLength(50)]
    public string Colour { get; set; } = string.Empty;

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    public bool Sterilized { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000.")]
    public decimal Cost { get; set; }

    [StringLength(300)]
    public string? Features { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Images - At least one required, with one marked as IsPrincipal in the Image entity
    [JsonIgnore]
    [MinLength(1, ErrorMessage = "Animal must have at least one image.")]
    public ICollection<Image> Images { get; set; } = new List<Image>();

    // Foreign Key - Shelter (required)
    [Required]
    public string ShelterId { get; set; } = string.Empty;

    [JsonIgnore]
    public Shelter Shelter { get; set; } = null!;

    // Foreign Key - Breed (required)
    [Required]
    public string BreedId { get; set; } = string.Empty;

    [JsonIgnore]
    public Breed Breed { get; set; } = null!;
    public string? OwnerId { get; set; }

    [JsonIgnore]
    public User? Owner { get; set; }

    public DateTime? OwnershipStartDate { get; set; }
    public DateTime? OwnershipEndDate { get; set; }

    // Navigation Properties
    [JsonIgnore]
    public ICollection<Fostering> Fosterings { get; set; } = new List<Fostering>();

    [JsonIgnore]
    public ICollection<OwnershipRequest> OwnershipRequests { get; set; } = new List<OwnershipRequest>();

    [JsonIgnore]
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();

    [JsonIgnore]
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}