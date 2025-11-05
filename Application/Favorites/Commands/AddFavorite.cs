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
        /// <summary>
        /// Represents the request to add or reactivate a favorite animal.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The unique identifier of the animal to be favorited.
            /// </summary>
            public required string AnimalId { get; set; }
        }

        /// <summary>
        /// Handles the logic for adding or reactivating a favorite animal for the current user.
        /// </summary>
        public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
            : IRequestHandler<Command, Result<Animal>>
        {
            /// <summary>
            /// Executes the process of adding or reactivating a favorite record for a user.
            /// </summary>
            /// <param name="request">The command containing the target <see cref="Animal"/> ID.</param>
            /// <param name="ct">The cancellation token used to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the favorited <see cref="Animal"/> if successful,  
            /// or an error result with the appropriate message and HTTP status code if any validation fails.
            /// </returns>
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
