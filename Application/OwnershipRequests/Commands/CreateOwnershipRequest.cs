using Application.Core;
using MediatR;
using Persistence;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.OwnershipRequests.Commands;

public class CreateOwnershipRequest
{
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string AnimalID { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? RequestInfo { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var animal = await _context.Animals.FindAsync(request.AnimalID);
            if (animal == null)
                return Result<OwnershipRequest>.Failure("Animal not found", 404);

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return Result<OwnershipRequest>.Failure("User not found", 404);

            if (animal.AnimalState == AnimalState.HasOwner || animal.AnimalState == AnimalState.Inactive)
                return Result<OwnershipRequest>.Failure("Animal not available for ownership", 400);

            // Check for existing ownership request
            var existingRequest = await _context.OwnershipRequests
                .Where(or => or.AnimalId == request.AnimalID
                          && or.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRequest != null)
                return Result<OwnershipRequest>.Failure("You already have a pending ownership request for this animal", 400);

            var ownershipRequest = new OwnershipRequest
            {
                AnimalId = request.AnimalID,
                UserId = request.UserId,
                Amount = animal.Cost,
                RequestInfo = request.RequestInfo,
                Status = OwnershipStatus.Pending
            };

            _context.OwnershipRequests.Add(ownershipRequest);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to create ownership request", 500);

            var createdRequest = await _context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == ownershipRequest.Id, cancellationToken);

            return Result<OwnershipRequest>.Success(createdRequest!);
        }
    }
}
