using Application.Core;
using Domain.Enums;
using MediatR;
using Persistence;

namespace Application.Animals.Queries;
/// <summary>
/// Query responsible for verifying whether a given animal is eligible
/// </summary>
/// <remarks>
/// An animal is considered <b>not eligible</b> if:
/// <list type="bullet">
/// <item><description>The animal does not exist in the database.</description></item>
/// <item><description>The animal already has an owner (<see cref="AnimalState.HasOwner"/>).</description></item>
/// <item><description>The animal is inactive (<see cref="AnimalState.Inactive"/>).</description></item>
/// <item><description>The animal is partially fostered (<see cref="AnimalState.PartiallyFostered"/>).</description></item>
/// <item><description>The animal is totally fostered (<see cref="AnimalState.TotallyFostered"/>).</description></item>
/// </list>
/// Otherwise, it is considered eligible for Ownership.
/// </remarks>
public class CheckAnimalEligibilityForOwnership
{
    /// <summary>
    /// Represents the query request used to check animal eligibility.
    /// </summary>
    public class Query : IRequest<Result<bool>>
    {
        /// <summary>
        /// Unique identifier of the animal to be checked.
        /// </summary>
        public required String AnimalId { get; set; }
    }

    /// <summary>
    /// Handles the <see cref="CheckAnimalEligibilityForOwnership.Query"/> logic.
    /// </summary>
    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<bool>>
    {
        
        /// <summary>
        /// Processes the animal eligibility check request.
        /// </summary>
        /// <param name="request">The query containing the animal ID to validate.</param>
        /// <param name="cancellationToken">A token for cancelling the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing:
        /// <list type="bullet">
        /// <item><description><c>Success(true, 200)</c> if the animal is eligible for ownership.</description></item>
        /// <item><description><c>Failure("Animal ID not found", 404)</c> if the animal does not exist.</description></item>
        /// <item><description><c>Failure("Animal not eligible for ownership", 400)</c> if the animal’s state is invalid.</description></item>
        /// </list>
        /// </returns>
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            // 🔍 Check if the animal exists in the database
            var animal = await context.Animals.FindAsync(request.AnimalId);
            if (animal == null)
                return Result<bool>.Failure("Animal ID not found", 404);

            //  Validate if the animal is in a state that makes it ineligible
            if (animal.AnimalState == AnimalState.HasOwner || animal.AnimalState == AnimalState.Inactive ||
                animal.AnimalState == AnimalState.PartiallyFostered ||
                animal.AnimalState == AnimalState.TotallyFostered)
                return Result<bool>.Failure("Animal not eligible for ownership", 400);
            
             //  The animal is eligible for ownership
            return Result<bool>.Success(true, 200);
        }
    }
}