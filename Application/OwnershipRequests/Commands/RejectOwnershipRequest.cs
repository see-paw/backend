using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

public class RejectOwnershipRequest
{
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string OwnershipRequestId { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class Handler(AppDbContext context, IUserAcessor userAccessor) 
        : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate access token, only Admin CAA is allowed to update the Ownership Request's status
            var currentUser = await userAccessor.GetUserAsync();
            if (currentUser.ShelterId == null)
            {
                return Result<OwnershipRequest>.Failure(
                    "Only shelter administrators can reject ownership requests", 403);
            }

            var ownershipRequest = await context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == request.OwnershipRequestId, cancellationToken);

            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            // Validate if animal belongs to the shelter
            if (ownershipRequest.Animal.ShelterId != currentUser.ShelterId)
            {
                return Result<OwnershipRequest>.Failure(
                    "You can only reject requests for animals in your shelter", 403);
            }

            if (ownershipRequest.Status != OwnershipStatus.Analysing)
                return Result<OwnershipRequest>.Failure("Only requests in 'Analysing' status can be rejected", 400);

            // Update ownership request
            ownershipRequest.Status = OwnershipStatus.Rejected;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.RejectionReason))
                ownershipRequest.RequestInfo = request.RejectionReason;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to reject ownership request", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }
    }
}