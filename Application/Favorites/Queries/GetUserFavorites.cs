using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Favorites.Queries;

/// <summary>
/// Retrieves a paginated list of favorite animals for the authenticated user.
/// 
/// This query allows users to view all animals they have marked as favorites,
/// providing essential information about each animal including images, breed,
/// and shelter details for easy browsing and management.
/// </summary>
public class GetUserFavorites
{
    /// <summary>
    /// Query to retrieve paginated favorite animals for a user.
    /// </summary>
    public class Query : IRequest<Result<PagedList<Animal>>>
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
    /// Handles the retrieval of paginated favorite animals with authentication.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Query, Result<PagedList<Animal>>>
    {
        /// <summary>
        /// Retrieves a paginated list of favorite animals for the authenticated user.
        /// 
        /// This method performs the following operations:
        /// - Extracts the user ID from the authenticated JWT token
        /// - Queries all active favorites belonging to the user
        /// - Includes related Animal data with Images, Breed, and Shelter
        /// - Orders results by creation date (most recent favorites first)
        /// - Applies pagination to limit the number of results returned
        /// </summary>
        /// <param name="request">The query containing pagination parameters.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A paginated list of animals marked as favorites with metadata (current page, total pages, total count)
        /// if successful. Returns an empty paginated list (with TotalCount = 0) if no favorites exist.
        /// </returns>
        /// <remarks>
        /// The user ID is extracted from the authenticated user's JWT token, ensuring
        /// users can only view their own favorites.
        /// Only active favorites (IsActive = true) are returned.
        /// Results are ordered by CreatedAt in descending order to show most recent favorites first.
        /// </remarks>
        public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var query = context.Favorites
                .Where(f => f.UserId == userId && f.IsActive)
                .Include(f => f.Animal)
                    .ThenInclude(a => a.Images)
                .Include(f => f.Animal)
                    .ThenInclude(a => a.Breed)
                .Include(f => f.Animal)
                    .ThenInclude(a => a.Shelter)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Animal);

            var pagedList = await PagedList<Animal>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<Animal>>.Success(pagedList, 200);
        }
    }
}