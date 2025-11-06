namespace WebAPI.DTOs.AnimalSchedule;

/// <summary>
/// Data transfer object representing a shelter unavailability slot,
/// extending <see cref="SlotDto"/> with an optional reason for the unavailability.
/// </summary>
public class ShelterUnavailabilitySlotDto : SlotDto
{
    /// <summary>
    /// Optional description indicating the reason for the shelter’s unavailability.
    /// </summary>
    public string? Reason { get; init; }
}