namespace WebAPI.DTOs.AnimalSchedule;

/// <summary>
/// Data transfer object representing an activity slot associated with an animal's schedule.
/// Extends <see cref="SlotDto"/> with reservation details.
/// </summary>
public class ActivitySlotDto : SlotDto
{
    /// <summary>
    /// Name or identifier of the user who reserved the slot.
    /// </summary>
    public string ReservedBy { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the slot reservation belongs to the currently authenticated user.
    /// </summary>
    public bool IsOwnReservation { get; init; }
}
