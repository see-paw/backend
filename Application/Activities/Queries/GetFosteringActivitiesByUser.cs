using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Queries;

/// <summary>
/// Query handler responsible for retrieving **active fostering activities** associated 
/// with the currently authenticated user.
/// 
/// This handler ensures that only valid and future fostering activities are returned, 
/// applying pagination and filtering logic to guarantee relevant results.
/// </summary>
public class GetFosteringActivitiesByUser
{
    /// <summary>
    /// Represents the request to retrieve paginated fostering activities for the current user.
    /// </summary>
    public class Query : IRequest<Result<PagedList<Activity>>>
    {
        /// <summary>
        /// The page number for pagination (must be greater than 0).
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// The number of records per page (must be between 1 and 50).
        /// </summary>
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Handles the logic for retrieving and filtering fostering activities.
    /// </summary>
    /// <param name="context">Database context for accessing activity data.</param>
    /// <param name="userAccessor">Service used to access the currently authenticated user.</param>
    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<PagedList<Activity>>>
    {
        /// <summary>
        /// Processes the query to retrieve a paginated list of fostering activities for the authenticated user.
        /// </summary>
        /// <param name="request">Query parameters including pagination info.</param>
        /// <param name="cancellationToken">Cancellation token for request control.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a paginated list of <see cref="Activity"/> objects, 
        /// or a failure result with an appropriate status code and message.
        /// </returns>
        public async Task<Result<PagedList<Activity>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Get current authenticated user
            var user = await userAccessor.GetUserAsync();

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                return Result<PagedList<Activity>>.Failure("Page number must be greater than 0", 400);
            }

            if (request.PageSize < 1 || request.PageSize > 50)
            {
                return Result<PagedList<Activity>>.Failure("Page size must be between 1 and 50", 400);
            }

            var now = DateTime.UtcNow;
            
            // Build query with all required filters
            var query = context.Activities
                .Include(a => a.Slot)
                .Include(a => a.Animal)
                    .ThenInclude(animal => animal.Images)
                .Include(a => a.Animal)
                    .ThenInclude(animal => animal.Breed)
                .Include(a => a.Animal)
                    .ThenInclude(animal => animal.Shelter)
                .Where(a => 
                    // Filter by current user
                    a.UserId == user.Id &&
                    // Only fostering activities
                    a.Type == ActivityType.Fostering &&
                    // Only active activities
                    a.Status == ActivityStatus.Active &&
                    // Only future visits
                    a.Slot != null &&
                    a.Slot.StartDateTime > now &&
                    // Slot must be reserved and of type Activity
                    a.Slot.Status == SlotStatus.Reserved &&
                    a.Slot.Type == SlotType.Activity &&
                    // Animal must have a principal image
                    a.Animal.Images.Any(img => img.IsPrincipal)
                )
                // Order by visit date (closest first)
                .OrderBy(a => a.Slot!.StartDateTime)
                .AsQueryable();

            // Additional validation: user must have active fostering for the animal
            query = query.Where(a => 
                a.Animal.Fosterings.Any(f => 
                    f.UserId == user.Id && 
                    f.Status == FosteringStatus.Active
                )
            );

            // Create paginated result
            var pagedList = await PagedList<Activity>.CreateAsync(
                query, 
                request.PageNumber, 
                request.PageSize
            );

            return Result<PagedList<Activity>>.Success(pagedList, 200);
            
            
        }
    }
}