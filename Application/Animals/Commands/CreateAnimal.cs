using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
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
<<<<<<< HEAD
            var shelter = await context.Shelters.FindAsync([request.Animal.ShelterId], cancellationToken);

            if (shelter == null)
            {
                return Result<string>.Failure("Shelter not found", 404);
            }

            var breed = await context.Breeds.FindAsync([request.Animal.BreedId], cancellationToken);

            if (breed == null)
            {
                return Result<string>.Failure("Breed not found", 404);
            }

            request.Animal.Breed = breed;
            request.Animal.Shelter = shelter;
=======
            /// <summary>
            /// The animal entity containing all its biological and adoption attributes.
            /// </summary>
            public required Animal Animal { get; set; }
>>>>>>> feature/create-and-list-animals

            /// <summary>
            /// The unique identifier of the shelter where the animal is hosted.
            /// </summary>
            public required string ShelterId { get; set; }

            /// <summary>
            ///  List of images associated with the animal.
            /// </summary>
            public List<Image> Images { get; set; }
        }

<<<<<<< HEAD
            return result ? Result<string>.Success(request.Animal.Id) 
                : Result<string>.Failure("Failed to add the animal", 400);
=======
        /// <summary>
        /// Handles the creation of an <see cref="Animal"/> by validating
        /// related entities (Shelter, Breed) and saving the entity into the database.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<string>>
        {
            private readonly AppDbContext _context;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class
            /// with the provided database context.
            /// </summary>
            /// <param name="context">Entity Framework Core database context.</param>
            public Handler(AppDbContext context)
            {
                _context = context;
            }

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
                var shelterExists = await _context.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

                if (!shelterExists)
                    return Result<string>.Failure("Shelter not found", 404);

                // Validate that the breed exists (if specified)
                if (!string.IsNullOrEmpty(request.Animal.BreedId))
                {
                    var breedExists = await _context.Breeds
                        .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);

                    if (!breedExists)
                        return Result<string>.Failure("Breed not found", 404);
                }

                //Assign the shelter ID to the animal
                request.Animal.ShelterId = request.ShelterId;

                // Associate images with the animal if provided
                if (request.Images != null && request.Images.Any())
                {
                    request.Animal.Images = request.Images;
                }

                // Persist the entity in the database
                _context.Animals.Add(request.Animal);
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<string>.Failure("Failed to create animal", 400);

                // Return success with the new animal ID
                return Result<string>.Success(request.Animal.Id);
            }
>>>>>>> feature/create-and-list-animals
        }
    }
}
