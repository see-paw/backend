using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Notifications.Commands;

/// <summary>
/// Handles the operation of marking a notification as read for the authenticated user.
/// </summary>
/// <remarks>
/// This command ensures that only the owner of a notification can mark it as read,
/// performing proper validation and persistence.  
/// It is typically triggered when a user views or acknowledges a notification
/// from the client interface.
/// </remarks>
public class MarkNotificationAsRead
{
    /// <summary>
    /// Represents the command request to mark a specific notification as read.
    /// </summary>
    /// <remarks>
    /// The command requires the <see cref="NotificationId"/> of the notification to update.  
    /// </remarks>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// The unique identifier of the notification to be marked as read.
        /// </summary>
        public string NotificationId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the execution of the command that marks a notification as read.
    /// </summary>
    /// <remarks>
    /// Performs the following operations:
    /// <list type="bullet">
    /// <item>Retrieves the authenticated user's ID from <see cref="IUserAccessor"/>.</item>
    /// <item>Finds the notification associated with that user and ID.</item>
    /// <item>If not found, returns a <c>404</c> result.</item>
    /// <item>If already marked as read, returns success without changes.</item>
    /// <item>Otherwise, sets <see cref="Domain.Notification.IsRead"/> to <c>true</c> and records <see cref="Domain.Notification.ReadAt"/>.</item>
    /// <item>Saves the changes to the database and returns the operation result.</item>
    /// </list>
    /// </remarks>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Command, Result<Unit>>
    {
        /// <summary>
        /// Executes the command to mark the specified notification as read.
        /// </summary>
        /// <param name="request">The command containing the ID of the notification to mark as read.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> indicating whether the operation succeeded.
        /// Returns:
        /// <list type="bullet">
        /// <item><c>200</c> if successful or already read.</item>
        /// <item><c>404</c> if the notification was not found for the user.</item>
        /// <item><c>500</c> if an unexpected error occurred during persistence.</item>
        /// </list>
        /// </returns>
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

            if (notification == null)
                return Result<Unit>.Failure("Notification not found", 404);

            if (notification.IsRead)
                return Result<Unit>.Success(Unit.Value, 200);

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<Unit>.Failure("Failed to mark notification as read", 500);

            return Result<Unit>.Success(Unit.Value, 200);
        }
    }
}