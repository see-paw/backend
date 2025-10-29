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
        /// Executes the update process for an existing animal.
        /// </summary>
        /// <param name="request">
        /// The command containing the updated <see cref="Animal"/> data to apply.
        /// </param>
        /// <param name="ct">Cancellation token to handle operation cancellation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the updated <see cref="Animal"/> if successful,
        /// or an error message with a status code otherwise.
        /// </returns>
        /// <remarks>
        /// Steps performed:
        /// 1. Verifies if the specified breed exists.
        /// 2. Loads the current animal with related entities.
        /// 3. Maps updated values using <see cref="AutoMapper"/>.
        /// 4. Saves changes to the database.
        ///
        /// Return codes:
        /// - 200: Animal successfully updated  
        /// - 400: Failed to save changes  
        /// - 404: Breed or animal not found
        /// </remarks>
        public class Handler(AppDbContext dbContext, IMapper mapper) : IRequestHandler<Command, Result<Animal>>
        {
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