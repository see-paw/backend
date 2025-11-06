namespace WebAPI.DTOs.AnimalSchedule;

/// <summary>
/// Data transfer object representing a single day within an animal’s weekly schedule.
/// Includes available, reserved, and unavailable time slots.
/// </summary>
public class DailyScheduleDto
{
    /// <summary>
    /// The date of the schedule day (in ISO string format).
    /// </summary>
    public string Date { get; init; } = default!;

    /// <summary>
    /// Collection of available time slots for this day.
    /// </summary>
    public List<SlotDto> AvailableSlots { get; init; } = new();

    /// <summary>
    /// Collection of reserved activity slots for this day.
    /// </summary>
    public List<ActivitySlotDto> ReservedSlots { get; init; } = new();

    /// <summary>
    /// Collection of time slots when the shelter is unavailable.
    /// </summary>
    public List<ShelterUnavailabilitySlotDto> UnavailableSlots { get; init; } = new();
}
