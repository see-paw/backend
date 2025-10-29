using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Persistence;

namespace Application.OwnershipRequests.Queries;

public class CheckAnimalEligibilityForOwnership
{
    public class Query : IRequest<Result<bool>>
    {
        public required String AnimalId { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<bool>>
    {
        public async Task<Result<bool>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate existence of animal
            var animal = await context.Animals.FindAsync(request.AnimalId);
            if (animal == null)
                return Result<bool>.Failure("Animal ID not found", 404);

            // Validate if animal is inactive or if it already has an owner
            if (animal.AnimalState == AnimalState.HasOwner || animal.AnimalState == AnimalState.Inactive ||
                animal.AnimalState == AnimalState.PartiallyFostered ||
                animal.AnimalState == AnimalState.TotallyFostered)
                return Result<bool>.Failure("Animal not eligible for ownership", 400);
            return Result<bool>.Success(true, 200);
        }
    }
}