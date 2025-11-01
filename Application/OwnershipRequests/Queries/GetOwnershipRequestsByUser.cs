using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Ownerships.Queries;

/// <summary>
/// Retrieves all ownership requests submitted by the currently authenticated user.
/// </summary>
/// <remarks>
/// This query returns ownership requests associated with the current user.
/// It includes related entities such as <see cref="Animal"/>, <see cref="Breed"/>, <see cref="Shelter"/> and <see cref="Image"/>.
/// Only requests that are pending, approved, or rejected within the last month are returned.
/// </remarks>
public class GetOwnershipRequestsByUser
{
    /// <summary>
    /// Represents the query to retrieve ownership requests of the current user.
    /// </summary>
    public class Query : IRequest<Result<List<OwnershipRequest>>>
    {
        
    }

    /// <summary>
    /// Handles the retrieval of ownership requests made by the current user.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="userAcessor">Provides access to the currently authenticated user.</param>
    public class Handler(AppDbContext context, IUserAccessor userAcessor) : IRequestHandler<Query, Result<List<OwnershipRequest>>>
    {
        /// <summary>
        /// Executes the query to retrieve all relevant ownership requests for the current user.
        /// </summary>
        /// <param name="request">The query request (no parameters required).</param>
        /// <param name="cancellationToken">A token for cancelling the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="OwnershipRequest"/> objects if successful,
        /// or a failure result if the user is not found or no data matches the query.
        /// </returns>
        public async Task<Result<List<OwnershipRequest>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAcessor.GetUserAsync();
            
            // Ensure the user exists before proceeding
            if (currentUser == null)
                return Result<List<OwnershipRequest>>.Failure("User not found", 404);
            
            // Only show rejected requests updated in the last month
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            
            var ownershipRequests = await context.OwnershipRequests
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Breed)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Shelter)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Images)
                .Where(or => or.UserId == currentUser.Id && 
                             ((or.Status != OwnershipStatus.Rejected && or.Status != OwnershipStatus.Approved) ||
                              (or.Status == OwnershipStatus.Rejected && or.UpdatedAt >= oneMonthAgo)))
                .OrderBy(or => or.Status == OwnershipStatus.Rejected ? 1 : 0) // Non rejected first
                .ThenByDescending(or => or.RequestedAt) // then by decreasing order
                .ToListAsync(cancellationToken);
            
            return Result<List<OwnershipRequest>>.Success(ownershipRequests, 200);

        }
    }
}