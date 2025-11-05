using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Scheduling.Queries;

public class GetAnimalWeeklySchedule
{
    public class Query : IRequest<Result<AnimalWeeklySchedule>>
    {
        public string AnimalId { get; init; }

        public DateOnly StartDate { get; init; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        ITimeRangeCalculator timeRangeCalculator,
        IScheduleAssembler scheduleAssembler,
        ISlotNormalizer slotNormalizer
    ) : IRequestHandler<Query, Result<AnimalWeeklySchedule>>
    {
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