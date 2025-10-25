using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OwnershipRequests.Commands;

public class UpdateOwnershipRequestStatus
{
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string OwnershipRequestId { get; set; } = string.Empty;
        public OwnershipStatus NewStatus { get; set; }
        public string? RequestInfo { get; set; }

    }

    public class Handler(AppDbContext context, IUserAcessor userAccessor) : IRequestHandler<Command, Result<OwnershipRequest>>
    {

        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate access token, only Admin CAA is allowed to update the Ownership Request's status
            var currentUser = await userAccessor.GetUserAsync();
            if (currentUser.ShelterId == null)
            {
                return Result<OwnershipRequest>.Failure(
                    "Non Authorized Request: Only shelter administrators can update ownership requests", 403);
            }

            // Validate if the the new status is "Analysing"
            if (request.NewStatus != OwnershipStatus.Analysing)
                return Result<OwnershipRequest>.Failure("Invalid status transition", 400);

            var ownershipRequest = await context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == request.OwnershipRequestId, cancellationToken);

            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            // Validate if the animal belongs to the shelter
            if (ownershipRequest.Animal.ShelterId != currentUser.ShelterId)
            {
                return Result<OwnershipRequest>.Failure(
                    "You can only update requests for animals in your shelter", 403);
            }

            if (ownershipRequest.Status != OwnershipStatus.Pending)
                return Result<OwnershipRequest>.Failure("Only pending requests can be moved to analysis", 400);

            ownershipRequest.Status = request.NewStatus;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.RequestInfo))
                ownershipRequest.RequestInfo = request.RequestInfo;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to update ownership request status", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }
    }
}
