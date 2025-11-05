using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Notifications.Queries;


/// <summary>
/// Handles the retrieval of notifications belonging to the authenticated user.
/// </summary>
/// <remarks>
/// This query provides access to a user's notification history, supporting optional
/// filtering for unread notifications only. It ensures that the results are ordered
/// chronologically (most recent first) and belong to the authenticated user.
/// </remarks>
public class GetUserNotifications
{
    public class Query : IRequest<Result<List<Notification>>> 
    {
        /// <summary>
        /// When set to true, retrieves only notifications that have not been marked as read.
        /// When null or false, retrieves all notifications.
        /// </summary>
        public bool? UnreadOnly { get; set; }
    }

    /// <summary>
    /// Handles the execution of the query to retrieve the user's notifications.
    /// </summary>
    /// <remarks>
    /// Performs the following operations:
    /// - Retrieves the authenticated user's ID via <see cref="IUserAccessor"/>.
    /// - Filters notifications belonging to that user.
    /// - Optionally restricts results to unread notifications.
    /// - Orders notifications by creation date (most recent first).
    /// - Returns the list wrapped in a standardized <see cref="Result{T}"/> response.
    /// </remarks>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Query, Result<List<Notification>>> 
    {
        /// <summary>
        /// Executes the query to retrieve user notifications.
        /// </summary>
        /// <param name="request">The query parameters for filtering notifications.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the list of notifications associated
        /// with the authenticated user, ordered by creation date.
        /// </returns>
        public async Task<Result<List<Notification>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var query = context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (request.UnreadOnly == true)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query.ToListAsync(cancellationToken); 

            return Result<List<Notification>>.Success(notifications, 200);
        }
    }
}