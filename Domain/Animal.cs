using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using System.Text.Json.Serialization;


namespace Domain;

/// <summary>
/// Represents an animal entity within the system.
/// </summary>
/// <remarks>
/// Contains all relevant data about an animal, including biological information,
/// ownership details, and relationships with shelters, breeds, and activities.
/// </remarks>
public class Animal
{
    /// <summary>
    /// Unique identifier of the animal (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The animal’s display name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The current availability or ownership state of the animal.
    /// </summary>
    [Required]
    public AnimalState AnimalState { get; init; } = AnimalState.Available;

    /// <summary>
    /// Short textual description of the animal.
    /// </summary>
    [StringLength(250)]
    public string? Description { get; init; }

    /// <summary>
    /// The species classification of the animal (e.g., dog, cat, bird).
    /// </summary>
    [Required]
    public Species Species { get; init; }

    /// <summary>
    /// The size category of the animal (e.g., small, medium, large).
    /// </summary>
    [Required]
    public SizeType Size { get; init; }

    /// <summary>
    /// The biological sex of the animal.
    /// </summary>
    [Required]
    public SexType Sex { get; init; }

    /// <summary>
    /// The color or main color pattern of the animal.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Colour { get; init; } = string.Empty;

    /// <summary>
    /// The animal’s date of birth.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; init; }

    /// <summary>
    /// Indicates whether the animal has been sterilized.
    /// </summary>
    [Required]
    public bool Sterilized { get; init; }

    /// <summary>
    /// The monthly cost associated with the animal’s care.
    /// </summary>
    [Required]
    [Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000.")]
    public decimal Cost { get; init; }

    /// <summary>
    /// Additional characteristics or traits of the animal.
    /// </summary>
    [StringLength(300)]
    public string? Features { get; init; }

    /// <summary>
    /// The UTC timestamp when the record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// The collection of images associated with the animal.
    /// At least one image is required, with one marked as principal.
    /// </summary>
    [JsonIgnore]
    [MinLength(1, ErrorMessage = "Animal must have at least one image.")]
    public ICollection<Image> Images { get; init; } = new List<Image>();

    /// <summary>
    /// The foreign key referencing the shelter where the animal is located.
    /// </summary>
    [Required]
    public string ShelterId { get; init; } = string.Empty;

    /// <summary>
    /// The shelter entity associated with the animal.
    /// </summary>
    [JsonIgnore]
    public Shelter Shelter { get; set; } = null!;

    /// <summary>
    /// The foreign key referencing the breed of the animal.
    /// </summary>
    [Required]
    public string BreedId { get; init; } = string.Empty;

    /// <summary>
    /// The breed entity associated with the animal.
    /// </summary>
    [JsonIgnore]
    public Breed Breed { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the user who owns the animal, if applicable.
    /// </summary>
    public string? OwnerId { get; init; }

    /// <summary>
    /// The user entity representing the animal’s owner.
    /// </summary>
    [JsonIgnore]
    public User? Owner { get; init; }

    /// <summary>
    /// The date when ownership of the animal began.
    /// </summary>
    public DateTime? OwnershipStartDate { get; init; }

    /// <summary>
    /// The date when ownership of the animal ended, if applicable.
    /// </summary>
    public DateTime? OwnershipEndDate { get; init; }

    /// <summary>
    /// The list of fostering records associated with the animal.
    /// </summary>
    [JsonIgnore]
    public ICollection<Fostering> Fosterings { get; init; } = new List<Fostering>();

    /// <summary>
    /// The collection of ownership requests made for this animal.
    /// </summary>
    [JsonIgnore]
    public ICollection<OwnershipRequest> OwnershipRequests { get; init; } = new List<OwnershipRequest>();

    /// <summary>
    /// The list of activities related to this animal (e.g., visits, fostering updates).
    /// </summary>
    [JsonIgnore]
    public ICollection<Activity> Activities { get; init; } = new List<Activity>();

    /// <summary>
    /// The list of users who have marked this animal as a favorite.
    /// </summary>
    [JsonIgnore]
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
