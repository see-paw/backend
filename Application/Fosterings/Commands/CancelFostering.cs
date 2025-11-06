using Application.Core;
using Application.Interfaces;
using Application.Services;
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
        /// <summary>
        /// Represents the command to cancel an active fostering record.
        /// </summary>
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
        public class Handler(AppDbContext context, IFosteringService fosteringService) : IRequestHandler<Command, Result<Fostering>>
        {
            

            /// <summary>
            /// Cancels an active fostering record if it belongs to the authenticated user.
            /// </summary>
            /// <param name="request">The command containing the fostering ID and user ID.</param>
            /// <param name="ct">A token to cancel the operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the updated <see cref="Fostering"/> if successful,  
            /// or an error message with the appropriate status code otherwise.
            /// </returns>
            /// <exception cref="Exception">Thrown if a database error occurs unexpectedly.</exception>
            public async Task<Result<Fostering>> Handle(Command request, CancellationToken ct)
            {
                // Retrieve the fostering record for the authenticated user
                var fostering = await context.Fosterings
                    .Include(f => f.Animal)
                    .ThenInclude(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
                    .FirstOrDefaultAsync(f =>
                        f.Id == request.FosteringId &&
                        f.UserId == request.UserId, ct);

                if (fostering == null)
                    return Result<Fostering>.Failure(
                        "Fostering record not found or does not belong to the authenticated user.",
                        404);

                if (fostering.Status != FosteringStatus.Active)
                    return Result<Fostering>.Failure(
                        "Only active fosterings can be cancelled.",
                        400);

                fostering.Status = FosteringStatus.Cancelled;
                fostering.EndDate = DateTime.UtcNow;
                fostering.UpdatedAt = DateTime.UtcNow;

                fosteringService.UpdateFosteringState(fostering.Animal);

                var success = await context.SaveChangesAsync(ct) > 0;
                if (!success)
                    return Result<Fostering>.Failure(
                        "An error occurred while cancelling the fostering record.",
                        500);

                return Result<Fostering>.Success(fostering, 200);
            }
        }
    }
}
