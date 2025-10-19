using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Animals.Queries
{
    /// <summary>
    /// Query and handler responsible for retrieving a paginated list of available or partially fostered animals.
    /// </summary>
    public class GetAnimalList
    {
        /// <summary>
        /// Represents the query parameters for retrieving animals.
        /// Supports pagination through <see cref="PageNumber"/> and <see cref="PageSize"/>.
        /// </summary>
        public class Query : IRequest<Result<PagedList<Animal>>>
        {
            /// <summary>
            /// The number of the page to be retrieved. Defaults to 1.
            /// </summary>
            public int PageNumber { get; set; } = 1;

            /// <summary>
            /// The number of records per page. Defaults to 20.
            /// </summary>
            public int PageSize { get; set; } = 20;
        }

        /// <summary>
        /// Handles the execution of the query to fetch a paginated list of animals.
        /// Includes related entities (Breed, Shelter, Images) and filters by animal availability.
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
            /// Executes the query by retrieving a paginated list of animals that are either available
            /// or partially fostered. The results include related data for breed, shelter, and images.
            /// </summary>
            /// <param name="request">The query containing pagination parameters.</param>
            /// <param name="cancellationToken">Token used to cancel the asynchronous operation if needed.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the paginated list of animals on success,
            /// or an error message and status code if no results are found.
            /// </returns>
            public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Base query with related entities
                var query = _context.Animals
                    .Include(a => a.Breed)        // Include breed information
                    .Include(a => a.Shelter)      // Include shelter data
                    .Include(a => a.Images)       // Include associated images
                    .Where(a => a.AnimalState == AnimalState.Available
                             || a.AnimalState == AnimalState.PartiallyFostered)
                    .OrderBy(a => a.Name)
                    .AsQueryable();

                // Apply pagination using the PagedList helper
                var pagedList = await PagedList<Animal>.CreateAsync(
                    query,
                    request.PageNumber,
                    request.PageSize
                );

                // Return consistent Result object
                if (pagedList == null || !pagedList.Any())
                    return Result<PagedList<Animal>>.Failure("No animals found", 404);

                return Result<PagedList<Animal>>.Success(pagedList);
            }
        }
    }
}
