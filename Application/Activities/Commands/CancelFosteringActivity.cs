using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Commands;

/// <summary>
/// Handles the cancellation of fostering activity visits.
/// 
/// This command allows a user who currently has an active fostering relationship
/// with an animal to cancel a scheduled fostering activity visit, provided that
/// the visit has not yet started and meets all business rule validations.
/// </summary>
public class CancelFosteringActivity
{
    /// <summary>
    /// Represents the result returned after successfully cancelling
    /// a fostering activity.
    /// </summary>
    public class CancelFosteringActivityResult
    {
        /// <summary>
        /// The unique identifier of the cancelled activity.
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;
        
        /// <summary>
        /// A human-readable confirmation message indicating success.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command used to request the cancellation of a fostering activity.
    /// </summary>
    public class Command : IRequest<Result<CancelFosteringActivityResult>>
    {
        /// <summary>
        /// The unique identifier of the activity that should be cancelled.
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the logic for cancelling a fostering activity.
    /// 
    /// This handler performs multiple validation steps to ensure data integrity
    /// and compliance with business rules before cancelling an activity:
    /// <list type="bullet">
    /// <item><description>Verifies that the activity exists in the database</description></item>
    /// <item><description>Checks that the current user owns the activity</description></item>
    /// <item><description>Ensures the activity type is <see cref="ActivityType.Fostering"/></description></item>
    /// <item><description>Validates that the activity is currently active (not cancelled or completed)</description></item>
    /// <item><description>Confirms the user still has an active fostering relationship with the animal</description></item>
    /// <item><description>Ensures the associated slot exists and is reserved</description></item>
    /// <item><description>Prevents cancellation if the activity start time has already passed</description></item>
    /// <item><description>Updates the activity status to <see cref="ActivityStatus.Cancelled"/></description></item>
    /// <item><description>Sets the associated slot status back to <see cref="SlotStatus.Available"/></description></item>
    /// </list>
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<CancelFosteringActivityResult>>
    {
        /// <summary>
        /// Executes the command to cancel a fostering activity visit.
        /// </summary>
        /// <param name="request">The command containing the activity ID to cancel.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing either a success response with activity details
        /// or an appropriate failure message with HTTP status code.
        /// </returns>
        public async Task<Result<CancelFosteringActivityResult>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            // Retrieve the current authenticated user
            var currentUser = await userAccessor.GetUserAsync();

            // Retrieve the activity including its related animal and slot
            var activity = await context.Activities
                .Include(a => a.Animal)
                .ThenInclude(animal => animal.Fosterings)
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == request.ActivityId, cancellationToken);

            if (activity == null)
                return Result<CancelFosteringActivityResult>.Failure("Activity not found", 404);

            // Validate activity belongs to current user
            if (activity.UserId != currentUser.Id)
                return Result<CancelFosteringActivityResult>.Failure(
                    "You are not authorized to cancel this activity", 403);

            // Validate activity is of type Fostering
            if (activity.Type != ActivityType.Fostering)
                return Result<CancelFosteringActivityResult>.Failure(
                    "Only fostering activities can be cancelled through this endpoint", 400);

            // Validate activity is currently active
            if (activity.Status != ActivityStatus.Active)
                return Result<CancelFosteringActivityResult>.Failure(
                    $"Cannot cancel an activity with status '{activity.Status}'. Only active activities can be cancelled.",
                    400);

            // Validate user still has active fostering relationship with the animal
            var activeFostering = activity.Animal.Fosterings
                .FirstOrDefault(f => f.UserId == currentUser.Id && f.Status == FosteringStatus.Active);

            if (activeFostering == null)
                return Result<CancelFosteringActivityResult>.Failure(
                    "You no longer have an active fostering relationship with this animal", 403);

            // Validate the activity slot exists
            if (activity.Slot == null)
                return Result<CancelFosteringActivityResult>.Failure(
                    "Activity slot not found", 404);

            // Validate the slot is currently reserved
            if (activity.Slot.Status != SlotStatus.Reserved)
                return Result<CancelFosteringActivityResult>.Failure(
                    $"Cannot cancel a slot with status '{activity.Slot.Status}'. Only reserved slots can be cancelled.",
                    400);

            // Validate the activity hasn't already started
            var nowUtc = DateTime.UtcNow;
            if (activity.StartDate <= nowUtc)
                return Result<CancelFosteringActivityResult>.Failure(
                    "Cannot cancel an activity that has already started or passed", 400);

            // Update activity status to Cancelled
            activity.Status = ActivityStatus.Cancelled;

            // Update slot status to Available
            activity.Slot.Status = SlotStatus.Available;
            activity.Slot.UpdatedAt = nowUtc;

            // Save changes
            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<CancelFosteringActivityResult>.Failure(
                    "Failed to cancel the activity", 500);

            var result = new CancelFosteringActivityResult
            {
                ActivityId = activity.Id,
                Message = "Visit cancelled successfully"
            };

            return Result<CancelFosteringActivityResult>.Success(result, 200);
        }
    }
}