using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Handles the editing of an existing <see cref="Animal"/> entity.
    /// </summary>
    public class EditAnimal
    {
        /// <summary>
        /// Command containing the information necessary to edit an animal.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The animal entity containing all updated biological and adoption attributes.
            /// </summary>
            public required Animal Animal { get; set; }
        }
        
        /// <summary>
        /// Handles the command responsible for editing an existing <see cref="Animal"/>.
        /// </summary>
        /// <remarks>
        /// Uses Entity Framework Core to load the target animal and its related entities,  
        /// applies updates using AutoMapper, and saves the changes to the database.
        /// </remarks>
        public class Handler(AppDbContext dbContext, IMapper mapper) : IRequestHandler<Command, Result<Animal>>
        {
            /// <summary>
            /// Updates an existing animal with new data.
            /// </summary>
            /// <param name="request">The command containing the updated animal information.</param>
            /// <param name="ct">A token to cancel the operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the updated <see cref="Animal"/> if successful,  
            /// or an error message with the corresponding status code otherwise.
            /// </returns>
            /// <exception cref="Exception">Thrown if a database operation fails unexpectedly.</exception>
             public async Task<Result<Animal>> Handle(Command request, CancellationToken ct)
             {
                var breed = await dbContext.Breeds.FirstOrDefaultAsync(b => b.Id == request.Animal.BreedId, ct);

                if (breed == null)
                {
                    return Result<Animal>.Failure("Breed not found", 404);
                }
                
                request.Animal.Breed = breed;
                
                var animal = await dbContext.Animals
                    .Include(a => a.Breed)
                    .Include(a => a.Shelter)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == request.Animal.Id, ct);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found or not owned by this shelter", 404);
                
                mapper.Map(request.Animal, animal);
                
                var success = await dbContext.SaveChangesAsync(ct) > 0;

                return success
                    ? Result<Animal>.Success(animal, 200)
                    : Result<Animal>.Failure("Failed to update animal", 400);
            }
        }
        }
    }