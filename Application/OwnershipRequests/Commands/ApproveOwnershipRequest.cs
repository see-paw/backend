using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

/// <summary>
/// Handles the approval of ownership requests for animals in shelters.
/// 
/// This command orchestrates the complete approval workflow, including validating permissions,
/// updating the animal's ownership status, canceling active fosterings, and automatically
/// rejecting competing ownership requests for the same animal.
/// </summary>
public class ApproveOwnershipRequest
{
    /// <summary>
    /// Command to approve an ownership request.
    /// </summary>
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        public string OwnershipRequestId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the approval of an ownership request with comprehensive validation and side effects.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<Command, Result<OwnershipRequest>>
    {
        /// <summary>
        /// Processes the approval of an ownership request.
        /// 
        /// This method performs the following operations:
        /// - Validates that the requester is a shelter administrator
        /// - Verifies the request exists and belongs to the administrator's shelter
        /// - Validates approval conditions (animal state, request status, etc.)
        /// - Approves the request and updates timestamps
        /// - Transfers animal ownership to the requesting user
        /// - Cancels any active fostering agreements for the animal
        /// - Automatically rejects other pending or analyzing requests for the same animal
        /// </summary>
        /// <param name="request">The command containing the ownership request ID to approve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the approved ownership request if successful, or an error message with appropriate status code if validation fails.
        /// </returns>
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUser = await userAccessor.GetUserAsync();
            if (currentUser.ShelterId == null)
            {
                return Result<OwnershipRequest>.Failure(
                    "Only shelter administrators can approve ownership requests", 403);
            }

            var ownershipRequest = await GetOwnershipRequestWithRelations(request.OwnershipRequestId, cancellationToken);
            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            if (ownershipRequest.Animal.ShelterId != currentUser.ShelterId)
            {
                return Result<OwnershipRequest>.Failure(
                    "You can only approve requests for animals in your shelter", 403);
            }

            var validationResult = ValidateApprovalConditions(ownershipRequest, request.OwnershipRequestId);
            if (!validationResult.IsSuccess)
                return validationResult;

            ApproveRequest(ownershipRequest);
            UpdateAnimalToOwned(ownershipRequest);
            CancelActiveFosterings(ownershipRequest.Animal);
            RejectOtherPendingRequests(ownershipRequest.Animal, request.OwnershipRequestId);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to approve ownership request", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        /// <summary>
        /// Retrieves an ownership request with all necessary related entities for approval processing.
        /// </summary>
        /// <param name="requestId">The unique identifier of the ownership request.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The ownership request with eagerly loaded Animal (including Fosterings and OwnershipRequests) and User,
        /// or null if not found.
        /// </returns>
        private async Task<OwnershipRequest?> GetOwnershipRequestWithRelations(string requestId, CancellationToken cancellationToken)
        {
            return await context.OwnershipRequests
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Fosterings)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.OwnershipRequests)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == requestId, cancellationToken);
        }

        /// <summary>
        /// Validates all business rules and conditions required for approving an ownership request.
        /// </summary>
        /// <param name="ownershipRequest">The ownership request to validate.</param>
        /// <param name="requestId">The ID of the request being validated (used to exclude self in duplicate checks).</param>
        /// <returns>
        /// A success result if all conditions are met, or a failure result with an appropriate error message and status code.
        /// </returns>
        /// <remarks>
        /// Validation checks performed:
        /// - Animal must not be inactive
        /// - Animal must not already have an owner
        /// - No other approved ownership request should exist for the animal
        /// - Request status must be 'Analysing'
        /// </remarks>
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

            if (ownershipRequest.Status != OwnershipStatus.Analysing)
                return Result<OwnershipRequest>.Failure("Only requests in 'Analysing' status can be approved", 400);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        /// <summary>
        /// Updates the ownership request to approved status and sets approval timestamps.
        /// </summary>
        /// <param name="ownershipRequest">The ownership request to approve.</param>
        private static void ApproveRequest(OwnershipRequest ownershipRequest)
        {
            ownershipRequest.Status = OwnershipStatus.Approved;
            ownershipRequest.ApprovedAt = DateTime.UtcNow;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Transfers ownership of the animal to the requesting user and updates the animal's state.
        /// </summary>
        /// <param name="ownershipRequest">The approved ownership request containing the user and animal information.</param>
        private static void UpdateAnimalToOwned(OwnershipRequest ownershipRequest)
        {
            ownershipRequest.Animal.OwnerId = ownershipRequest.UserId;
            ownershipRequest.Animal.OwnershipStartDate = DateTime.UtcNow;
            ownershipRequest.Animal.AnimalState = AnimalState.HasOwner;
            ownershipRequest.Animal.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancels all active fostering agreements for the specified animal.
        /// </summary>
        /// <param name="animal">The animal whose fosterings should be cancelled.</param>
        /// <remarks>
        /// When an animal is adopted (ownership approved), any active fostering agreements
        /// must be terminated as the animal will no longer be available for fostering.
        /// </remarks>
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

        /// <summary>
        /// Automatically rejects all other pending or analyzing ownership requests for the same animal.
        /// </summary>
        /// <param name="animal">The animal whose competing requests should be rejected.</param>
        /// <param name="approvedRequestId">The ID of the approved request to exclude from rejection.</param>
        /// <remarks>
        /// Since only one ownership request can be approved per animal, all competing requests
        /// are automatically rejected to prevent conflicts and maintain data integrity.
        /// </remarks>
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