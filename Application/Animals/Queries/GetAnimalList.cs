using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Queries;

public class GetAnimalList
{
    public class Query : IRequest<List<Animal>> { }

    public class Handler(AppDbContext context)
        : IRequestHandler<Query, List<Animal>>
    {
        public async Task<List<Animal>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var animals = await context.Animals.ToListAsync(cancellationToken);

            return animals;
        }
    }
}