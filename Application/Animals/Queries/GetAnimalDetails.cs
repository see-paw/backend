using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Queries;

/// <summary>
/// Defines the query and handler responsible for retrieving detailed information about a specific animal.
/// </summary>
/// <remarks>
/// Implements the MediatR <see cref="IRequest{TResponse}"/> and <see cref="IRequestHandler{TRequest, TResponse}"/>  
/// patterns to separate query logic from controller responsibilities.  
/// The handler fetches an animal by its unique identifier, including related breed and image data,
/// and returns a <see cref="Result{T}"/> indicating success or failure.
/// </remarks>
public class GetAnimalDetails
{
    /// <summary>
    /// Represents a request to retrieve detailed information about a specific animal.
    /// </summary>
    /// <remarks>
    /// Contains the unique identifier (<c>Id</c>) of the animal to be fetched from the database.  
    /// Used with MediatR to trigger the <see cref="Handler"/> that performs the retrieval.
    /// </remarks>
    public class Query : IRequest<Result<Animal>>
    {
        /// <summary>
        /// The unique identifier of the animal to retrieve.
        /// </summary>
        public required string Id { get; set; }
    }

    /// <summary>
    /// Handles the <see cref="Query"/> request by retrieving the corresponding animal from the database.
    /// </summary>
    /// <remarks>
    /// Uses Entity Framework Core to fetch the animal matching the specified <c>Id</c>,  
    /// including its related <see cref="Breed"/> and <see cref="Image"/> entities.  
    /// Returns a <see cref="Result{T}"/> indicating success if the animal is found and retrievable,
    /// or failure if the animal does not exist or is in an unavailable state.
    /// </remarks>
    public class Handler(AppDbContext dbContext) : IRequestHandler<Query, Result<Animal>>
    {
        /// <summary>
        /// Processes the <see cref="Query"/> request and retrieves the specified animal from the database.
        /// </summary>
        /// <param name="request">The query containing the animal's unique identifier.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation if required.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the requested <see cref="Animal"/> if found and available;  
        /// otherwise, a failure result with an appropriate error message and HTTP status code.
        /// </returns>
        /// <remarks>
        /// Executes an asynchronous database query using Entity Framework Core to fetch the animal,
        /// including its related <c>Breed</c> and <c>Images</c>.  
        /// Returns a failure result if the animal does not exist or is not in a retrievable state
        /// (i.e., not <c>Available</c> or <c>PartiallyFostered</c>).
        /// </remarks>
        public async Task<Result<Animal>> Handle(Query request, CancellationToken cancellationToken)
        {
            var animal = await dbContext.Animals
                .Include(x => x.Breed)
                .Include(x => x.Images)
                .Include(x => x.Fosterings)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (animal == null)
            {
                return Result<Animal>.Failure("Animal not found", 404);
            }

            if (animal.AnimalState != AnimalState.Available
                && animal.AnimalState != AnimalState.PartiallyFostered)
            {
                return Result<Animal>.Failure("Animal not retrievable", 404);
            }

            return Result<Animal>.Success(animal, 200);
        }
    }
}

