using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands
{
    /// <summary>
    /// Handles the deactivation of an existing <see cref="Animal"/> entity.
    /// The deactivation is a logical operation (soft delete), where the record remains
    /// in the database but its state is changed to <see cref="AnimalState.Inactive"/>.
    /// </summary>
    /// <remarks>
    /// This command enforces business rules that prevent the deactivation of animals
    /// currently associated with ownership or fostering processes.
    /// </remarks>
    public class DeactivateAnimal
    {
        /// <summary>
        /// Command containing the required identifiers to perform the deactivation.
        /// </summary>
        public class Command : IRequest<Result<Animal>>
        {
            /// <summary>
            /// The unique identifier of the animal to deactivate.
            /// </summary>
            public required string AnimalId { get; set; }

            /// <summary>
            /// The unique identifier of the shelter that owns the animal.
            /// </summary>
            public required string ShelterId { get; set; }
        }

        /// <summary>
        /// Handles the execution of the <see cref="Command"/>, performing validation,
        /// rule enforcement, and persistence of the updated <see cref="Animal"/> entity.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<Animal>>
        {
            private readonly AppDbContext _context;

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class using
            /// the provided Entity Framework Core database context.
            /// </summary>
            /// <param name="context">Database context for accessing persistent data.</param>
            public Handler(AppDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Executes the deactivation command.
            /// </summary>
            /// <param name="request">The command containing animal and shelter identifiers.</param>
            /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> representing the outcome of the operation:
            /// <list type="bullet">
            ///   <item><description><c>200 OK</c> — The animal was successfully deactivated.</description></item>
            ///   <item><description><c>400 Bad Request</c> — The animal cannot be deactivated because it is owned or fostered.</description></item>
            ///   <item><description><c>404 Not Found</c> — The animal or shelter could not be found.</description></item>
            /// </list>
            /// </returns>
            public async Task<Result<Animal>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Validate that the shelter exists
                var shelterExists = await _context.Shelters
                    .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

                if (!shelterExists)
                    return Result<Animal>.Failure("Shelter not found", 404);

                // Retrieve the animal to deactivate
                var animal = await _context.Animals
                    .FirstOrDefaultAsync(a =>
                        a.Id == request.AnimalId && a.ShelterId == request.ShelterId,
                        cancellationToken);

                if (animal == null)
                    return Result<Animal>.Failure("Animal not found or not owned by this shelter", 404);

                // Ensure the animal can be deactivated according to its current state
                if (animal.AnimalState is AnimalState.HasOwner
                    or AnimalState.PartiallyFostered
                    or AnimalState.TotallyFostered)
                {
                    return Result<Animal>.Failure(
                        "Cannot deactivate an animal that is owned or currently fostered.", 400);
                }

                // Update the animal's state to inactive instead of deleting it
                animal.AnimalState = AnimalState.Inactive;
                animal.UpdatedAt = DateTime.UtcNow;

                var success = await _context.SaveChangesAsync(cancellationToken) > 0;

                if (!success)
                    return Result<Animal>.Failure("Failed to deactivate animal", 400);

                // Return the updated animal entity
                return Result<Animal>.Success(animal, 200);
            }
        }
    }
}
