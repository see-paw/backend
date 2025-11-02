namespace Domain.Enums;

/// <summary>
/// Defines the types of notifications that can be sent to users.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Notification sent to AdminCAA when a new ownership request is created.
    /// </summary>
    NEW_OWNERSHIP_REQUEST,

    /// <summary>
    /// Notification sent to user when their ownership request is approved.
    /// </summary>
    OWNERSHIP_REQUEST_APPROVED,

    /// <summary>
    /// Notification sent to user when their ownership request is being analyzed.
    /// </summary>
    OWNERSHIP_REQUEST_ANALYZING,

    /// <summary>
    /// Notification sent to user when their ownership request is rejected.
    /// </summary>
    OWNERSHIP_REQUEST_REJECTED,

    /// <summary>
    /// Broadcast notification sent to all users when a new animal is added to the catalog.
    /// </summary>
    NEW_ANIMAL_ADDED,

    /// <summary>
    /// Notification sent to fostering users when their fostered animal is adopted.
    /// </summary>
    FOSTERED_ANIMAL_ADOPTED
}