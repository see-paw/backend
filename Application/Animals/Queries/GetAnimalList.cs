using MediatR;
using Domain;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Animals.Queries;

public class GetAnimalList
{
    public class Query : IRequest<List<Animal>> { }

    public class Handler(AppDbContext context) : IRequestHandler<Query, List<Animal>>
    {
        public async Task<List<Animal>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await context.Animals.ToListAsync(cancellationToken);
        }
    }
}