using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using System.Text.Json.Serialization;
using Domain.Interfaces;


namespace Domain;

/// <summary>
/// Represents an animal entity within the system.
/// </summary>
/// <remarks>
/// Contains all relevant data about an animal, including biological information,
/// ownership details, and relationships with shelters, breeds, and activities.
/// </remarks>
public class Animal : IHasPhotos
{
    /// <summary>
    /// Unique identifier of the animal (GUID).
    /// </summary>
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The animal’s display name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The current availability or ownership state of the animal.
    /// </summary>
    [Required]
    public AnimalState AnimalState { get; set; } = AnimalState.Available;

    /// <summary>
    /// Short textual description of the animal.
    /// </summary>
    [StringLength(250)]
    public string? Description { get; set; }

    /// <summary>
    /// The species classification of the animal (e.g., dog, cat, bird).
    /// </summary>
    [Required]
    public Species Species { get; set; }

    /// <summary>
    /// The size category of the animal (e.g., small, medium, large).
    /// </summary>
    [Required]
    public SizeType Size { get; set; }

    /// <summary>
    /// The biological sex of the animal.
    /// </summary>
    [Required]
    public SexType Sex { get; set; }

    /// <summary>
    /// The color or main color pattern of the animal.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Colour { get; set; } = string.Empty;

    /// <summary>
    /// The animal’s date of birth.
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Indicates whether the animal has been sterilized.
    /// </summary>
    [Required]
    public bool Sterilized { get; set; }

    /// <summary>
    /// The monthly cost associated with the animal’s care.
    /// </summary>
    [Required]
    [Range(0, 1000, ErrorMessage = "Cost must be between 0 and 1000.")]
    public decimal Cost { get; set; }

    /// <summary>
    /// Additional characteristics or traits of the animal.
    /// </summary>
    [StringLength(300)]
    public string? Features { get; set; }

    /// <summary>
    /// The UTC timestamp when the record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The collection of images associated with the animal.
    /// At least one image is required, with one marked as principal.
    /// </summary>
    [JsonIgnore]
    public ICollection<Image> Images { get; set; } = new List<Image>();

    /// <summary>
    /// The foreign key referencing the shelter where the animal is located.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string ShelterId { get; set; } = string.Empty;

    /// <summary>
    /// The shelter entity associated with the animal.
    /// </summary>
    [JsonIgnore]
    public Shelter Shelter { get; set; } = null!;

    /// <summary>
    /// The foreign key referencing the breed of the animal.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string BreedId { get; set; } = string.Empty;

    /// <summary>
    /// The breed entity associated with the animal.
    /// </summary>
    [JsonIgnore]
    public Breed Breed { get; set; } = null!;

    /// <summary>
    /// The unique identifier of the user who owns the animal, if applicable.
    /// </summary>
    [MaxLength(36)]
    public string? OwnerId { get; set; }

    /// <summary>
    /// The user entity representing the animal’s owner.
    /// </summary>
    [JsonIgnore]
    public User? Owner { get; set; }

    /// <summary>
    /// The date when ownership of the animal began.
    /// </summary>
    public DateTime? OwnershipStartDate { get; set; }

    /// <summary>
    /// The date when ownership of the animal ended, if applicable.
    /// </summary>
    public DateTime? OwnershipEndDate { get; set; }

    /// <summary>
    /// The list of fostering records associated with the animal.
    /// </summary>
    [JsonIgnore]
    public ICollection<Fostering> Fosterings { get; set; } = new List<Fostering>();

    /// <summary>
    /// The collection of ownership requests made for this animal.
    /// </summary>
    [JsonIgnore]
    public ICollection<OwnershipRequest> OwnershipRequests { get; set; } = new List<OwnershipRequest>();

    /// <summary>
    /// The list of activities related to this animal (e.g., visits, fostering updates).
    /// </summary>
    [JsonIgnore]
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();

    /// <summary>
    /// The list of users who have marked this animal as a favorite.
    /// </summary>
    [JsonIgnore]
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}