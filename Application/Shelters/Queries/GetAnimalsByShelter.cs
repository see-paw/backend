using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shelters.Queries;

public class GetAnimalsByShelter
{
    // Query object that carries parameters from the controller to the handler
    public class Query : IRequest<Result<PagedList<Animal>>>
    {
        public required string ShelterId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Handler responsible for executing the business logic and returning a result
    public class Handler(AppDbContext context)
        : IRequestHandler<Query, Result<PagedList<Animal>>>
    {
        public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Check whether the shelter exists in the database
            var shelterExists = await context.Shelters
                .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

            if (!shelterExists)
                return Result<PagedList<Animal>>.Failure("Shelter not found", 404);

            // Build the query to retrieve animals belonging to this shelter
            var query = context.Animals
                .Include(a => a.Breed)//breed object
                .Where(a => a.ShelterId == request.ShelterId)
                .OrderBy(a => a.Name)
                .AsQueryable();

            // Apply pagination using the generic PagedList helper
            var pagedList = await PagedList<Animal>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize
            );

            // Handle the case when no animals were found
            if (pagedList == null || pagedList.Count == 0)
                return Result<PagedList<Animal>>.Failure("No animals found for this shelter", 404);

            // Return success result with the paginated list
            return Result<PagedList<Animal>>.Success(pagedList);
        }
    }
}
