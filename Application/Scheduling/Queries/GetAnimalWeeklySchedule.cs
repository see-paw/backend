using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Scheduling.Queries;


/// <summary>
/// Retrieves the complete weekly schedule for a fostered animal associated with the current user.
/// </summary>
public class GetAnimalWeeklySchedule
{
    /// <summary>
    /// Represents the request parameters for retrieving an animal’s weekly schedule.
    /// </summary>
    public class Query : IRequest<Result<AnimalWeeklySchedule>>
    {
        /// <summary>
        /// The unique identifier of the animal for which the schedule is being requested.
        /// </summary>
        public required string AnimalId { get; init; }

        /// <summary>
        /// The starting date of the week to retrieve the schedule for.
        /// </summary>
        public DateOnly StartDate { get; init; }
    }

    /// <summary>
    /// Handles the logic for generating a complete weekly schedule for a fostered animal.
    /// </summary>
    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        ITimeRangeCalculator timeRangeCalculator,
        IScheduleAssembler scheduleAssembler,
        ISlotNormalizer slotNormalizer
    ) : IRequestHandler<Query, Result<AnimalWeeklySchedule>>
    {
        /// <summary>
        /// Executes the retrieval and assembly of an animal’s weekly schedule.
        /// </summary>
        /// <param name="request">The <see cref="Query"/> containing the animal ID and start date.</param>
        /// <param name="ct">A cancellation token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the <see cref="AnimalWeeklySchedule"/> if successful,  
        /// or an error result with a message and status code if validation or data retrieval fails.
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="number">
        /// <item>Retrieves the current authenticated user.</item>
        /// <item>Loads the specified animal and validates that it exists.</item>
        /// <item>Verifies that the animal is fostered by the requesting user.</item>
        /// <item>Retrieves all relevant activity and unavailability slots for the week.</item>
        /// <item>Normalizes the slots to align with the shelter’s opening and closing hours.</item>
        /// <item>Calculates available time ranges using <see cref="ITimeRangeCalculator"/>.</item>
        /// <item>Assembles the final weekly schedule using <see cref="IScheduleAssembler"/>.</item>
        /// </list>
        /// Returns:
        /// <list type="bullet">
        /// <item><b>200</b> – Schedule successfully generated.</item>
        /// <item><b>404</b> – Animal not found.</item>
        /// <item><b>409</b> – Animal not fostered by the user.</item>
        /// </list>
        /// </remarks>
        public async Task<Result<AnimalWeeklySchedule>> Handle(Query request, CancellationToken ct)
        {
            var user = await userAccessor.GetUserAsync();

            var animal = await dbContext.Animals
                .AsNoTracking()
                .Include(a => a.Shelter)
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

            if (animal == null)
            {
                return Result<AnimalWeeklySchedule>.Failure("Animal not found", 404);
            }
            
            var hasFostering = await dbContext.Fosterings
                .AnyAsync(f => f.AnimalId == animal.Id 
                               && f.UserId == user.Id 
                               && f.Status == FosteringStatus.Active, ct);

            if (!hasFostering)
            {
                return Result<AnimalWeeklySchedule>.Failure("Animal not fostered by user", 409);
            }

            var weekStart = request.StartDate.ToDateTime(TimeOnly.MinValue);
            var weekEnd = request.StartDate.AddDays(7).ToDateTime(TimeOnly.MinValue);
            var opening = animal.Shelter.OpeningTime.ToTimeSpan();
            var closing = animal.Shelter.ClosingTime.ToTimeSpan();
            
            if (opening >= closing) throw new ArgumentException("Opening must be before closing");

            var reservedSlots = await dbContext.Set<ActivitySlot>()
                .AsNoTracking()
                .Include(s => s.Activity)
                .ThenInclude(a => a.User) 
                .Where(s => s.Activity.AnimalId == animal.Id
                            && s.Activity.Status == ActivityStatus.Active
                            && s.StartDateTime < weekEnd
                            && s.EndDateTime > weekStart)
                .ToListAsync(ct);

            var unavailableSlots = await dbContext.Set<ShelterUnavailabilitySlot>()
                .AsNoTracking()
                .Where(s => s.ShelterId == animal.ShelterId
                            && s.Status == SlotStatus.Unavailable
                            && s.StartDateTime < weekEnd
                            && s.EndDateTime > weekStart)
                .ToListAsync(ct);

            var grouped = reservedSlots.Concat<Slot>(unavailableSlots);

            var normalizedSlots = slotNormalizer.Normalize(grouped, opening, closing);
            
            var normalizedReserved = normalizedSlots.Slots.OfType<ActivitySlot>().ToList();
            var normalizedUnavailable = normalizedSlots.Slots.OfType<ShelterUnavailabilitySlot>().ToList();

            var available = timeRangeCalculator.CalculateWeeklyAvailableRanges(normalizedSlots.Slots, opening, closing, DateOnly.FromDateTime(weekStart));

            var sched = scheduleAssembler.AssembleWeekSchedule(
                normalizedReserved, normalizedUnavailable, available, animal, request.StartDate);

            return Result<AnimalWeeklySchedule>.Success(sched, 200);
        }
    }
}