namespace Application.Notifications.DTOs;

/// <summary>
/// Response DTO for notification data sent to clients.
/// </summary>
public class ResNotificationDto
{
    /// <summary>
    /// Unique identifier of the notification.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification (e.g., NEW_OWNERSHIP_REQUEST, OWNERSHIP_REQUEST_APPROVED).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Notification message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Related animal ID (if applicable).
    /// </summary>
    public string? AnimalId { get; set; }

    /// <summary>
    /// Related ownership request ID (if applicable).
    /// </summary>
    public string? OwnershipRequestId { get; set; }

    /// <summary>
    /// Indicates if the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Timestamp when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}