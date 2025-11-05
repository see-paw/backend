using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Commands;

/// <summary>
/// Handles the cancellation of fostering activity visits.
/// </summary>
public class CancelFosteringActivity
{
    /// <summary>
    /// Result returned after successfully cancelling an activity.
    /// </summary>
    public class CancelFosteringActivityResult
    {
        public string ActivityId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to cancel a fostering activity visit.
    /// </summary>
    public class Command : IRequest<Result<CancelFosteringActivityResult>>
    {
        /// <summary>
        /// The unique identifier of the activity to cancel.
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;
    }
    /// <summary>
    /// Handler for cancelling fostering activities with comprehensive validation.
    /// 
    /// This handler performs the following validations:
    /// - Verifies the activity exists
    /// - Validates the activity belongs to the current user
    /// - Validates the activity is of type Fostering
    /// - Validates the activity is in Active status (not already cancelled/completed)
    /// - Verifies the user has an active fostering relationship with the animal
    /// - Validates the associated slot exists
    /// - Ensures the activity hasn't already started
    /// - Updates the Activity status to Cancelled
    /// - Updates the ActivitySlot status to Available
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor) 
        : IRequestHandler<Command, Result<CancelFosteringActivityResult>>
    {
        public async Task<Result<CancelFosteringActivityResult>> Handle(
            Command request, 
            CancellationToken cancellationToken)
        {
            // Get current user
            var currentUser = await userAccessor.GetUserAsync();

            // Get activity with all necessary relations
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
                    $"Cannot cancel an activity with status '{activity.Status}'. Only active activities can be cancelled.", 400);

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
                    $"Cannot cancel a slot with status '{activity.Slot.Status}'. Only reserved slots can be cancelled.", 400);

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