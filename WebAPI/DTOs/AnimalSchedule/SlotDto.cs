using Domain.Enums;

namespace WebAPI.DTOs.AnimalSchedule;

/// <summary>
/// Data transfer object representing a generic time slot,
/// used as a base type for scheduling-related DTOs.
/// </summary>
public class SlotDto
{
    /// <summary>
    /// Unique identifier of the slot.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Start time of the slot (in ISO string format).
    /// </summary>
    public string Start { get; init; } = null!;

    /// <summary>
    /// End time of the slot (in ISO string format).
    /// </summary>
    public string End { get; init; } = null!;
}
