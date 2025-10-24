using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

public class ApproveOwnershipRequest
{
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string OwnershipRequestId { get; set; } = string.Empty;
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
            var ownershipRequest = await GetOwnershipRequestWithRelations(request.OwnershipRequestId, cancellationToken);
            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            var validationResult = ValidateApprovalConditions(ownershipRequest, request.OwnershipRequestId);
            if (!validationResult.IsSuccess)
                return validationResult;

            ApproveRequest(ownershipRequest);
            UpdateAnimalToOwned(ownershipRequest);
            CancelActiveFosterings(ownershipRequest.Animal);
            RejectOtherPendingRequests(ownershipRequest.Animal, request.OwnershipRequestId);

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to approve ownership request", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        private async Task<OwnershipRequest?> GetOwnershipRequestWithRelations(string requestId, CancellationToken cancellationToken)
        {
            return await _context.OwnershipRequests
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Fosterings)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.OwnershipRequests)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == requestId, cancellationToken);
        }

        private static Result<OwnershipRequest> ValidateApprovalConditions(OwnershipRequest ownershipRequest, string requestId)
        {
            if (ownershipRequest.Animal.AnimalState == AnimalState.Inactive)
                return Result<OwnershipRequest>.Failure("Animal is inactive", 400);

            if (ownershipRequest.Animal.AnimalState == AnimalState.HasOwner)
                return Result<OwnershipRequest>.Failure("Animal already has an owner", 400);

            var hasApprovedRequest = ownershipRequest.Animal.OwnershipRequests
                .Any(or => or.Status == OwnershipStatus.Approved && or.Id != requestId);

            if (hasApprovedRequest)
                return Result<OwnershipRequest>.Failure("Animal already has an approved ownership request", 400);

            if (ownershipRequest.Status != OwnershipStatus.Analysing && ownershipRequest.Status != OwnershipStatus.Rejected)
                return Result<OwnershipRequest>.Failure("Only requests in 'Analysing' or 'Rejected' status can be approved", 400);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        private static void ApproveRequest(OwnershipRequest ownershipRequest)
        {
            ownershipRequest.Status = OwnershipStatus.Approved;
            ownershipRequest.ApprovedAt = DateTime.UtcNow;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;
        }

        private static void UpdateAnimalToOwned(OwnershipRequest ownershipRequest)
        {
            ownershipRequest.Animal.OwnerId = ownershipRequest.UserId;
            ownershipRequest.Animal.OwnershipStartDate = DateTime.UtcNow;
            ownershipRequest.Animal.AnimalState = AnimalState.HasOwner;
            ownershipRequest.Animal.UpdatedAt = DateTime.UtcNow;
        }

        private static void CancelActiveFosterings(Animal animal)
        {
            var activeFosterings = animal.Fosterings
                .Where(f => f.Status == FosteringStatus.Active)
                .ToList();

            foreach (var fostering in activeFosterings)
            {
                fostering.Status = FosteringStatus.Cancelled;
                fostering.EndDate = DateTime.UtcNow;
                fostering.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static void RejectOtherPendingRequests(Animal animal, string approvedRequestId)
        {
            var otherRequests = animal.OwnershipRequests
                .Where(or => or.Id != approvedRequestId
                          && (or.Status == OwnershipStatus.Pending || or.Status == OwnershipStatus.Analysing))
                .ToList();

            foreach (var otherRequest in otherRequests)
            {
                otherRequest.Status = OwnershipStatus.Rejected;
                otherRequest.UpdatedAt = DateTime.UtcNow;
                otherRequest.RequestInfo = "Automatically rejected - another ownership request was approved";
            }
        }
    }
}