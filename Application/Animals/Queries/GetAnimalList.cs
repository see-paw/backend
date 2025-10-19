using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.Animals.Queries;

public class GetAnimalList
{
    public class Query : IRequest<Result<PagedList<Animal>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class Handler(AppDbContext context)
        : IRequestHandler<Query, Result<PagedList<Animal>>>
    {
        public async Task<Result<PagedList<Animal>>> Handle(Query request, CancellationToken cancellationToken)
        {
            //Base query with filters
            var query = context.Animals
                .Include(a => a.Breed)        // object breed
                .Include(a => a.Shelter)      // shelter's data
                .Include(a => a.Images)     
                .Where(a => a.AnimalState == AnimalState.Available
                         || a.AnimalState == AnimalState.PartiallyFostered)
                .OrderBy(a => a.Name)
                .AsQueryable();

            //Apply pagination using your PagedList helper
            var pagedList = await PagedList<Animal>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize
            );

            //Return consistent Result object
            if (pagedList == null || !pagedList.Any())
                return Result<PagedList<Animal>>.Failure("No animals found", 404);

            return Result<PagedList<Animal>>.Success(pagedList);
        }
    }
}
