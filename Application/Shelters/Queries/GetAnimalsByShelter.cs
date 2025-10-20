using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shelters.Queries
{
    /// <summary>
    /// Query and handler responsible for retrieving a paginated list of animals
    /// that belong to a specific shelter.
    /// </summary>
    public class GetAnimalsByShelter
    {
        /// <summary>
        /// Represents the query parameters required to fetch animals
        /// from a given shelter with pagination support.
        /// </summary>
        public class Query : IRequest<Result<PagedList<Animal>>>
        {
            /// <summary>
            /// The unique identifier of the shelter for which to retrieve animals.
            /// </summary>
            public required string ShelterId { get; set; }

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
        /// Handles the business logic for retrieving animals belonging to a specific shelter.
        /// Validates the existence of the shelter, applies pagination, and returns a consistent result.
        /// </summary>
        public class Handler : IRequestHandler<Query, Result<PagedList<Animal>>>
        {
            private readonly AppDbContext _context;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class using the provided database context.
            /// </summary>
            /// <param name="context">Entity Framework Core database context.</param>
            public Handler(AppDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Executes the query to fetch a paginated list of animals for the specified shelter.
            /// </summary>
            /// <param name="request">The query containing the shelter ID and pagination parameters.</param>
            /// <param name="cancellationToken">Token to cancel the asynchronous operation if needed.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing a <see cref="PagedList{Animal}"/> on success,
            /// or an error message and HTTP-like status code on failure.
            /// </returns>
            public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate if the shelter exists before querying animals
                if (!await ShelterExists(request.ShelterId, cancellationToken))
                    return Result<PagedList<Animal>>.Failure("Shelter not found", 404);

                // Build query to retrieve animals linked to the given shelter
                var query = BuildAnimalQuery(request.ShelterId);

                // Apply pagination using generic helper
                var pagedList = await PagedList<Animal>.CreateAsync(
                    query,
                    request.PageNumber,
                    request.PageSize
                );

                // Handle the case when no animals were found
                if (NoAnimalsFound(pagedList))
                    return Result<PagedList<Animal>>.Failure("No animals found for this shelter", 404);

                //Return success with the paginated list
                return Result<PagedList<Animal>>.Success(pagedList);
            }

            /// <summary>
            /// Checks asynchronously whether a shelter with the specified ID exists.
            /// </summary>
            private async Task<bool> ShelterExists(string shelterId, CancellationToken ct)
            {
                return await _context.Shelters
                    .AnyAsync(s => s.Id == shelterId, ct);
            }

            /// <summary>
            /// Builds the base query for retrieving animals of a specific shelter.
            /// Includes breed and images for complete data representation.
            /// </summary>
            private IQueryable<Animal> BuildAnimalQuery(string shelterId)
            {
                return _context.Animals
                    .Include(a => a.Breed)   // Include related breed data
                    .Include(a => a.Images)  // Include associated images
                    .Where(a => a.ShelterId == shelterId)
                    .OrderBy(a => a.Name)
                    .AsQueryable();
            }

            /// <summary>
            /// Checks if a paginated list of animals is null or empty.
            /// </summary>
            private static bool NoAnimalsFound(PagedList<Animal>? list)
            {
                return list == null || list.Count == 0;
            }
        }
    }
}
