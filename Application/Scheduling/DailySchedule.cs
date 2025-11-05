using Domain;

namespace Application.Scheduling;

/// <summary>
/// Represents the full daily schedule for a specific date.
/// </summary>
public class DailySchedule
{
    /// <summary>
    /// The calendar date represented by this schedule.
    /// </summary>
    public required DateOnly Date { get; init; }
    
    /// <summary>
    /// The list of available time blocks for this day.
    /// </summary>
    public required List<TimeBlock> AvailableSlots { get; set; } = new();
    
    /// <summary>
    /// The list of slots reserved for confirmed activities.
    /// </summary>
    public required List<Slot> ReservedSlots { get; set; } = new();
    
    /// <summary>
    /// The list of slots marking times when the shelter or resource is unavailable.
    /// </summary>
    public required List<Slot> UnavailableSlots { get; set; } = new();
}