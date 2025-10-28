using Application.Core;
using AutoMapper;
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
        
        public class Handler(AppDbContext dbContext, IMapper mapper) : IRequestHandler<Command, Result<Animal>>
        {
            public async Task<Result<Animal>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Check if the shelter exists
                var shelterExists = await dbContext.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);
                if (!shelterExists)
                    return Result<Animal>.Failure("Shelter not found", 404);

                // Get the animal to be edited
                var animal = await dbContext.Animals
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a =>
                        a.Id == request.AnimalId && a.ShelterId == request.ShelterId,
                        cancellationToken);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found or not owned by this shelter", 404);

                // Check if the breed exists 
                if (!string.IsNullOrEmpty(request.Animal.BreedId))
                {
                    var breedExists = await dbContext.Breeds
                        .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);

                    if (!breedExists)
                        return Result<Animal>.Failure("Breed not found", 404);
                }

                // Update animal properties
                mapper.Map<Animal>(animal);
                
                // Save updated animal
                var success = await dbContext.SaveChangesAsync(cancellationToken) > 0;

                return success
                    ? Result<Animal>.Success(animal, 200)
                    : Result<Animal>.Failure("Failed to update animal", 400);
            }
        }
    }
}
