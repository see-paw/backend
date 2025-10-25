using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

public class CreateOwnershipRequest
{
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string AnimalID { get; set; } = string.Empty;
        public string? RequestInfo { get; set; }
    }

    public class Handler(AppDbContext context, IUserAcessor userAccessor) : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId(); 

            // Validate existence of animal
            var animal = await context.Animals.FindAsync(request.AnimalID);
            if (animal == null)
                return Result<OwnershipRequest>.Failure("Animal ID not found", 404);

            // Validate if animal is inactive or if it already has an owner
            if (animal.AnimalState == AnimalState.HasOwner || animal.AnimalState == AnimalState.Inactive)
                return Result<OwnershipRequest>.Failure("Animal not available for ownership", 400);

            // Check for existing ownership request
            var existingRequest = await context.OwnershipRequests
                .Where(or => or.AnimalId == request.AnimalID
                          && or.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRequest != null)
                return Result<OwnershipRequest>.Failure("You already have a pending ownership request for this animal", 400);

            var ownershipRequest = new OwnershipRequest
            {
                AnimalId = request.AnimalID,
                UserId = userId,
                Amount = animal.Cost,
                RequestInfo = request.RequestInfo,
                Status = OwnershipStatus.Pending
            };

            context.OwnershipRequests.Add(ownershipRequest);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to create ownership request", 500);

            var createdRequest = await context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == ownershipRequest.Id, cancellationToken);

            return Result<OwnershipRequest>.Success(createdRequest!, 200);
        }
    }
}
