using Domain.Enums;

using WebAPI.DTOs.Images;

namespace WebAPI.DTOs.Ownership;

/// <summary>
/// Data transfer object representing ownership information for a user,
/// including animal details, financial data, and ownership status.
/// </summary>
public class ResUserOwnershipsDto
{
    /// <summary>
    /// Unique identifier of the ownership record.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier of the associated animal.
    /// </summary>
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the owned animal.
    /// </summary>
    public string AnimalName { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the animal (e.g., Available, Adopted, Fostered).
    /// </summary>
    public AnimalState AnimalState { get; set; }

    /// <summary>
    /// Image associated with the animal.
    /// </summary>
    public ResImageDto Image { get; set; } = null!;

    /// <summary>
    /// Monetary amount related to the ownership (e.g., cost or contribution).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Current ownership status (e.g., Pending, Approved, Rejected).
    /// </summary>
    public OwnershipStatus? OwnershipStatus { get; set; }

    /// <summary>
    /// Optional additional information provided in the ownership request.
    /// </summary>
    public string? RequestInfo { get; set; }

    /// <summary>
    /// Date and time when the ownership request was created.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Date and time when the ownership request was approved, if applicable.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Date and time when the ownership record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
