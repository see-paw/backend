using Application.Core;
using Application.Images.Commands;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Command and handler responsible for creating a new <see cref="Animal"/> entity
    /// in the database, ensuring all related entities (e.g., Shelter, Breed) exist
    /// before persisting the record.
    /// </summary>
    public class CreateAnimal
    {
        /// <summary>
        /// Represents the command request to create a new animal.
        /// Implements <see cref="IRequest{TResponse}"/> returning a <see cref="Result{T}"/> 
        /// with the created animal's unique identifier.
        /// </summary>
        public class Command : IRequest<Result<string>>
        {
            /// <summary>
            /// The animal entity containing all its biological and adoption attributes.
            /// </summary>
            public required Animal Animal { get; set; }

            /// <summary>
            /// The unique identifier of the shelter where the animal is hosted.
            /// </summary>
            public required string ShelterId { get; set; }
            
            /// <summary>
            /// The metadata of the images associated with the animal.
            /// </summary>
            public required List<Image> Images { get; set; }  
            
            /// <summary>
            /// The actual uploaded image files.
            /// </summary>
            public required List<IFormFile> Files { get; set; } 
        }

        /// <summary>
        /// Handles the creation of an <see cref="Animal"/> by validating
        /// related entities (Shelter, Breed) and saving the entity into the database.
        /// </summary>
        public class Handler(AppDbContext dbContext, IMediator mediator) : IRequestHandler<Command, Result<string>>
        {
            /// <summary>
            /// Executes the command to create a new <see cref="Animal"/>.
            /// Performs validation checks for shelter and breed existence,
            /// associates images if provided, and persists the record.
            /// </summary>
            /// <param name="request">The command containing the animal details and related IDs.</param>
            /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> object containing either:
            /// - The unique ID of the created animal (on success), or
            /// - An error message and status code (on failure).
            /// </returns>
            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate that the shelter exists
                var shelterExists = await dbContext.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

                if (!shelterExists)
                    return Result<string>.Failure("Shelter not found", 404);

                // Validate that the breed exists (if specified)
                if (!string.IsNullOrEmpty(request.Animal.BreedId))
                {
                    var breedExists = await dbContext.Breeds
                        .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);

                    if (!breedExists)
                        return Result<string>.Failure("Breed not found", 404);
                }

                //Assign the shelter ID to the animal
                request.Animal.ShelterId = request.ShelterId;

                // Persist the entity in the database
                dbContext.Animals.Add(request.Animal);
                var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<string>.Failure("Failed to create animal", 400);
                
                for (var i = 0; i < request.Files.Count; i++)
                {
                    var file = request.Files[i];
                    var imageMeta = request.Images[i];

                    var addImageCommand = new AddImage<Animal>.Command
                    {
                        EntityId = request.Animal.Id,
                        File = file,
                        Description = imageMeta.Description ?? string.Empty,
                        IsPrincipal = imageMeta.IsPrincipal
                    };

                    var imageResult = await mediator.Send(addImageCommand, cancellationToken);

                    if (!imageResult.IsSuccess)
                        return Result<string>.Failure($"Image upload failed: {imageResult.Error}", 400);
                }
                
                // Return success with the new animal ID
                return Result<string>.Success(request.Animal.Id, 201);
            }
        }
    }
}