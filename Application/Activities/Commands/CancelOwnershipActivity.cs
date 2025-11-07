using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Commands;

/// <summary>
/// Command to cancel an active ownership activity.
/// </summary>
/// <remarks>
/// Allows animal owners to cancel their scheduled visits/interactions with animals.
/// Only activities with Active status can be cancelled.
/// </remarks>
public class CancelOwnershipActivity
{
    /// <summary>
    /// Command request containing the activity ID to cancel.
    /// </summary>
    public class Command : IRequest<Result<Activity>>
    {
        /// <summary>
        /// The unique identifier of the activity to cancel.
        /// </summary>
        public string ActivityId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the cancellation of an ownership activity.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Activity>>
    {
        /// <summary>
        /// Executes the cancellation of an active ownership activity.
        /// </summary>
        /// <param name="request">
        /// The command request containing the <see cref="Activity.Id"/> of the activity to cancel.
        /// </param>
        /// <param name="cancellationToken">
        /// Token that signals if the asynchronous operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the updated <see cref="Activity"/> if the cancellation succeeds,
        /// or an error result if validation fails or the operation could not be completed.
        /// </returns>
        public async Task<Result<Activity>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var activity = await GetActivityWithRelations(request.ActivityId, cancellationToken);
            if (activity == null)
                return Result<Activity>.Failure("Activity not found", 404);

            var ownershipValidation = ValidateOwnership(activity, userId);
            if (!ownershipValidation.IsSuccess)
                return ownershipValidation;

            var statusValidation = ValidateStatus(activity);
            if (!statusValidation.IsSuccess)
                return statusValidation;

            activity.Status = ActivityStatus.Cancelled;

            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<Activity>.Failure("Failed to cancel activity", 500);

            return Result<Activity>.Success(activity, 200);
        }

        /// <summary>
        /// Retrieves an activity with all necessary related entities.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The activity with eagerly loaded Animal and User entities, or null if not found.
        /// </returns>
        private async Task<Activity?> GetActivityWithRelations(string activityId, CancellationToken cancellationToken)
        {
            return await context.Activities
                .Include(a => a.Animal)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == activityId, cancellationToken);
        }

        /// <summary>
        /// Validates that the user is authorized to cancel the activity.
        /// </summary>
        /// <param name="activity">The activity to validate ownership for.</param>
        /// <param name="userId">The ID of the user attempting to cancel the activity.</param>
        /// <returns>
        /// A success result if the user owns the activity, or a failure result with an 
        /// appropriate error message and status code.
        /// </returns>
        private static Result<Activity> ValidateOwnership(Activity activity, string userId)
        {
            if (activity.UserId != userId)
                return Result<Activity>.Failure("You are not authorized to cancel this activity", 403);

            if (activity.Type != ActivityType.Ownership)
                return Result<Activity>.Failure("Only ownership activities can be cancelled through this endpoint", 400);

            return Result<Activity>.Success(null!, 200);
        }

        /// <summary>
        /// Validates that the activity is in a valid state to be cancelled.
        /// </summary>
        /// <param name="activity">The activity to validate.</param>
        /// <returns>
        /// A success result if the activity can be cancelled, or a failure result with an 
        /// appropriate error message and status code.
        /// </returns>
        /// <remarks>
        /// Business rule: Only activities with Active status can be cancelled.
        /// Activities that are already Cancelled or Completed cannot be cancelled again.
        /// </remarks>
        private static Result<Activity> ValidateStatus(Activity activity)
        {
            if (activity.Status != ActivityStatus.Active)
            {
                var statusMessage = activity.Status switch
                {
                    ActivityStatus.Cancelled => "This activity has already been cancelled",
                    ActivityStatus.Completed => "Cannot cancel a completed activity",
                    _ => "Activity is not in a valid state to be cancelled"
                };
                return Result<Activity>.Failure(statusMessage, 400);
            }

            return Result<Activity>.Success(null!, 200);
        }
    }
}
