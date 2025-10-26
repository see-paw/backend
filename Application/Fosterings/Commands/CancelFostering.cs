using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Fosterings.Commands
{
    /// <summary>
    /// Handles the cancellation of an active fostering record belonging to the authenticated user.
    /// </summary>
    public class CancelFostering
    {
        public class Command : IRequest<Result<Fostering>>
        {
            /// <summary>
            /// The unique identifier of the fostering record to cancel.
            /// </summary>
            public required string FosteringId { get; set; }

            /// <summary>
            /// The unique identifier of the authenticated user performing the cancellation.
            /// </summary>
            public required string UserId { get; set; }
        }

        /// <summary>
        /// Command handler responsible for validating and updating the fostering record.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<Fostering>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context) => _context = context;

            public async Task<Result<Fostering>> Handle(Command request, CancellationToken ct)
            {
                // Retrieve the fostering record for the authenticated user
                var fostering = await _context.Fosterings
                    .Include(f => f.Animal)
                    .FirstOrDefaultAsync(f =>
                        f.Id == request.FosteringId &&
                        f.UserId == request.UserId, ct);

                if (fostering == null)
                    return Result<Fostering>.Failure(
                        "Fostering record not found or does not belong to the authenticated user.",
                        404);

                fostering.Status = FosteringStatus.Cancelled;
                fostering.EndDate = DateTime.UtcNow;
                fostering.UpdatedAt = DateTime.UtcNow;

                var success = await _context.SaveChangesAsync(ct) > 0;
                if (!success)
                    return Result<Fostering>.Failure(
                        "An error occurred while cancelling the fostering record.",
                        500);

                return Result<Fostering>.Success(fostering, 200);
            }
        }
    }
}
