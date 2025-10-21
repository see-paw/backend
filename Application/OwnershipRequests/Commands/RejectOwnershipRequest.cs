using Application.Core;
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

    public class Handler : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var ownershipRequest = await _context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == request.OwnershipRequestId, cancellationToken);

            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            if (ownershipRequest.Status != OwnershipStatus.Analysing)
                return Result<OwnershipRequest>.Failure("Only requests in 'Analysing' status can be rejected", 400);

            // Update ownership request
            ownershipRequest.Status = OwnershipStatus.Rejected;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.RejectionReason))
                ownershipRequest.RequestInfo = request.RejectionReason;

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to reject ownership request", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest);
        }
    }
}