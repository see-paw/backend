using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain;

/// <summary>
/// Represents a favorite relationship between a user and an animal.
/// </summary>
/// <remarks>
/// Indicates that a user has marked a specific animal as a favorite.  
/// This entity also tracks whether the favorite is currently active and when it was created or updated.
/// </remarks>
public class Favorite
{
    /// <summary>
    /// Unique identifier of the favorite entry (GUID).
    /// </summary>
    [Key]
    [MaxLength(36)]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The foreign key referencing the user who favorited the animal.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key referencing the favorited animal.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the favorite is currently active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// The UTC timestamp when the favorite record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the favorite record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The animal associated with this favorite record.
    /// </summary>
    [JsonIgnore]
    public Animal Animal { get; set; } = null!;

    /// <summary>
    /// The user who marked the animal as a favorite.
    /// </summary>
    [JsonIgnore]
    public User User { get; set; } = null!;
}