using Application.Animals.Filters;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;
using Application.Interfaces;

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

            /// <summary>
            /// parameter to sort the results by. Acceptable values: "name", "age", "created".
            /// </summary>
            public string? SortBy { get; set; } = null;

            /// <summary>
            /// direction of the sorting. Acceptable values: "asc", "desc".
            /// </summary>
            public string? Order { get; set; } = null;
            
            /// <summary>
            /// Filter criteria for querying animals.
            /// </summary>
            public AnimalFilterModel? Filters { get; set; } = null;
        }

        /// <summary>
        /// Handles the execution of the query to fetch a paginated list of animals.
        /// Includes related entities (Breed, Shelter, Images) and filters by animal availability.
        /// </summary>
        public class Handler(AppDbContext _context, 
            AnimalSpecBuilder specBuilder) : IRequestHandler<Query, Result<PagedList<Animal>>>
        {
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
                if (request.PageNumber < 1)
                    return Result<PagedList<Animal>>.Failure("Page number must be 1 or greater", 404);

                // Base query with related entities
                var query = _context.Animals
                    .Include(a => a.Breed)        // Include breed information
                    .Include(a => a.Shelter)      // Include shelter data
                    .Include(a => a.Images)       // Include associated images
                    .Where(a => a.AnimalState == AnimalState.Available
                             || a.AnimalState == AnimalState.PartiallyFostered)
                    .AsQueryable();

                if (request.Filters != null)
                {
                    var specs = specBuilder.Build(request.Filters);
                    
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

                // Apply pagination using the PagedList helper
                var pagedList = await PagedList<Animal>.CreateAsync(
                    query,
                    request.PageNumber,
                    request.PageSize
                );

                // Return consistent Result object
                return !pagedList.Items.Any() ? Result<PagedList<Animal>>.Failure("No animals found", 404) : Result<PagedList<Animal>>.Success(pagedList, 200);
            }
        }
    }
}