using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Notifications.Commands;

/// <summary>
/// Handles the deletion of a user notification.
/// </summary>
/// <remarks>
/// This command allows the authenticated user to permanently delete a specific notification
/// from their account. It ensures that users can only delete notifications that belong
/// to them, enforcing proper access control and data integrity.
/// </remarks>
public class DeleteNotification
{
    /// <summary>
    /// Represents the command request to delete a notification.
    /// </summary>
    /// <remarks>
    /// Requires the <see cref="NotificationId"/> of the notification to be removed.
    /// </remarks>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// ID of the notification to delete.
        /// </summary>
        public string NotificationId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the execution of the command to delete a notification.
    /// </summary>
    /// <remarks>
    /// Performs the following operations:
    /// <list type="bullet">
    /// <item>Retrieves the authenticated user's ID using <see cref="IUserAccessor"/>.</item>
    /// <item>Queries the database for the notification matching both the provided ID and user ID.</item>
    /// <item>Returns a <c>404</c> result if no matching notification is found.</item>
    /// <item>Removes the notification and saves the changes.</item>
    /// <item>Returns success if the operation completes successfully.</item>
    /// </list>
    /// </remarks>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Command, Result<Unit>>
    {
        /// <summary>
        /// Executes the deletion of a notification belonging to the authenticated user.
        /// </summary>
        /// <param name="request">
        /// The <see cref="Command"/> containing the <see cref="Command.NotificationId"/> of the notification to delete.
        /// </param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> wrapping a <see cref="Unit"/> result if the deletion succeeds,  
        /// or an error result with an appropriate message and HTTP status code if it fails.
        /// </returns>
        /// <remarks>
        /// This method performs the following sequence:
        /// <list type="number">
        /// <item>Retrieves the current user’s ID via <see cref="IUserAccessor"/>.</item>
        /// <item>Searches for the notification that matches both the provided ID and the user’s ID.</item>
        /// <item>If no notification is found, returns a <c>404</c> failure result.</item>
        /// <item>Removes the notification from the database context and commits the change.</item>
        /// <item>Returns a success result with status <c>200</c> if the deletion is completed successfully.</item>
        /// </list>
        /// Possible return codes:
        /// <list type="bullet">
        /// <item><b>200</b> – Notification successfully deleted.</item>
        /// <item><b>404</b> – Notification not found for the authenticated user.</item>
        /// <item><b>500</b> – Database update failed during deletion.</item>
        /// </list>
        /// </remarks>
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

            if (notification == null)
                return Result<Unit>.Failure("Notification not found", 404);

            context.Notifications.Remove(notification);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<Unit>.Failure("Failed to delete notification", 500);

            return Result<Unit>.Success(Unit.Value, 200);
        }
    }
}