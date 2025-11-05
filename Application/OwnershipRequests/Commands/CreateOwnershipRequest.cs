using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading;

namespace Application.OwnershipRequests.Commands;

/// <summary>
/// Handles the creation of new ownership requests for animals available for adoption.
/// 
/// This command allows authenticated users to submit requests to adopt animals from shelters,
/// automatically calculating the adoption cost and preventing duplicate requests for the same animal.
/// </summary>
public class CreateOwnershipRequest
{
    /// <summary>
    /// Command to create a new ownership request.
    /// </summary>
    public class Command : IRequest<Result<OwnershipRequest>>
    {
        /// <summary>
        /// The unique identifier of the animal for which ownership is being requested.
        /// </summary>
        public string AnimalID { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the creation of ownership requests with validation and duplicate prevention.
    /// </summary>
    public class Handler(
        AppDbContext context, 
        IUserAccessor userAccessor, 
        INotificationService notificationService) : IRequestHandler<Command, Result<OwnershipRequest>>
    {

        /// <summary>
        /// Creates a new ownership request for an animal.
        /// 
        /// This method performs the following operations:
        /// - Extracts the authenticated user's ID from the JWT token
        /// - Validates that the animal exists
        /// - Verifies the animal is available (not inactive or already owned)
        /// - Checks for duplicate requests from the same user for the same animal
        /// - Creates the request with automatic cost calculation from animal data
        /// - Persists the request to the database
        /// - Returns the complete request with loaded Animal and User navigation properties
        /// </summary>
        /// <param name="request">The command containing the animal ID and optional request information.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the created ownership request with full details if successful,
        /// or an error message with appropriate status code if validation fails.
        /// </returns>
        /// <remarks>
        /// The user ID is extracted from the authenticated JWT token, not from the request body,
        /// ensuring users cannot create requests on behalf of others. The adoption amount is
        /// automatically set to match the animal's maintenance cost.
        /// </remarks>
        public async Task<Result<OwnershipRequest>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            // Validate existence of animal
            var animal = await context.Animals.FindAsync(request.AnimalID);
            if (animal == null)
                return Result<OwnershipRequest>.Failure("Animal ID not found", 404);
            
            // Validate if animal is inactive or if it already has an owner
            if (animal.AnimalState == AnimalState.HasOwner || animal.AnimalState == AnimalState.Inactive)
                return Result<OwnershipRequest>.Failure("Animal not available for ownership", 400);
                
            // Check for existing ownership request
            var existingRequest = await context.OwnershipRequests
                .Where(or => or.AnimalId == request.AnimalID
                          && or.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRequest != null)
                return Result<OwnershipRequest>.Failure("User already has a pending ownership request for this animal", 400);

            var ownershipRequest = CreateOwnershipRequest(request.AnimalID, userId, animal.Cost);

            context.OwnershipRequests.Add(ownershipRequest);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<OwnershipRequest>.Failure("Failed to create ownership request", 500);

            var createdRequest = await context.OwnershipRequests
                .Include(or => or.Animal)
                .ThenInclude(a => a.Shelter)
                .Include(or => or.User)
                .FirstOrDefaultAsync(or => or.Id == ownershipRequest.Id, cancellationToken);

            if (createdRequest == null)
                return Result<OwnershipRequest>.Failure("Failed to retrieve created request", 500);

            await NotifyAdmin(createdRequest, cancellationToken);

            return Result<OwnershipRequest>.Success(createdRequest!, 200);
        }

        /// <summary>
        /// Creates a new ownership request entity with initial Pending status.
        /// </summary>
        /// <param name="animalId">The animal being requested for adoption.</param>
        /// <param name="userId">The user making the request.</param>
        /// <param name="amount">The adoption cost (from animal's maintenance cost).</param>
        /// <returns>A new ownership request entity ready to be persisted.</returns>
        private static OwnershipRequest CreateOwnershipRequest(string animalId, string userId, decimal amount)
        {
            return new OwnershipRequest
            {
                AnimalId = animalId,
                UserId = userId,
                Amount = amount,
                Status = OwnershipStatus.Pending
            };
        }

        /// <summary>
        /// Notifies the shelter administrator (AdminCAA) about a new ownership request.
        /// </summary>
        /// <param name="createdRequest">The newly created ownership request with loaded navigation properties (Animal, Shelter, User).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <remarks>
        /// This is a best-effort notification. If the notification fails to send (e.g., admin is offline),
        /// it does not affect the ownership request creation, which has already been persisted to the database.
        /// The notification is only sent if the animal's shelter and admin user exist.
        /// </remarks>
        private async Task NotifyAdmin(OwnershipRequest createdRequest, CancellationToken cancellationToken)
        {
            var userRequestingOwnership = createdRequest.User;
            var animal = createdRequest.Animal;

            if (animal?.Shelter != null && userRequestingOwnership?.Id != null)
            {
                var adminCAA = await context.Users
                    .FirstOrDefaultAsync(u => u.ShelterId == animal.ShelterId, cancellationToken);

                if (adminCAA != null)
                {
                    await notificationService.CreateAndSendToUserAsync(
                        userId: adminCAA.Id,
                        type: NotificationType.NEW_OWNERSHIP_REQUEST,
                        message: $"{userRequestingOwnership.Name} fez um pedido para adotar {animal.Name}",
                        animalId: animal.Id,
                        ownershipRequestId: createdRequest.Id,
                        cancellationToken: cancellationToken
                    );
                }
            }
        }
    }
}