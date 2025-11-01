using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Queries;

/// <summary>
/// Retrieves a paginated list of all ownership requests for animals in a specific shelter.
/// 
/// This query allows shelter administrators to view all adoption requests submitted for
/// animals under their care, providing essential information for managing the adoption
/// workflow including applicant details, request status, and submission dates.
/// </summary>
public class GetOwnershipRequestsByShelter
{
    /// <summary>
    /// Query to retrieve paginated ownership requests for a shelter.
    /// </summary>
    public class Query: IRequest<Result<PagedList<OwnershipRequest>>>
    {
        /// <summary>
        /// The page number to retrieve (default is 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// The number of items per page (default is 20).
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Handles the retrieval of paginated ownership requests with authentication and authorization.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAcessor) : IRequestHandler<Query, Result<PagedList<OwnershipRequest>>>
    {
        /// <summary>
        /// Retrieves a paginated list of ownership requests for the authenticated administrator's shelter.
        /// 
        /// This method performs the following operations:
        /// - Validates that the requester is a shelter administrator
        /// - Verifies that the shelter referenced by the administrator exists
        /// - Queries all ownership requests for animals in the administrator's shelter
        /// - Includes related Animal and User data for complete information
        /// - Orders results by request date (most recent first)
        /// - Applies pagination to limit the number of results returned
        /// </summary>
        /// <param name="request">The query containing pagination parameters (page number and page size).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A paginated list of ownership requests with metadata (current page, total pages, total count)
        /// if successful, or an error message with appropriate status code if validation fails.
        /// Returns an empty paginated list (with TotalCount = 0) if no requests exist for the shelter.
        /// </returns>
        /// <remarks>
        /// The shelter ID is extracted from the authenticated user's JWT token, ensuring
        /// administrators can only view requests for animals in their own shelter.
        /// Results are ordered by RequestedAt in descending order to show newest requests first.
        /// </remarks>
        public async Task<Result<PagedList<OwnershipRequest>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAcessor.GetUserAsync();
            if (currentUser.ShelterId == null)
            {
                return Result<PagedList<OwnershipRequest>>.Failure(
                    "Non Authorized Access: Only shelter adiministrators can view ownership requests", 403);
            }

            // Shelter referenced by the User has to exist
            var shelterExists = await context.Shelters.AnyAsync(s => s.Id == currentUser.ShelterId, cancellationToken);
            if (!shelterExists)
            {
                return Result<PagedList<OwnershipRequest>>.Failure(
                    "Shelter not found", 404);
            }

            var query = context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .Where(or => or.Animal.ShelterId == currentUser.ShelterId)
                .OrderByDescending(or => or.RequestedAt);

            var pagedList = await PagedList<OwnershipRequest>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<OwnershipRequest>>.Success(pagedList, 200);
        }
    }
    
        
}