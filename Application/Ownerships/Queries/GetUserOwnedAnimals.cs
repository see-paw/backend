using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Ownerships.Queries;

public class GetUserOwnedAnimals
{
    public class Query : IRequest<Result<List<Animal>>>
    {
        
    }
    public class Handler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<Query, Result<List<Animal>>>
    {
        public async Task<Result<List<Animal>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAccessor.GetUserAsync();
            
            if (currentUser == null)
                return Result<List<Animal>>.Failure("User not found", 404);

            var ownedAnimals = await context.Animals
                .Include(a => a.Breed)
                .Include(a => a.Shelter)
                .Include(a => a.Images)
                .Where(a => a.OwnerId == currentUser.Id)
                .OrderByDescending(a => a.OwnershipStartDate)
                .ToListAsync(cancellationToken);

            return Result<List<Animal>>.Success(ownedAnimals, 200);
        }
    }
}