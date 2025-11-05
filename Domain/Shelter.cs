using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace Domain;

/// <summary>
/// Represents an animal shelter within the system.
/// </summary>
/// <remarks>
/// Contains identification, contact, and scheduling information for a shelter,  
/// as well as relationships with animals and images registered under it.
/// </remarks>
public class Shelter: IHasImages
{
    /// <summary>
    /// Unique identifier of the shelter (GUID).
    /// </summary>
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The official name of the shelter.
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The street address where the shelter is located.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// The city where the shelter operates.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// The postal code of the shelter, formatted as 0000-000.
    /// </summary>
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "Postal Code must be in the format 0000-000.")]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// The phone number of the shelter.  
    /// Must contain 9 digits and start with 2 or 9.
    /// </summary>
    [Required]
    [Phone]
    [StringLength(100)]
    [RegularExpression(@"^[29]\d{8}$", ErrorMessage = "Phone number must have 9 digits and start with 2 or 9.")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The tax identification number (NIF) of the shelter.  
    /// Must contain exactly 9 digits.
    /// </summary>
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Nif must contain exactly 9 digits.")]
    public string NIF { get; init; } = string.Empty;

    /// <summary>
    /// The opening time of the shelter.
    /// </summary>
    [Required]
    public TimeOnly OpeningTime { get; set; }

    /// <summary>
    /// The closing time of the shelter.
    /// </summary>
    [Required]
    public TimeOnly ClosingTime { get; set; }

    /// <summary>
    /// The UTC timestamp when the shelter record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the shelter record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The list of animals currently registered in the shelter.
    /// </summary>
    [JsonIgnore]
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();

    /// <summary>
    /// The list of images of the shelter.
    /// </summary>
    [JsonIgnore]
    [MinLength(1, ErrorMessage = "Shelter must have at least one image.")]
    public ICollection<Image> Images { get; set; } = new List<Image>();
    
    /// <summary>
    /// The list of time slots during which the shelter is unavailable.
    /// </summary>
    [JsonIgnore]
    public ICollection<ShelterUnavailabilitySlot> UnavailabilitySlots { get; set; } = new List<ShelterUnavailabilitySlot>();
}
