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
        public string AnimalId { get; set; }

        public DateOnly StartDate { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor,
        FosteringDomainService fosteringDomainService
    ) : IRequestHandler<Query, Result<AnimalWeeklySchedule>>
    {
        public async Task<Result<AnimalWeeklySchedule>> Handle(Query request, CancellationToken ct)
        {
            var user = await userAccessor.GetUserAsync();

            var animal = await dbContext.Animals
                .AsNoTracking()
                .Include(a => a.Shelter)
                .Include(a => a.Activities)
                .Include(a => a.Fosterings)
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

            if (animal == null)
            {
                return Result<AnimalWeeklySchedule>.Failure("Animal not found", 404);
            }

            if (!fosteringDomainService.IsAlreadyFosteredByUser(animal, user.Id))
            {
                return Result<AnimalWeeklySchedule>.Failure("Animal not fostered by user", 404);
            }

            var weekStart = request.StartDate.ToDateTime(TimeOnly.MinValue);
            var weekEnd = request.StartDate.AddDays(7).ToDateTime(TimeOnly.MaxValue);
            var openingHours = animal.Shelter.OpeningTime.ToTimeSpan();
            var closingHours = animal.Shelter.ClosingTime.ToTimeSpan();
            
            var reservedSlots = await dbContext.Set<ActivitySlot>()
                .AsNoTracking()
                .Include(s => s.Activity)
                .ThenInclude(a => a.User)
                .Where(s =>
                    s.Activity.AnimalId == animal.Id &&
                    s.Activity.Status == ActivityStatus.Active &&
                    s.StartDateTime >= weekStart && s.EndDateTime <= weekEnd &&
                    s.StartDateTime.TimeOfDay >= openingHours &&
                    s.EndDateTime.TimeOfDay <= closingHours)
                .ToListAsync(ct);
            
            var unavailableSlots = await dbContext.Set<ShelterUnavailabilitySlot>()
                .AsNoTracking()
                .Include(s => s.Shelter)
                .Where(s =>
                    s.ShelterId == animal.ShelterId &&
                    s.StartDateTime >= weekStart && s.EndDateTime <= weekEnd &&
                    s.Status == SlotStatus.Unavailable)
                .ToListAsync(ct);
            
            var allSlots = reservedSlots.Cast<Slot>().Concat(unavailableSlots);

            var grouped = allSlots
                .GroupBy(s => DateOnly.FromDateTime(s.StartDateTime))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(s => s.StartDateTime).ToList()
                );

            for (var day = request.StartDate; day < request.StartDate.AddDays(7); day = day.AddDays(1))
            {
                if (!grouped.ContainsKey(day))
                {
                    grouped[day] = new List<Slot>();
                }
            }

            var completeWeeklySchedule = new AnimalWeeklySchedule
            {
                Animal = animal,
                Shelter = animal.Shelter,
                WeeklySchedule = grouped
            };

            return Result<AnimalWeeklySchedule>.Success(completeWeeklySchedule, 200);
        }
    }
}