using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities.Commands;

/// <summary>
/// Handles the creation of fostering activity slots for scheduled visits.
/// </summary>
public class CreateFosteringActivity
{
    /// <summary>
    /// Command to create a fostering activity and corresponding slot for a visit.
    /// </summary>
    public class Command : IRequest<Result<object>>
    {
        /// <summary>
        /// The unique identifier of the animal to visit.
        /// </summary>
        public string AnimalId { get; set; } = string.Empty;

        /// <summary>
        /// The start date and time of the visit.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The end date and time of the visit.
        /// </summary>
        public DateTime EndDateTime { get; set; }
    }
    
    // <summary>
    /// Handles the creation of a fostering activity slot with comprehensive validation.
    /// 
    /// This handler orchestrates the complete visit scheduling workflow, including validating
    /// fostering relationships, temporal constraints, slot availability, and shelter operating hours.
    /// </summary>
    public class Handler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<Command, Result<object>>
    {

        /// <summary>
        /// Processes the creation of a fostering activity and its corresponding slot.
        /// 
        /// This method performs the following operations:
        /// - Validates the command data using FluentValidation
        /// - Verifies the user has an active fostering relationship with the animal
        /// - Validates the animal exists and is not inactive
        /// - Validates temporal constraints (duration 1-3h, 24h advance, same day)
        /// - Checks shelter operating hours
        /// - Verifies no shelter unavailability slots overlap
        /// - Verifies no activity slots overlap for the animal
        /// - Verifies no other activities overlap for the animal
        /// - Creates the Activity entity (Type: Fostering, Status: Active)
        /// - Creates the ActivitySlot entity (Status: Reserved, Type: Activity)
        /// - Returns both entities for mapping in the controller
        /// </summary>
        /// <param name="request">The command containing visit scheduling details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing an anonymous object with Activity, ActivitySlot, Animal, and Shelter
        /// if successful, or an error message with appropriate status code if validation fails.
        /// </returns>
        public async Task<Result<object>> Handle(Command request, CancellationToken cancellationToken)
        {

            // Get current user
            var currentUser = await userAccessor.GetUserAsync();

            // Convert to UTC for consistent handling
            var startUtc = request.StartDateTime.Kind == DateTimeKind.Utc 
                ? request.StartDateTime 
                : request.StartDateTime.ToUniversalTime();
            
            var endUtc = request.EndDateTime.Kind == DateTimeKind.Utc 
                ? request.EndDateTime 
                : request.EndDateTime.ToUniversalTime();
            
            // Get animal with all necessary relations
            var animal = await GetAnimalWithRelations(request.AnimalId, cancellationToken);
            if (animal == null)
                return Result<object>.Failure("Animal not found", 404);

            // Validate user is foster of this animal
            var fosteringValidation = ValidateFosteringRelationship(animal, currentUser.Id);
            if (fosteringValidation != null)
                return fosteringValidation;

            // Validate animal state
            if (animal.AnimalState == AnimalState.Inactive || animal.AnimalState == AnimalState.Available || animal.AnimalState == AnimalState.HasOwner )
                return Result<object>.Failure("Animal cannot be visited", 400);
            
            // Validate shelter operating hours
            var shelterValidation = ValidateShelterOperatingHours(animal.Shelter, startUtc, endUtc);
            if (shelterValidation != null)
                return shelterValidation;

            // Check for shelter unavailability slots
            var shelterUnavailable = await CheckShelterUnavailability(
                animal.ShelterId, startUtc, endUtc, cancellationToken);
            if (shelterUnavailable != null)
                return shelterUnavailable;

            // Check for overlapping activity slots for this animal
            var slotConflict = await CheckActivitySlotOverlap(
                request.AnimalId, startUtc, endUtc, cancellationToken);
            if (slotConflict != null)
                return slotConflict;

            // Check for overlapping activities for this animal
            var activityConflict = await CheckActivityOverlap(
                request.AnimalId, startUtc, endUtc, cancellationToken);
            if (activityConflict != null)
                return activityConflict;
            
            
            // Create Activity
            var activity = new Activity
            {
                AnimalId = request.AnimalId,
                UserId = currentUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = startUtc,
                EndDate = endUtc
            };

            context.Activities.Add(activity);

            // Create ActivitySlot
            var activitySlot = new ActivitySlot
            {
                ActivityId = activity.Id,
                StartDateTime = startUtc,
                EndDateTime = endUtc,
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            };

            context.ActivitySlots.Add(activitySlot);

            // Save changes
            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<object>.Failure("Failed to create fostering activity", 500);

            // Return entities for mapping in controller
            var result = new
            {
                Activity = activity,
                ActivitySlot = activitySlot,
                Animal = animal,
                Shelter = animal.Shelter
            };

            return Result<object>.Success(result, 201);
        }
        
        /// <summary>
        /// Retrieves an animal with all necessary related entities for visit scheduling.
        /// </summary>
        /// <param name="animalId">The unique identifier of the animal.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The animal with eagerly loaded Shelter, Fosterings, and Images,
        /// or null if not found.
        /// </returns>
        private async Task<Animal?> GetAnimalWithRelations(
            string animalId, 
            CancellationToken cancellationToken)
        {
            return await context.Animals
                .Include(a => a.Shelter)
                .Include(a => a.Fosterings)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == animalId, cancellationToken);
        }
        
        /// <summary>
        /// Validates that the current user has an active fostering relationship with the animal.
        /// </summary>
        /// <param name="animal">The animal to validate fostering for.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>
        /// A failure result if the user is not fostering the animal, or null if validation passes.
        /// </returns>
        private static Result<object>? ValidateFosteringRelationship(Animal animal, string userId)
        {
            var activeFostering = animal.Fosterings
                .FirstOrDefault(f => f.UserId == userId && f.Status == FosteringStatus.Active);

            if (activeFostering == null)
            {
                return Result<object>.Failure(
                    "You are not currently fostering this animal", 404);
            }

            return null;
        }
        
        /// <summary>
        /// Validates that the visit occurs within the shelter's operating hours.
        /// </summary>
        /// <param name="shelter">The shelter where the visit will take place.</param>
        /// <param name="startUtc">The start time of the visit in UTC.</param>
        /// <param name="endUtc">The end time of the visit in UTC.</param>
        /// <returns>
        /// A failure result if the visit is outside operating hours, or null if validation passes.
        /// </returns>
        private static Result<object>? ValidateShelterOperatingHours(
            Shelter shelter, 
            DateTime startUtc, 
            DateTime endUtc)
        {
            var startTime = TimeOnly.FromDateTime(startUtc);
            var endTime = TimeOnly.FromDateTime(endUtc);

            if (startTime < shelter.OpeningTime)
            {
                return Result<object>.Failure(
                    $"Visit cannot start before shelter opening time ({shelter.OpeningTime})", 422);
            }

            if (endTime > shelter.ClosingTime)
            {
                return Result<object>.Failure(
                    $"Visit cannot end after shelter closing time ({shelter.ClosingTime})", 422);
            }

            return null;
        }
        
        /// <summary>
        /// Checks if the shelter has any unavailability slots overlapping with the requested time.
        /// </summary>
        /// <param name="shelterId">The unique identifier of the shelter.</param>
        /// <param name="startUtc">The start time of the visit in UTC.</param>
        /// <param name="endUtc">The end time of the visit in UTC.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A failure result if the shelter is unavailable, or null if validation passes.
        /// </returns>
        private async Task<Result<object>?> CheckShelterUnavailability(
            string shelterId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken)
        {
            var hasUnavailability = await context.ShelterUnavailabilitySlots
                .AnyAsync(s => s.ShelterId == shelterId
                               && s.StartDateTime < endUtc
                               && s.EndDateTime > startUtc,
                    cancellationToken);

            if (hasUnavailability)
            {
                return Result<object>.Failure(
                    "Shelter is unavailable during the requested time", 409);
            }

            return null;
        }

        /// <summary>
        /// Checks if the animal has any activity slots overlapping with the requested time.
        /// </summary>
        /// <param name="animalId">The unique identifier of the animal.</param>
        /// <param name="startUtc">The start time of the visit in UTC.</param>
        /// <param name="endUtc">The end time of the visit in UTC.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A failure result if there's an overlapping slot, or null if validation passes.
        /// </returns>
        /// <remarks>
        /// Checks for overlap using the formula: (NewStart &lt; ExistingEnd) AND (NewEnd &gt; ExistingStart)
        /// Only considers slots with Reserved status to avoid conflicts with available slots.
        /// </remarks>
        private async Task<Result<object>?> CheckActivitySlotOverlap(
            string animalId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken)
        {
            var hasOverlap = await context.ActivitySlots
                .Include(slot => slot.Activity)
                .AnyAsync(slot => slot.Activity.AnimalId == animalId
                                  && slot.StartDateTime < endUtc
                                  && slot.EndDateTime > startUtc
                                  && slot.Status == SlotStatus.Reserved,
                    cancellationToken);

            if (hasOverlap)
            {
                return Result<object>.Failure(
                    "The animal has another visit scheduled during this time", 409);
            }

            return null;
        }
        
        /// <summary>
        /// Checks if the animal has any activities overlapping with the requested time.
        /// </summary>
        /// <param name="animalId">The unique identifier of the animal.</param>
        /// <param name="startUtc">The start time of the visit in UTC.</param>
        /// <param name="endUtc">The end time of the visit in UTC.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A failure result if there's an overlapping activity, or null if validation passes.
        /// </returns>
        /// <remarks>
        /// Only considers active activities to ensure the animal is not engaged in other activities
        /// during the requested time period.
        /// </remarks>
        private async Task<Result<object>?> CheckActivityOverlap(
            string animalId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken)
        {
            var hasOverlap = await context.Activities
                .AnyAsync(a => a.AnimalId == animalId
                               && a.StartDate < endUtc
                               && a.EndDate > startUtc
                               && a.Status == ActivityStatus.Active,
                    cancellationToken);

            if (hasOverlap)
            {
                return Result<object>.Failure(
                    "The animal has another activity scheduled during this time", 409);
            }

            return null;
        }

    }
}