using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Handles the deletion of an existing <see cref="Animal"/> entity
    /// within the context of a specific shelter.
    /// </summary>
    public class DeleteAnimal
    {
        /// <summary>
        /// Command containing the information necessary to delete an animal.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The unique identifier of the animal being deleted.
            /// </summary>
            public required string AnimalId { get; set; }

            /// <summary>
            /// The unique identifier of the shelter where the animal is located.
            /// </summary>
            public required string ShelterId { get; set; }
        }

        /// <summary>
        /// Handles the execution of the <see cref="Command"/> to remove an animal record.
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
            /// Executes the command to delete an existing <see cref="Animal"/>.
            /// Performs validation checks to ensure both the shelter and animal exist
            /// before removing the record from the database.
            /// </summary>
            /// <param name="request">The command containing the animal and shelter identifiers.</param>
            /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> object containing either:
            /// <list type="bullet">
            ///   <item><description>The deleted <see cref="Animal"/> entity (on success).</description></item>
            ///   <item><description>An error message and corresponding status code (on failure).</description></item>
            /// </list>
            /// </returns>
            public async Task<Result<Animal>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Check if the shelter exists
                var shelterExists = await _context.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

                if (!shelterExists)
                    return Result<Animal>.Failure("Shelter not found", 404);

                // Get the animal to be deleted
                var animal = await _context.Animals
                    .FirstOrDefaultAsync(a =>
                        a.Id == request.AnimalId && a.ShelterId == request.ShelterId,
                        cancellationToken);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found or not owned by this shelter", 404);

                if(animal.AnimalState != AnimalState.Available)
                    return Result<Animal>.Failure("Only animals in 'Available' state can be deleted", 400);

                // Remove the animal from the database context
                _context.Animals.Remove(animal);

                // Persist deletion in the database
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<Animal>.Failure("Failed to delete animal", 400);

                // Return the deleted animal entity (for mapping to ResAnimalDto in the controller)
                return Result<Animal>.Success(animal, 200);
            }
        }
    }
}
