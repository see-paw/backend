using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain;

/// <summary>
/// Represents an ownership request made by a user for a specific animal.
/// </summary>
/// <remarks>
/// Contains information about the request status, requested amount,  
/// and timestamps related to submission and approval.
/// </remarks>
public class OwnershipRequest
{
    /// <summary>
    /// Unique identifier of the ownership request (GUID).
    /// </summary>
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The foreign key referencing the requested animal.
    /// </summary>
    [Required]
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// The foreign key referencing the user who made the ownership request.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The proposed financial contribution or fee for ownership.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The current status of the ownership request (e.g., Pending, Approved, or Rejected).
    /// </summary>
    [Required]
    public OwnershipStatus Status { get; set; } = OwnershipStatus.Pending;

    /// <summary>
    /// Additional information provided by the user during the request.
    /// </summary>
    [MaxLength(500)]
    public string? RequestInfo { get; set; }

    /// <summary>
    /// The UTC timestamp when the ownership request was submitted.
    /// </summary>
    [Required]
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the request was approved, if applicable.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// The UTC timestamp when the ownership request record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The animal associated with this ownership request.
    /// </summary>
    public Animal Animal { get; set; } = null!;

    /// <summary>
    /// The user who submitted this ownership request.
    /// </summary>
    public User User { get; set; } = null!;
}
