namespace Domain.Enums;

/// <summary>
/// Defines the type or purpose of a <see cref="Slot"/> within the scheduling system.
/// </summary>
public enum SlotType
{
    /// <summary>
    /// The slot is linked to an activity, such as a reservation, visit, or other scheduled event.
    /// </summary>
    Activity,
    
    /// <summary>
    /// The slot marks a time range when the shelter is unavailable for scheduling.
    /// </summary>
    ShelterUnavailable
}