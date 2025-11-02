using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Favorites.Commands
{
    /// <summary>
    /// Adds or reactivates a favorite record for a user.
    /// </summary>
    public class AddFavorite
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

                // Get animal and verify existence
                var animal = await dbContext.Animals
                    .Include(a => a.Breed)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found", 404);

                // Check animal availability
                if (animal.AnimalState != AnimalState.Available &&
                    animal.AnimalState != AnimalState.PartiallyFostered)
                {
                    return Result<Animal>.Failure(
                        "Animal is not available to be favorited",
                        409
                    );
                }

                // Check if favorite already exists
                var favorite = await dbContext.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.AnimalId == request.AnimalId, ct);

                if (favorite != null)
                {
                    if (favorite.IsActive)
                        return Result<Animal>.Failure("Animal already in favorites", 409);

                    // Reactivate the favorite
                    favorite.IsActive = true;
                    favorite.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new favorite
                    favorite = new Favorite
                    {
                        UserId = user.Id,
                        AnimalId = request.AnimalId,
                        IsActive = true
                    };
                    dbContext.Favorites.Add(favorite);
                }

                var success = await dbContext.SaveChangesAsync(ct) > 0;

                return success
                    ? Result<Animal>.Success(animal, 201)
                    : Result<Animal>.Failure("Failed to add favorite", 400);
            }
        }
    }
}
