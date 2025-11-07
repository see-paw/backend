using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain;

/// <summary>
/// Represents a notification sent to a user.
/// </summary>
public class Notification
{
    /// <summary>
    /// The unique identifier of the notification.
    /// </summary>
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The identifier of the user who receives this notification.
    /// Null for broadcast notifications.
    /// </summary>
    [MaxLength(36)]
    public string? UserId { get; set; }

    /// <summary>
    /// The type of notification.
    /// </summary>
    [Required]
    public NotificationType Type { get; set; }

    /// <summary>
    /// The notification message content.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the notification has been read by the user.
    /// </summary>
    [Required]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// The UTC timestamp when the notification was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The UTC timestamp when the notification was marked as read.
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Reference to the related animal.
    /// </summary>
    [MaxLength(36)]
    public string? AnimalId { get; set; }

    /// <summary>
    /// Reference to the related ownership request.
    /// </summary>
    [MaxLength(36)]
    public string? OwnershipRequestId { get; set; }

    /// <summary>
    /// Reference to the related activity (ownership or fostering).
    /// </summary>
    [MaxLength(36)]
    public string? ActivityId { get; set; }

    /// <summary>
    /// Indicates if this is a broadcast notification (sent to multiple users by role).
    /// </summary>
    [Required]
    public bool IsBroadcast { get; set; } = false;

    /// <summary>
    /// Target role for broadcast notifications ("User" or "AdminCAA").
    /// </summary>
    [MaxLength(50)]
    public string? TargetRole { get; set; }

    /// <summary>
    /// The <see cref="User"/> entity associated with this notification.
    /// </summary>
    [JsonIgnore]
    public User? User { get; set; }

    /// <summary>
    /// The <see cref="Animal"/> entity associated with this notification.
    /// </summary>
    [JsonIgnore]
    public Animal? Animal { get; set; }

    /// <summary>
    /// The <see cref="OwnershipRequest"/> entity related to this notification.
    /// </summary>
    [JsonIgnore]
    public OwnershipRequest? OwnershipRequest { get; set; }

    /// <summary>
    /// The <see cref="Activity"/> entity related to this notification.
    /// </summary>
    [JsonIgnore]
    public Activity? Activity { get; set; }
}