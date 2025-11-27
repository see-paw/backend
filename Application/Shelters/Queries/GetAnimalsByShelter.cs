using Application.Animals.Filters;
using Application.Core;
using Domain;
using Domain.Enums;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shelters.Queries
{
    /// <summary>
    /// Provides the query and handler responsible for retrieving a paginated list of animals
    /// that belong to a specific shelter. Supports dynamic filtering, sorting, and pagination.
    /// Uses specification-based filtering and includes related Breed and Image data.
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

            /// <summary>
            /// parameter to sort the results by. Acceptable values: "name", "age", "created".
            /// </summary>
            public string? SortBy { get; set; } = null;

            /// <summary>
            /// direction of the sorting. Acceptable values: "asc", "desc".
            /// </summary>
            public string? Order { get; set; } = null;

            /// <summary>
            /// Optional filter criteria for animals (species, age, size, sex, name, breed).
            /// </summary>
            public AnimalFilterModel? Filters { get; set; }
        }

        /// <summary>
        /// Handles the query for retrieving animals that belong to a specific shelter.
        /// Validates shelter existence, applies specifications, dynamic sorting,
        /// and returns a paginated result set.
        /// </summary>
        public class Handler : IRequestHandler<Query, Result<PagedList<Animal>>>
        {
            private readonly AppDbContext _context;
            private readonly AnimalSpecBuilder _specBuilder;

            /// <summary>
            /// Creates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">Entity Framework Core database context.</param>
            /// <param name="specBuilder">Builder used to generate specification filters for animals.</param>
            public Handler(AppDbContext context, AnimalSpecBuilder specBuilder)
            {
                _context = context;
                _specBuilder = specBuilder;
            }

            /// <summary>
            /// Executes the request to retrieve a paginated list of animals for the specified shelter.
            /// Applies the following steps:
            /// <list type="number">
            /// <item><description>Validates pagination and shelter existence.</description></item>
            /// <item><description>Builds the base query including Breed and Image navigation properties.</description></item>
            /// <item><description>Applies specification-based filters if provided.</description></item>
            /// <item><description>Applies dynamic sorting based on <c>SortBy</c> / <c>Order</c>.</description></item>
            /// <item><description>Executes pagination using <see cref="PagedList{T}"/>.</description></item>
            /// </list>
            /// </summary>
            /// <param name="request">The query containing the required parameters.</param>
            /// <param name="cancellationToken">Cancellation token for the async operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing:
            /// <list type="bullet">
            /// <item><description><c>200 OK</c> — Paginated list successfully retrieved.</description></item>
            /// <item><description><c>404 Not Found</c> — Invalid page number, shelter not found, or no animals available.</description></item>
            /// </list>
            /// </returns>
            public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Validate pagination
                if (request.PageNumber < 1)
                    return Result<PagedList<Animal>>.Failure("Page number must be 1 or greater", 404);

                // Check whether the shelter exists in the database
                var shelterExists = await _context.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

                if (!shelterExists)
                    return Result<PagedList<Animal>>.Failure("Shelter not found", 404);

                // Build the query to retrieve animals belonging to this shelter
                var query = _context.Animals
                    .Include(a => a.Breed)   // Include breed object
                    .Include(a => a.Images)  // Include associated images
                    .Where(a => a.ShelterId == request.ShelterId)
                    .Where(a => a.AnimalState != AnimalState.Inactive)
                    .AsQueryable();

                // Apply dynamic filters if provided
                if (request.Filters != null)
                {
                    var specs = _specBuilder.Build(request.Filters);

                    foreach (var spec in specs)
                    {
                        var expression = spec.ToExpression();
                        if (expression != null)
                        {
                            query = query.Where(expression);
                        }
                    }
                }

                // Normalize params
                string sort = request.SortBy?.ToLower() ?? "created";
                string direction = request.Order?.ToLower() ?? "desc";
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Apply sorting based on parameters
                query = (sort, direction) switch
                {
                    ("name", "asc") => query.OrderBy(a => a.Name),
                    ("name", "desc") => query.OrderByDescending(a => a.Name),

                    ("age", "asc") => query.OrderBy(a => today.Year - a.BirthDate.Year),
                    ("age", "desc") => query.OrderByDescending(a => today.Year - a.BirthDate.Year),

                    ("created", "asc") => query.OrderBy(a => a.CreatedAt),
                    ("created", "desc") => query.OrderByDescending(a => a.CreatedAt),

                    _ => query.OrderByDescending(a => a.CreatedAt)
                };
                // Apply pagination using the generic PagedList helper
                var pagedList = await PagedList<Animal>.CreateAsync(
                    query,
                    request.PageNumber,
                    request.PageSize
                );

                // Handle the case when no animals were found
                if (pagedList.Items.Count == 0)
                    return Result<PagedList<Animal>>.Failure("No animals found for this shelter", 404);


                // Handle the case when no animals were found
                return pagedList.Items.Count == 0 ? Result<PagedList<Animal>>.Failure("No animals found for this shelter", 404) :
                    // Return success result with the paginated list
                    Result<PagedList<Animal>>.Success(pagedList, 200);
            }
        }
    }
}
