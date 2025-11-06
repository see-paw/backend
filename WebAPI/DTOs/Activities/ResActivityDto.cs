using Domain.Enums;

namespace WebAPI.DTOs.Activities;

/// <summary>
/// Represents a response containing summarized information about an activity.
/// </summary>
public class ResActivityDto
{
    /// <summary>
    /// Unique identifier of the activity.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the related animal.
    /// </summary>
    public string AnimalId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the related animal.
    /// </summary>
    public string AnimalName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the user who scheduled the activity.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user who scheduled the activity.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the activity (e.g., Fostering, Ownership).
    /// </summary>
    public ActivityType Type { get; set; }

    /// <summary>
    /// Current status of the activity (e.g., Active, Completed, Cancelled).
    /// </summary>
    public ActivityStatus Status { get; set; }

    /// <summary>
    /// Scheduled start date and time of the activity.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Scheduled end date and time of the activity.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Date and time when the activity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}