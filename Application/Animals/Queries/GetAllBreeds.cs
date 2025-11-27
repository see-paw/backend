using Application.Core;
using Application.Interfaces;

using Domain;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Persistence;

namespace Application.Animals.Queries;

/// <summary>
/// Query responsible for retrieving all animal breeds stored in the local database.
/// If the database is empty, the handler automatically synchronizes the breed list
/// with the external breed API before returning the results.
///
/// This mechanism ensures that the platform always has an updated catalog of breeds,
/// while never removing or modifying existing records.
/// </summary>
public class GetAllBreeds
{
    /// <summary>
    /// Request object for fetching all breeds.
    /// </summary>
    public class Query : IRequest<Result<List<Breed>>>
    {
    }

    /// <summary>
    /// Handles the <see cref="Query"/> request.
    /// Performs an external API synchronization step and then returns all breeds
    /// sorted alphabetically.
    /// </summary>
    public class Handler : IRequestHandler<Query, Result<List<Breed>>>
    {
        private readonly AppDbContext _dbContext;
        private readonly IExternalBreedService _externalBreedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="dbContext">Database context used to read and write breed records.</param>
        /// <param name="externalBreedService">Service responsible for fetching breeds from an external API.</param>

        public Handler(
            AppDbContext dbContext,
            IExternalBreedService externalBreedService)
        {
            _dbContext = dbContext;
            _externalBreedService = externalBreedService;
        }

        /// <summary>
        /// Executes the handler logic:
        /// <list type="number">
        /// <item><description>Synchronizes the local database with external breed data (only inserts new breeds).</description></item>
        /// <item><description>Retrieves all existing breeds from the local database.</description></item>
        /// <item><description>Returns the complete list sorted alphabetically.</description></item>
        /// </list>
        /// </summary>
        /// <param name="request">The incoming query request (contains no parameters).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing:
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> — List of breeds successfully retrieved.</description></item>
        /// <item><description><c>500 Internal Server Error</c> — If synchronization or retrieval fails.</description></item>
        /// </list>
        /// </returns>
        public async Task<Result<List<Breed>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Synchronize local DB with external API if needed
                await SyncBreedsFromExternalAPI(cancellationToken);

                // Retrieve all breeds
                var allBreeds = await _dbContext.Breeds
                    .OrderBy(b => b.Name)
                    .ToListAsync(cancellationToken);

                return Result<List<Breed>>.Success(allBreeds, 200);
            }
            catch (Exception ex)
            {
                return Result<List<Breed>>.Failure($"Failed to fetch breeds: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Synchronizes the local breed database with the external API.
        /// Only new breeds are inserted:
        /// <list type="bullet">
        /// <item><description>Does NOT delete existing breeds.</description></item>
        /// <item><description>Does NOT update existing breeds.</description></item>
        /// <item><description>Prevents duplicates through case-insensitive comparison.</description></item>
        /// </list>
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task SyncBreedsFromExternalAPI(CancellationToken cancellationToken)
        {
            // Fetch from external API (service returns domain entities)
            var externalBreeds = await _externalBreedService.FetchBreedsAsync();

            // Get all existing breed names (for fast lookup)
            var existingBreedNames = await _dbContext.Breeds
                .Select(b => b.Name)
                .ToListAsync(cancellationToken);

            var existingNamesSet = new HashSet<string>(existingBreedNames, StringComparer.OrdinalIgnoreCase);

            // Add only NEW breeds (breeds that don't exist yet)
            var newBreeds = externalBreeds
                .Where(breed => !existingNamesSet.Contains(breed.Name))
                .ToList();

            // Insert only the new ones
            if (newBreeds.Any())
            {
                await _dbContext.Breeds.AddRangeAsync(newBreeds, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}






























































































