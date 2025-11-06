using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

/// <summary>
/// Handles the rejection of ownership requests for animals in shelters.
/// 
/// This command allows shelter administrators to reject adoption requests that do not meet
/// the shelter's criteria, providing an optional reason for the rejection that is visible
/// to the requesting user.
/// </summary>
public class RejectOwnershipRequest
{
    /// <summary>
    /// Command to reject an ownership request.
    /// </summary>
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        /// <summary>
        /// The unique identifier of the ownership request to reject.
        /// </summary>
        public string OwnershipRequestId { get; set; } = string.Empty;

        /// <summary>
        /// Optional explanation for why the ownership request is being rejected.
        /// This message will be visible to the user who submitted the request.
        /// </summary>
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// Handles the rejection of ownership requests with validation and reason tracking.
    /// </summary>
    public class Handler(
        AppDbContext context, 
        IUserAccessor userAccessor,
        INotificationService notificationService) : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        /// <summary>
        /// Rejects an ownership request with an optional reason.
        /// 
        /// This method performs the following operations:
        /// - Validates that the requester is a shelter administrator
        /// - Verifies the request exists and belongs to the administrator's shelter
        /// - Ensures the request is in 'Analysing' status (already under review)
        /// - Updates the request status to Rejected
        /// - Records the rejection reason if provided
        /// - Updates the timestamp to track when the rejection occurred
        /// </summary>
        /// <param name="request">The command containing the ownership request ID and optional rejection reason.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the rejected ownership request if successful,
        /// or an error message with appropriate status code if validation fails.
        /// </returns>
        /// <remarks>
        /// Only ownership requests in 'Analysing' status can be rejected. This ensures that
        /// requests have been properly reviewed before rejection. After rejection, the user
        /// can contact the shelter to address concerns and request re-analysis.
        /// </remarks>
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
                    "User can only reject requests for animals in its shelter", 403);
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

            await NotifyUser(ownershipRequest, cancellationToken);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        /// <summary>
        /// Notifies the user who made the ownership request about the rejection.
        /// </summary>
        /// <param name="ownershipRequest">The rejected ownership request with loaded navigation properties.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <remarks>
        /// Informs the user that their adoption request was rejected. The rejection reason
        /// (if provided) is available in the ownership request details.
        /// </remarks>
        private async Task NotifyUser(OwnershipRequest ownershipRequest, CancellationToken cancellationToken)
        {
            await notificationService.CreateAndSendToUserAsync(
                ownershipRequest.UserId,
                type: NotificationType.OWNERSHIP_REQUEST_REJECTED,
                message: $"O teu pedido para adotar o/a {ownershipRequest.Animal.Name} infelizmente foi recusado.",
                animalId: ownershipRequest.AnimalId,
                ownershipRequestId: ownershipRequest.Id,
                cancellationToken: cancellationToken
                );
        }
    }
}