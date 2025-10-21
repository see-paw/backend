using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.Animals.Commands;

/// <summary>
/// Defines the command and handler responsible for creating a new animal record in the database.
/// </summary>
/// <remarks>
/// Implements the MediatR <see cref="IRequest{TResponse}"/> and <see cref="IRequestHandler{TRequest, TResponse}"/>  
/// patterns to separate command logic from controller operations.  
/// The handler validates the related entities (shelter and breed) before persisting the new animal
/// and returns a <see cref="Result{T}"/> containing the created animal’s identifier.
/// </remarks>
public class CreateAnimal
{
    /// <summary>
    /// Represents a command request to create a new animal in the database.
    /// </summary>
    /// <remarks>
    /// Contains the <see cref="Animal"/> entity with all required data for creation.  
    /// Used with MediatR to trigger the <see cref="Handler"/> that performs the creation logic.
    /// </remarks>
    //public class Command : IRequest<Result<string>>
    //{
    //    public required Animal Animal { get; init; }
    //}

    /// <summary>
    /// Handles the <see cref="Command"/> request by creating a new animal record in the database.
    /// </summary>
    /// <remarks>
    /// Validates that the associated <see cref="Shelter"/> and <see cref="Breed"/> exist before creating the animal.  
    /// If both entities are valid, the new <see cref="Animal"/> is added to the database context and persisted.  
    /// Returns a <see cref="Result{T}"/> containing the animal’s unique identifier on success,
    /// or an error message with the appropriate HTTP status code on failure.
    /// </remarks>
    //public class Handler(AppDbContext context) : IRequestHandler<Command, Result<string>>
    //{

        /// <summary>
        /// Processes the <see cref="Command"/> request to create a new animal record in the database.
        /// </summary>
        /// <param name="request">The command containing the animal data to be created.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation if needed.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the created animal’s unique identifier if successful;  
        /// otherwise, a failure result with an appropriate error message and HTTP status code.
        /// </returns>
        /// <remarks>
        /// Validates that the referenced <see cref="Shelter"/> and <see cref="Breed"/> exist.  
        /// If validation passes, assigns the relationships, adds the <see cref="Animal"/> entity to the context,
        /// and saves the changes to the database.  
        /// Returns <c>201 Created</c> on success or an appropriate error response if the operation fails.
        /// </remarks>
    //    public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
    //    {
    //        var shelter = await context.Shelters.FindAsync([request.Animal.ShelterId], cancellationToken);

    //        if (shelter == null)
    //        {
    //            return Result<string>.Failure("Shelter not found", 404);
    //        }

    //        var breed = await context.Breeds.FindAsync([request.Animal.BreedId], cancellationToken);

    //        if (breed == null)
    //        {
    //            return Result<string>.Failure("Breed not found", 404);
    //        }

    //        request.Animal.Breed = breed;
    //        request.Animal.Shelter = shelter;

    //        context.Animals.Add(request.Animal); //Asynchronous version not necessary here

    //        var result = await context.SaveChangesAsync(cancellationToken) > 0;

    //        return result ? Result<string>.Success(request.Animal.Id, 201) 
    //            : Result<string>.Failure("Failed to add the animal", 400);
    //    }
    //}
}