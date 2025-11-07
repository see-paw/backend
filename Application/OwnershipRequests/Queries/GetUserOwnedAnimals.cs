using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Queries;

/// <summary>
/// Retrieves all animals currently owned by the authenticated user.
/// </summary>
/// <remarks>
/// This query fetches the list of animals that are officially registered as owned by the current user.
/// It includes related entities such as <see cref="Breed"/>, <see cref="Shelter"/> and <see cref="Image"/> 
/// to provide a complete view of the owned animals.
/// </remarks>
public class GetUserOwnedAnimals
{
    /// <summary>
    /// Represents the query to retrieve the animals owned by the current user.
    /// </summary>
    public class Query : IRequest<Result<List<Animal>>>
    {
        
    }
    
    /// <summary>
    /// Handles the logic for retrieving animals owned by the current authenticated user.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    /// <param name="userAccessor">Provides access to the currently authenticated user.</param>

    public class Handler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<Query, Result<List<Animal>>>
    {
        /// <summary>
        /// Executes the query to retrieve all animals owned by the authenticated user.
        /// </summary>
        /// <param name="request">The query request (no parameters required).</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="Animal"/> objects if successful, 
        /// or a failure result if the user is not found.
        /// </returns>
        public async Task<Result<List<Animal>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAccessor.GetUserAsync();
            
            // Validate that the authenticated user exists
            if (currentUser == null)
                return Result<List<Animal>>.Failure("User not found", 404);

            // Retrieve all animals whose OwnerId matches the current user's ID
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