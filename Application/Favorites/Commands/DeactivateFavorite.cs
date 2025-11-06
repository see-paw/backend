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
        /// <summary>
        /// Represents the command request to deactivate a user's favorite.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The unique identifier of the animal to be removed from favorites.
            /// </summary>
            public required string AnimalId { get; set; }
        }

        /// <summary>
        /// Handles the logic for deactivating a favorite record for the current authenticated user.
        /// </summary>
        public class Handler(AppDbContext dbContext, IUserAccessor userAccessor, IMapper mapper)
            : IRequestHandler<Command, Result<Animal>>
        {
            /// <summary>
            /// Executes the process of deactivating a favorite record for a user.
            /// </summary>
            /// <param name="request">The command containing the <see cref="Animal"/> ID to deactivate.</param>
            /// <param name="ct">The cancellation token used to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the related <see cref="Animal"/> if the operation succeeds,  
            /// or an error result with the appropriate message and HTTP status code if any validation fails.
            /// </returns>
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
                    .AsNoTracking()
                    .Include(a => a.Breed)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

                if (animal == null)
                {
                    return Result<Animal>.Failure("Animal not found", 404);
                }
                var success = await dbContext.SaveChangesAsync(ct) > 0;

                return success
                    ? Result<Animal>.Success(animal, 200)
                    : Result<Animal>.Failure("Failed to deactivate favorite", 400);
            }
        }
    }
}