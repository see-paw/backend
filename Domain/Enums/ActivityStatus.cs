namespace Domain.Enums;

/// <summary>
/// Represents the possible states of an activity within the system.
/// </summary>
/// <remarks>
/// Used to indicate whether an activity is ongoing, has been cancelled, or has been successfully completed.
/// </remarks>
public enum ActivityStatus
{
    /// <summary>
    /// The activity is currently ongoing and active.
    /// </summary>
    Active,

    /// <summary>
    /// The activity has been cancelled before completion.
    /// </summary>
    Canceled,

    /// <summary>
    /// The activity has been successfully completed.
    /// </summary>
    Completed
}
