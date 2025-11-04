using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Favorites.Commands
{
    /// <summary>
    /// Deactivates a user's favorite record.
    /// </summary>
    public class DeactivateFavorite
    {
        public class Command : IRequest<Result<Animal>>
        {
            public required string AnimalId { get; set; }
        }

        public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
            : IRequestHandler<Command, Result<Animal>>
        {
            public async Task<Result<Animal>> Handle(Command request, CancellationToken ct)
            {
                var user = await userAccessor.GetUserAsync();
                if (user == null)
                    return Result<Animal>.Failure("User not authenticated", 401);

                // Find the favorite to deactivate
                var favorite = await dbContext.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.AnimalId == request.AnimalId && f.IsActive, ct);

                if (favorite == null)
                    return Result<Animal>.Failure("Favorite not found", 404);

                // Deactivate the record
                favorite.IsActive = false;
                favorite.UpdatedAt = DateTime.UtcNow;

                // Get related animal
                var animal = await dbContext.Animals
                    .Include(a => a.Breed)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

                var success = await dbContext.SaveChangesAsync(ct) > 0;

                return success
                    ? Result<Animal>.Success(animal, 200)
                    : Result<Animal>.Failure("Failed to deactivate favorite", 400);
            }
        }
    }
}