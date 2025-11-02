using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain;

/// <summary>
/// Represents an activity related to an animal within the system.
/// </summary>
/// <remarks>
/// An activity defines a period and relationship between a user and an animal,
/// such as fostering or ownership.  
/// Each activity is uniquely identified by its ID and is associated with specific status and type values.
/// </remarks>
public class Activity
{
    /// <summary>
    /// Unique identifier of the activity (GUID).  
    /// A constraint in <c>AppDbContext.OnModelCreating</c> ensures that the combination of <c>AnimalId</c> and <c>StartDate</c> is unique.
    /// </summary>
    [Key]
    [MaxLength(36)]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The foreign key referencing the animal associated with this activity.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string AnimalId { get; init; } = string.Empty;

    /// <summary>
    /// The foreign key referencing the user involved in this activity.
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The type of activity (e.g., fostering or ownership).
    /// </summary>
    [Required]
    public ActivityType Type { get; init; }

    /// <summary>
    /// The current status of the activity (e.g., active, cancelled, or completed).
    /// </summary>
    [Required]
    public ActivityStatus Status { get; init; }

    /// <summary>
    /// The start date of the activity.
    /// </summary>
    [Required]
    public DateTime StartDate { get; init; }

    /// <summary>
    /// The end date of the activity.
    /// </summary>
    [Required]
    public DateTime EndDate { get; init; }

    /// <summary>
    /// The UTC timestamp when the activity record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The animal entity associated with this activity.
    /// </summary>
    public Animal Animal { get; init; } = null!;

    /// <summary>
    /// The user entity participating in this activity.
    /// </summary>
    public User User { get; set; } = null!;
    public ActivitySlot? Slot { get; set; }
}