using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Queries;

/// <summary>
/// Retrieves a paginated list of ownership activities for the authenticated user.
/// 
/// This query allows users to view their scheduled visits and interactions with
/// animals they own, providing essential information for managing their activities
/// including dates, status, and animal details.
/// </summary>
public class GetOwnershipActivitiesByUser
{
    /// <summary>
    /// Query to retrieve paginated ownership activities for a user.
    /// </summary>
    public class Query : IRequest<Result<PagedList<Activity>>>
    {
        /// <summary>
        /// The page number to retrieve (default is 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// The number of items per page (default is 20).
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Optional filter by activity status (Active, Completed, Canceled, All).
        /// If not provided, returns all activities.
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// Handles the retrieval of paginated ownership activities with authentication.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<PagedList<Activity>>>
    {
        /// <summary>
        /// Retrieves a paginated list of ownership activities for the authenticated user.
        /// 
        /// This method performs the following operations:
        /// - Extracts the user ID from the authenticated JWT token
        /// - Queries all ownership activities belonging to the user
        /// - Optionally filters by activity status (Active, Completed, Canceled)
        /// - Includes related Animal and User data for complete information
        /// - Orders results by start date (upcoming activities first)
        /// - Applies pagination to limit the number of results returned
        /// </summary>
        /// <param name="request">The query containing pagination parameters and optional status filter.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A paginated list of ownership activities with metadata (current page, total pages, total count)
        /// if successful. Returns an empty paginated list (with TotalCount = 0) if no activities exist.
        /// </returns>
        /// <remarks>
        /// The user ID is extracted from the authenticated user's JWT token, ensuring
        /// users can only view their own activities.
        /// Results are ordered by StartDate in ascending order to show upcoming activities first.
        /// </remarks>
        public async Task<Result<PagedList<Activity>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var query = context.Activities
                .Include(a => a.Animal)
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.Type == ActivityType.Ownership);

            // Apply status filter if provided and not "All"
            if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
            {
                if (Enum.TryParse<ActivityStatus>(request.Status, ignoreCase: true, out var statusEnum))
                {
                    query = query.Where(a => a.Status == statusEnum);
                }
                else
                {
                    return Result<PagedList<Activity>>.Failure(
                        $"Invalid status value: {request.Status}. Valid values are: Active, Completed, Canceled, All", 400);
                }
            }

            query = query.OrderBy(a => a.StartDate);

            var pagedList = await PagedList<Activity>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<Activity>>.Success(pagedList, 200);
        }
    }
}