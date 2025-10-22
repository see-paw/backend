using Application.Core;
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

    public class Handler : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.NewStatus != OwnershipStatus.Analysing)
                return Result<OwnershipRequest>.Failure("Invalid status transition", 400);

            var ownershipRequest = await _context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == request.OwnershipRequestId, cancellationToken);

            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            if (ownershipRequest.Status != OwnershipStatus.Pending)
                return Result<OwnershipRequest>.Failure("Only pending requests can be moved to analysis", 400);

            ownershipRequest.Status = request.NewStatus;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.RequestInfo))
                ownershipRequest.RequestInfo = request.RequestInfo;

            var success = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to update ownership request status", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest);
        }
    }
}
