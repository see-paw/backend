﻿using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Activities.Commands;

public class CreateOwnershipActivity
{
    public class Command : IRequest<Result<Activity>>
    {
        public string AnimalId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Handler(AppDbContext context, IUserAccessor userAccessor)
        : IRequestHandler<Command, Result<Activity>>
    {
        public async Task<Result<Activity>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var animal = await GetAnimalWithRelations(request.AnimalId, cancellationToken);
            if (animal == null)
                return Result<Activity>.Failure("Animal not found", 404);

            var ownershipValidation = ValidateOwnership(animal, userId);
            if (!ownershipValidation.IsSuccess)
                return ownershipValidation;

            // Validate start date and end date
            var scheduleValidation = ValidateSchedule(request, animal);
            if (!scheduleValidation.IsSuccess)
                return scheduleValidation;

            // Validate conflicts with active activities
            var timeConflictValidation = ValidateTimeConflicts(animal, userId, request.StartDate, request.EndDate);
            if (!timeConflictValidation.IsSuccess)
                return timeConflictValidation;

            //var conflictValidation = ValidateNoConflictWithCompletedActivities(animal, userId, request.StartDate);
            //if (!conflictValidation.IsSuccess)
            //    return conflictValidation;

            var activity = CreateActivity(request, userId);
            context.Activities.Add(activity);

            var success = await context.SaveChangesAsync(cancellationToken) > 0;
            if (!success)
                return Result<Activity>.Failure("Failed to create activity", 500);

            // Load relations for the automapper to work properly
            var createdActivity = await context.Activities
                .Include(a => a.Animal)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == activity.Id, cancellationToken);

            return Result<Activity>.Success(createdActivity!, 201);
        }

        /// <summary>
        /// Retrieves an animal with all necessary related entities for activity creation.
        /// </summary>
        /// <param name="animalId">The unique identifier of the animal.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The animal with eagerly loaded Shelter, OwnershipRequests, and Activities,
        /// or null if not found.
        /// </returns>
        private async Task<Animal?> GetAnimalWithRelations(string animalId, CancellationToken cancellationToken)
        {
            return await context.Animals
                .Include(a => a.Shelter)
                .Include(a => a.OwnershipRequests)
                .Include(a => a.Activities)
                .FirstOrDefaultAsync(a => a.Id == animalId, cancellationToken);
        }

        /// <summary>
        /// Validates that the user is authorized to create activities for the animal.
        /// </summary>
        /// <param name="animal">The animal to validate ownership for.</param>
        /// <param name="userId">The ID of the user attempting to create the activity.</param>
        /// <returns>
        /// A success result if the user is the owner, or a failure result with an 
        /// appropriate error message and status code.
        /// </returns>
        /// <remarks>
        /// Validation checks performed:
        /// - User must be the registered owner of the animal
        /// - Animal must have 'HasOwner' status
        /// - An approved ownership request must exist as a fail-safe
        /// </remarks>
        private static Result<Activity> ValidateOwnership(Animal animal, string userId)
        {
            if (animal.OwnerId != userId)
                return Result<Activity>.Failure("User is not the owner of this animal", 403);

            if (animal.AnimalState != AnimalState.HasOwner)
                return Result<Activity>.Failure("Animal does not have an owner", 400);

            var hasApprovedRequest = animal.OwnershipRequests
                .Any(or => or.UserId == userId && or.Status == OwnershipStatus.Approved);

            if (!hasApprovedRequest)
                return Result<Activity>.Failure("No approved ownership request found", 403);

            return Result<Activity>.Success(null!, 200);
        }

        /// <summary>
        /// Validates scheduling constraints and shelter operating hours.
        /// </summary>
        /// <param name="request">The command containing the requested dates.</param>
        /// <param name="animal">The animal whose shelter hours will be checked.</param>
        /// <returns>
        /// A success result if the schedule is valid, or a failure result with an 
        /// appropriate error message and status code.
        /// </returns>
        /// <remarks>
        /// Validation checks performed:
        /// - Activities must be scheduled at least 24 hours in advance
        /// - End date must be after start date
        /// - Activity times must be within shelter operating hours
        /// </remarks>
        private static Result<Activity> ValidateSchedule(Command request, Animal animal)
        {
            if (request.StartDate < DateTime.UtcNow.AddHours(24))
                return Result<Activity>.Failure("Activities must be scheduled at least 24 hours in advance", 400);

            if (request.EndDate <= request.StartDate)
                return Result<Activity>.Failure("End date must be after start date", 400);

            if (request.EndDate.Date < request.StartDate.Date)
                return Result<Activity>.Failure(
                    "End date must be on the same day or after the start date", 400);

            var startTime = TimeOnly.FromDateTime(request.StartDate);
            var endTime = TimeOnly.FromDateTime(request.EndDate);

            if (startTime < animal.Shelter.OpeningTime || startTime > animal.Shelter.ClosingTime)
                return Result<Activity>.Failure(
                    $"Pick-up time must be within shelter hours ({animal.Shelter.OpeningTime:HH:mm}-{animal.Shelter.ClosingTime:HH:mm})",
                    400);

            if (endTime < animal.Shelter.OpeningTime || endTime > animal.Shelter.ClosingTime)
                return Result<Activity>.Failure(
                    $"Drop-off time must be within shelter hours ({animal.Shelter.OpeningTime:HH:mm}-{animal.Shelter.ClosingTime:HH:mm})",
                    400);

            return Result<Activity>.Success(null!, 200);
        }

        ///// <summary>
        ///// Validates that the new activity does not conflict with previously completed activities.
        ///// </summary>
        ///// <param name="animal">The animal whose activities will be checked.</param>
        ///// <param name="userId">The ID of the user creating the activity.</param>
        ///// <param name="startDate">The proposed start date for the new activity.</param>
        ///// <returns>
        ///// A success result if no conflicts exist, or a failure result with the conflict details.
        ///// </returns>
        ///// <remarks>
        ///// Business rule: New activities must start after the end of the most recent completed activity.
        ///// Cancelled activities are ignored as they did not occur.
        ///// </remarks>
        //private static Result<Activity> ValidateNoConflictWithCompletedActivities(Animal animal, string userId, DateTime startDate)
        //{
        //    var completedActivities = animal.Activities
        //        .Where(a => a.UserId == userId
        //                 && a.Type == ActivityType.Ownership
        //                 && a.Status == ActivityStatus.Completed)
        //        .ToList();

        //    if (completedActivities.Any())
        //    {
        //        var latestEndDate = completedActivities.Max(a => a.EndDate);

        //        if (startDate <= latestEndDate)
        //            return Result<Activity>.Failure(
        //                $"New activity must start after the last completed activity ended at {latestEndDate:yyyy-MM-dd HH:mm}",
        //                400);
        //    }

        //    return Result<Activity>.Success(null!, 200);
        //}

        /// <summary>
        /// Validates that the new activity does not overlap with any active or pending activities.
        /// </summary>
        /// <param name="animal">The animal whose activities will be checked.</param>
        /// <param name="userId">The ID of the user creating the activity.</param>
        /// <param name="startDate">The proposed start date for the new activity.</param>
        /// <param name="endDate">The proposed end date for the new activity.</param>
        /// <returns>
        /// A success result if no time conflicts exist, or a failure result with conflict details.
        /// </returns>
        /// <remarks>
        /// Business rule: Activities cannot overlap in time. 
        /// An overlap occurs when:
        /// - New activity starts during an existing activity
        /// - New activity ends during an existing activity
        /// - New activity completely encompasses an existing activity
        /// - Existing activity completely encompasses the new activity
        /// </remarks>
        private static Result<Activity> ValidateTimeConflicts(Animal animal, string userId, DateTime startDate, DateTime endDate)
        {
            var conflictingActivity = animal.Activities
                .FirstOrDefault(a => a.UserId == userId
                                  && a.Type == ActivityType.Ownership
                                  && a.Status == ActivityStatus.Active
                                  && a.StartDate < endDate        // Existing activity starts before the new one begins
                                  && a.EndDate > startDate);      // Existing activity ends after the new one begins

            if (conflictingActivity != null)
                return Result<Activity>.Failure(
                    $"This activity conflicts with an existing active activity scheduled from {conflictingActivity.StartDate:yyyy-MM-dd HH:mm} to {conflictingActivity.EndDate:yyyy-MM-dd HH:mm}",
                    409);

            return Result<Activity>.Success(null!, 200);
        }

        /// <summary>
        /// Creates a new ownership activity with Active status.
        /// </summary>
        /// <param name="request">The command containing the activity details.</param>
        /// <param name="userId">The ID of the user creating the activity.</param>
        /// <returns>
        /// The newly created activity entity.
        /// </returns>
        private static Activity CreateActivity(Command request, string userId)
        {
            return new Activity
            {
                AnimalId = request.AnimalId,
                UserId = userId,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
        }
    }

}
