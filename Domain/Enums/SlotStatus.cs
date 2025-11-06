namespace Domain.Enums;

/// <summary>
/// Represents the current availability state of a <see cref="Slot"/>.
/// </summary>
public enum SlotStatus
{
    /// <summary>
    /// The slot is free and available for new reservations or activities.
    /// </summary>
    Available,
    
    /// <summary>
    /// The slot is blocked and cannot be used for any reservation or activity.
    /// </summary>
    Unavailable,
    
    /// <summary>
    /// The slot is currently occupied by an existing reservation or scheduled activity.
    /// </summary>
    Reserved,
}