using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Handles the editing of an existing <see cref="Animal"/> entity within a specific shelter context.
    /// </summary>
    public class EditAnimal
    {
        /// <summary>
        /// Command containing the information necessary to edit an animal.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The unique identifier of the animal being edited.
            /// </summary>
            public required string AnimalId { get; set; }

            /// <summary>
            /// The unique identifier of the shelter where the animal is located.
            /// </summary>
            public required string ShelterId { get; set; }

            /// <summary>
            /// The animal entity containing all updated biological and adoption attributes.
            /// </summary>
            public required Animal Animal { get; set; }
        }

        /// <summary>
        /// Handles the execution of the <see cref="Command"/> to update an existing animal record.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<Animal>>
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
            /// Executes the command to edit an existing <see cref="Animal"/>.
            /// Performs validation checks for shelter and breed existence,
            /// updates the animal’s attributes and images, and persists the changes.
            /// </summary>
            /// <param name="request">The command containing the animal details and related IDs.</param>
            /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> object containing either:
            /// <list type="bullet">
            /// <item><description>The updated animal (on success).</description></item>
            /// <item><description>An error message and status code (on failure).</description></item>
            /// </list>
            /// </returns>
            public async Task<Result<Animal>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Check if the shelter exists
                var shelterExists = await _context.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);
                if (!shelterExists)
                    return Result<Animal>.Failure("Shelter not found", 404);

                // Get the animal to be edited
                var animal = await _context.Animals
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a =>
                        a.Id == request.AnimalId && a.ShelterId == request.ShelterId,
                        cancellationToken);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found or not owned by this shelter", 404);

                // Check if the breed exists 
                if (!string.IsNullOrEmpty(request.Animal.BreedId))
                {
                    var breedExists = await _context.Breeds
                        .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);

                    if (!breedExists)
                        return Result<Animal>.Failure("Breed not found", 404);
                }

                // Update animal properties
                animal.Name = request.Animal.Name;
                animal.Description = request.Animal.Description;
                animal.AnimalState = request.Animal.AnimalState;
                animal.Colour = request.Animal.Colour;
                animal.Species = request.Animal.Species;
                animal.BreedId = request.Animal.BreedId;
                animal.Size = request.Animal.Size;
                animal.Sex = request.Animal.Sex;
                animal.BirthDate = request.Animal.BirthDate;
                animal.Sterilized = request.Animal.Sterilized;
                animal.Cost = request.Animal.Cost;
                animal.Features = request.Animal.Features;
                animal.UpdatedAt = DateTime.UtcNow;

                // Update images
                if (request.Animal.Images != null && request.Animal.Images.Count > 0)
                {
                    animal.Images.Clear();

                    foreach (var image in request.Animal.Images)
                    {
                        animal.Images.Add(image);
                    }
                }

                // Save updated animal
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<Animal>.Failure("Failed to update animal", 400);

                return Result<Animal>.Success(animal, 200);
            }
        }
    }
}
