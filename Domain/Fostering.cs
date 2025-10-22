using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain;

/// <summary>
/// Represents an animal fostering, including the fostering period and contribution amount.
/// </summary>
/// <remarks>
/// Defines the relationship between a user and an animal for a given fostering period,
/// including financial contribution, status, and related navigation entities.
/// </remarks>
public class Fostering
{
    /// <summary>
    /// Unique identifier of the fostering record (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The foreign key referencing the fostered animal.
    /// </summary>
    [Required]
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key referencing the user who is fostering the animal.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The monthly financial contribution provided by the user for fostering.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The current status of the fostering (e.g., Active, Completed, or Canceled).
    /// </summary>
    [Required]
    public FosteringStatus Status { get; set; } = FosteringStatus.Active;

    /// <summary>
    /// The start date of the fostering period.
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the fostering period, if applicable.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// The UTC timestamp when the fostering record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The animal associated with this fostering record.
    /// </summary>
    public Animal Animal { get; set; } = null!;

    /// <summary>
    /// The user who is fostering the animal.
    /// </summary>
    public User User { get; set; } = null!;
}
