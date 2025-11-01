using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Commands;

/// <summary>
/// Handles the transition of ownership requests from Pending or Rejected status to Analysing status.
/// 
/// This command allows shelter administrators to begin the review process for adoption requests,
/// validating that the animal is still available and the request is in an appropriate state
/// for analysis. This supports the workflow where rejected requests can be reconsidered after
/// users address initial concerns.
/// </summary>
public class UpdateOwnershipRequestStatus
{
    /// <summary>
    /// Command to update an ownership request to Analysing status.
    /// </summary>
    public class Command : IRequest<Result<OwnershipRequest>>
    {

        /// <summary>
        /// The unique identifier of the ownership request to update.
        /// </summary>
        public string OwnershipRequestId { get; set; } = string.Empty;

        /// <summary>
        /// Optional information or notes from the administrator about the analysis process.
        /// This can include details about what aspects are being reviewed or what additional
        /// information is needed.
        /// </summary>
        public string? RequestInfo { get; set; }

    }

    /// <summary>
    /// Handles the status update of ownership requests with comprehensive validation.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<Command, Result<OwnershipRequest>>
    {

        /// <summary>
        /// Updates an ownership request status to Analysing.
        /// 
        /// This method performs the following operations:
        /// - Validates that the requester is a shelter administrator
        /// - Verifies the request exists and belongs to the administrator's shelter
        /// - Ensures the request status allows transition to Analysing (Pending or Rejected only)
        /// - Validates the animal is still available (not owned or inactive)
        /// - Updates the status to Analysing
        /// - Records optional analysis notes from the administrator
        /// - Updates the timestamp to track when analysis began
        /// </summary>
        /// <param name="request">The command containing the ownership request ID and optional analysis notes.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the updated ownership request if successful,
        /// or an error message with appropriate status code if validation fails.
        /// </returns>
        /// <remarks>
        /// This endpoint supports a two-phase workflow:
        /// - Pending → Analysing: Initial review of a new adoption request
        /// - Rejected → Analysing: Reconsideration after user addresses initial concerns
        /// 
        /// Requests cannot be moved to Analysing if they are already Approved or Analysing,
        /// or if the animal has been adopted or made inactive since the request was submitted.
        /// </remarks>
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var currentUser = await userAccessor.GetUserAsync();

            var authValidation = ValidateUserAuthorization(currentUser);
            if (!authValidation.IsSuccess)
                return authValidation;

            var ownershipRequest = await GetOwnershipRequestWithRelations(request.OwnershipRequestId, cancellationToken);
            if (ownershipRequest == null)
                return Result<OwnershipRequest>.Failure("Ownership request not found", 404);

            var shelterValidation = ValidateShelterOwnership(ownershipRequest, currentUser.ShelterId!);
            if (!shelterValidation.IsSuccess)
                return shelterValidation;

            var statusValidation = ValidateRequestTransitionConditions(ownershipRequest);
            if (!statusValidation.IsSuccess)
                return statusValidation;

            var animalValidation = ValidateAnimalState(ownershipRequest.Animal);
            if (!animalValidation.IsSuccess)
                return animalValidation;

            UpdateToAnalysing(ownershipRequest, request.RequestInfo);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to update ownership request status", 500);

            return Result<OwnershipRequest>.Success(ownershipRequest, 200);
        }

        /// <summary>
        /// Retrieves an ownership request with related Animal and User entities.
        /// </summary>
        /// <param name="requestId">The unique identifier of the ownership request.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The ownership request with loaded navigation properties, or null if not found.</returns>
        private async Task<OwnershipRequest?> GetOwnershipRequestWithRelations(string requestId, CancellationToken cancellationToken)
        {
            return await context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == requestId, cancellationToken);
        }

        /// <summary>
        /// Validates that the user is authorized to update ownership requests.
        /// </summary>
        /// <param name="user">The current authenticated user.</param>
        /// <returns>A success result if authorized, or a failure result with 403 status.</returns>
        /// <remarks>
        /// Only users with a ShelterId (shelter administrators) are authorized to update ownership requests.
        /// </remarks>
        private static Result<OwnershipRequest> ValidateUserAuthorization(User user)
        {
            if (user.ShelterId == null)
            {
                return Result<OwnershipRequest>.Failure(
                    "Non Authorized Request: Only shelter administrators can update ownership requests", 403);
            }
            return Result<OwnershipRequest>.Success(null!, 200);
        }

        /// <summary>
        /// Validates that the ownership request's animal belongs to the administrator's shelter.
        /// </summary>
        /// <param name="ownershipRequest">The ownership request to validate.</param>
        /// <param name="userShelterId">The shelter ID of the current user.</param>
        /// <returns>A success result if the animal belongs to the user's shelter, or a failure result with 403 status.</returns>
        private static Result<OwnershipRequest> ValidateShelterOwnership(OwnershipRequest ownershipRequest, string userShelterId)
        {
            if (ownershipRequest.Animal.ShelterId != userShelterId)
            {
                return Result<OwnershipRequest>.Failure(
                    "User can only reject requests for animals in its shelter", 403);
            }
            return Result<OwnershipRequest>.Success(null!, 200);
        }

        /// <summary>
        /// Validates that the ownership request status allows transition to Analysing.
        /// </summary>
        /// <param name="ownershipRequest">The ownership request to validate.</param>
        /// <returns>A success result if the transition is valid, or a failure result otherwise.</returns>
        /// <remarks>
        /// Only Pending and Rejected requests can be moved to Analysing status.
        /// Approved requests cannot be re-analyzed, and requests already in Analysing
        /// status do not need to be updated again.
        /// </remarks>
        private static Result<OwnershipRequest> ValidateRequestTransitionConditions(OwnershipRequest ownershipRequest)
        {
            if (ownershipRequest.Status == OwnershipStatus.Approved ||
                ownershipRequest.Status == OwnershipStatus.Analysing)
            {
                return Result<OwnershipRequest>.Failure(
                    "Only pending or rejected requests can be moved to analysis", 400);
            }
            return Result<OwnershipRequest>.Success(null!, 200);
        }

        /// <summary>
        /// Validates that the animal is available for ownership analysis.
        /// </summary>
        /// <param name="animal">The animal to validate.</param>
        /// <returns>A success result if the animal is available, or a failure result if unavailable.</returns>
        /// <remarks>
        /// Animals with an owner (HasOwner) or that are inactive cannot have their
        /// ownership requests moved to analysis, as they are no longer available for adoption.
        /// </remarks>
        private static Result<OwnershipRequest> ValidateAnimalState(Animal animal)
        {
            if (animal.AnimalState == AnimalState.HasOwner)
            {
                return Result<OwnershipRequest>.Failure(
                    "Cannot analyze request, animal already has an owner", 400);
            }

            if (animal.AnimalState == AnimalState.Inactive)
            {
                return Result<OwnershipRequest>.Failure(
                    "Cannot analyze request, animal is inactive", 400);
            }

            return Result<OwnershipRequest>.Success(null!, 200);
        }

        /// <summary>
        /// Updates the ownership request to Analysing status and records optional analysis notes.
        /// </summary>
        /// <param name="ownershipRequest">The ownership request to update.</param>
        /// <param name="requestInfo">Optional notes from the administrator about the analysis.</param>
        private static void UpdateToAnalysing(OwnershipRequest ownershipRequest, string? requestInfo)
        {
            ownershipRequest.Status = OwnershipStatus.Analysing;
            ownershipRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(requestInfo))
                ownershipRequest.RequestInfo = requestInfo;
        }
    }
}
