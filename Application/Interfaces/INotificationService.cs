using Domain;
using Domain.Enums;

namespace Application.Interfaces;

/// <summary>
/// Service for managing and sending notifications to users.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates and sends a notification to a specific user.
    /// </summary>
    Task<Notification> CreateAndSendToUserAsync(
        string userId,
        NotificationType type,
        string message,
        string? animalId = null,
        string? ownershipRequestId = null);

    /// <summary>
    /// Creates and sends a broadcast notification to all users with a specific role.
    /// </summary>
    Task<List<Notification>> CreateAndSendToRoleAsync(
        string role,
        NotificationType type,
        string message,
        string? animalId = null);

    /// <summary>
    /// Sends a notification via SignalR to a specific user (if connected).
    /// </summary>
    Task SendToUserAsync(string userId, Notification notification);

    /// <summary>
    /// Sends a notification via SignalR to all users with a specific role (if connected).
    /// </summary>
    Task SendToRoleAsync(string role, Notification notification);
}