using Domain;
using Domain.Enums;

namespace Persistence.Seeds;

internal static class SlotsSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Set<ActivitySlot>().Any() && !dbContext.Set<ShelterUnavailabilitySlot>().Any())
        {
            var baseDate = new DateTime(2025, 11, 3, 0, 0, 0, DateTimeKind.Utc);
            var twoDaysFromNow = DateTime.UtcNow.Date.AddDays(2);
            var now = DateTime.UtcNow;

            var activitySlots = new List<ActivitySlot>();
            activitySlots.AddRange(GetMainActivitySlots(baseDate));
            activitySlots.AddRange(GetCancelFosteringTestSlots(twoDaysFromNow));
            activitySlots.AddRange(GetFosteringsTestSlots(now));

            await dbContext.Set<ActivitySlot>().AddRangeAsync(activitySlots);

            var shelterUnavailabilitySlots = GetShelterUnavailabilitySlots(baseDate);
            await dbContext.Set<ShelterUnavailabilitySlot>().AddRangeAsync(shelterUnavailabilitySlots);

            await dbContext.SaveChangesAsync();
        }
    }

    private static List<ActivitySlot> GetMainActivitySlots(DateTime baseDate)
    {
        return new List<ActivitySlot>
        {
            new()
            {
                Id = SeedConstants.SlotNormal1,
                ActivityId = SeedConstants.ActivityAId,
                StartDateTime = baseDate.AddDays(1).AddHours(10),
                EndDateTime = baseDate.AddDays(1).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotNormal2,
                ActivityId = SeedConstants.ActivityBId,
                StartDateTime = baseDate.AddDays(1).AddHours(14),
                EndDateTime = baseDate.AddDays(1).AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotNormal3,
                ActivityId = SeedConstants.ActivityCId,
                StartDateTime = baseDate.AddDays(2).AddHours(9).AddMinutes(30),
                EndDateTime = baseDate.AddDays(2).AddHours(11).AddMinutes(30),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotNormal4,
                ActivityId = SeedConstants.ActivityDId,
                StartDateTime = baseDate.AddDays(3).AddHours(13),
                EndDateTime = baseDate.AddDays(3).AddHours(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotNormal5,
                ActivityId = SeedConstants.ActivityEId,
                StartDateTime = baseDate.AddDays(4).AddHours(10),
                EndDateTime = baseDate.AddDays(4).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotEdgeStartBefore,
                ActivityId = SeedConstants.ActivityFId,
                StartDateTime = baseDate.AddDays(5).AddHours(7),
                EndDateTime = baseDate.AddDays(5).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotEdgeEndAfter,
                ActivityId = SeedConstants.ActivityGId,
                StartDateTime = baseDate.AddDays(5).AddHours(17),
                EndDateTime = baseDate.AddDays(5).AddHours(20),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotEdgeExactOpen,
                ActivityId = SeedConstants.ActivityHId,
                StartDateTime = baseDate.AddDays(6).AddHours(9),
                EndDateTime = baseDate.AddDays(6).AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotEdgeExactClose,
                ActivityId = SeedConstants.ActivityIId,
                StartDateTime = baseDate.AddDays(6).AddHours(16),
                EndDateTime = baseDate.AddDays(6).AddHours(18),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotMultiDay1,
                ActivityId = SeedConstants.ActivityJId,
                StartDateTime = baseDate.AddDays(7).AddHours(15),
                EndDateTime = baseDate.AddDays(8).AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotMultiDay2,
                ActivityId = SeedConstants.ActivityKId,
                StartDateTime = baseDate.AddDays(9).AddHours(16),
                EndDateTime = baseDate.AddDays(10).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotShort1,
                ActivityId = SeedConstants.ActivityAId,
                StartDateTime = baseDate.AddDays(11).AddHours(11),
                EndDateTime = baseDate.AddDays(11).AddHours(11).AddMinutes(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotShort2,
                ActivityId = SeedConstants.ActivityBId,
                StartDateTime = baseDate.AddDays(11).AddHours(14).AddMinutes(30),
                EndDateTime = baseDate.AddDays(11).AddHours(14).AddMinutes(45),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotOverlap1,
                ActivityId = SeedConstants.ActivityCId,
                StartDateTime = baseDate.AddDays(12).AddHours(10),
                EndDateTime = baseDate.AddDays(12).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotOverlap2,
                ActivityId = SeedConstants.ActivityCId,
                StartDateTime = baseDate.AddDays(12).AddHours(11),
                EndDateTime = baseDate.AddDays(12).AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotSameDay1,
                ActivityId = SeedConstants.ActivityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(9),
                EndDateTime = baseDate.AddDays(13).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotSameDay2,
                ActivityId = SeedConstants.ActivityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(11),
                EndDateTime = baseDate.AddDays(13).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotSameDay3,
                ActivityId = SeedConstants.ActivityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(15),
                EndDateTime = baseDate.AddDays(13).AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<ActivitySlot> GetCancelFosteringTestSlots(DateTime twoDaysFromNow)
    {
        return new List<ActivitySlot>
        {
            new()
            {
                Id = SeedConstants.SlotC1,
                ActivityId = SeedConstants.ActivityC1Id,
                StartDateTime = twoDaysFromNow.AddHours(10),
                EndDateTime = twoDaysFromNow.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotC2,
                ActivityId = SeedConstants.ActivityC2Id,
                StartDateTime = twoDaysFromNow.AddHours(14),
                EndDateTime = twoDaysFromNow.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotC3,
                ActivityId = SeedConstants.ActivityC3Id,
                StartDateTime = twoDaysFromNow.AddHours(10),
                EndDateTime = twoDaysFromNow.AddHours(12),
                Status = SlotStatus.Available,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotC4,
                ActivityId = SeedConstants.ActivityC4Id,
                StartDateTime = DateTime.UtcNow.AddDays(-2),
                EndDateTime = DateTime.UtcNow.AddDays(-2).AddHours(2),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = SeedConstants.SlotC5,
                ActivityId = SeedConstants.ActivityC5Id,
                StartDateTime = DateTime.UtcNow.AddHours(-1),
                EndDateTime = DateTime.UtcNow.AddHours(1),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = SeedConstants.SlotC6,
                ActivityId = SeedConstants.ActivityC6Id,
                StartDateTime = twoDaysFromNow.AddHours(10),
                EndDateTime = twoDaysFromNow.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotC7,
                ActivityId = SeedConstants.ActivityC7Id,
                StartDateTime = twoDaysFromNow,
                EndDateTime = twoDaysFromNow.AddMonths(1),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.SlotC8,
                ActivityId = SeedConstants.ActivityC8Id,
                StartDateTime = twoDaysFromNow.AddHours(10),
                EndDateTime = twoDaysFromNow.AddHours(12),
                Status = SlotStatus.Available,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<ActivitySlot> GetFosteringsTestSlots(DateTime now)
    {
        return new List<ActivitySlot>
        {
            new()
            {
                Id = SeedConstants.SlotMax001,
                ActivityId = SeedConstants.ActivityMax001,
                StartDateTime = now.AddDays(2).Date.AddHours(10),
                EndDateTime = now.AddDays(2).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotMax002,
                ActivityId = SeedConstants.ActivityMax001,
                StartDateTime = now.AddDays(9).Date.AddHours(14),
                EndDateTime = now.AddDays(9).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotMax003,
                ActivityId = SeedConstants.ActivityMax001,
                StartDateTime = now.AddDays(16).Date.AddHours(10),
                EndDateTime = now.AddDays(16).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotLuna001,
                ActivityId = SeedConstants.ActivityLuna001,
                StartDateTime = now.AddDays(3).Date.AddHours(15),
                EndDateTime = now.AddDays(3).Date.AddHours(17),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotLuna002,
                ActivityId = SeedConstants.ActivityLuna001,
                StartDateTime = now.AddDays(10).Date.AddHours(11),
                EndDateTime = now.AddDays(10).Date.AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotLuna003,
                ActivityId = SeedConstants.ActivityLuna001,
                StartDateTime = now.AddDays(17).Date.AddHours(15),
                EndDateTime = now.AddDays(17).Date.AddHours(17),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotCharlie001,
                ActivityId = SeedConstants.ActivityCharlie001,
                StartDateTime = now.AddDays(4).Date.AddHours(9),
                EndDateTime = now.AddDays(4).Date.AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotCharlie002,
                ActivityId = SeedConstants.ActivityCharlie001,
                StartDateTime = now.AddDays(11).Date.AddHours(13),
                EndDateTime = now.AddDays(11).Date.AddHours(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotCharlie003,
                ActivityId = SeedConstants.ActivityCharlie001,
                StartDateTime = now.AddDays(18).Date.AddHours(9),
                EndDateTime = now.AddDays(18).Date.AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotBella001,
                ActivityId = SeedConstants.ActivityBella001,
                StartDateTime = now.AddDays(5).Date.AddHours(16),
                EndDateTime = now.AddDays(5).Date.AddHours(18),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotBella002,
                ActivityId = SeedConstants.ActivityBella001,
                StartDateTime = now.AddDays(12).Date.AddHours(10),
                EndDateTime = now.AddDays(12).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotBella003,
                ActivityId = SeedConstants.ActivityBella001,
                StartDateTime = now.AddDays(19).Date.AddHours(16),
                EndDateTime = now.AddDays(19).Date.AddHours(18),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotRocky001,
                ActivityId = SeedConstants.ActivityRocky001,
                StartDateTime = now.AddDays(6).Date.AddHours(14),
                EndDateTime = now.AddDays(6).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotRocky002,
                ActivityId = SeedConstants.ActivityRocky001,
                StartDateTime = now.AddDays(13).Date.AddHours(11),
                EndDateTime = now.AddDays(13).Date.AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotRocky003,
                ActivityId = SeedConstants.ActivityRocky001,
                StartDateTime = now.AddDays(20).Date.AddHours(14),
                EndDateTime = now.AddDays(20).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotMaxPast001,
                ActivityId = SeedConstants.ActivityMax001,
                StartDateTime = now.AddDays(-5).Date.AddHours(10),
                EndDateTime = now.AddDays(-5).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotCancelled001,
                ActivityId = SeedConstants.ActivityCancelled001,
                StartDateTime = now.AddDays(7).Date.AddHours(10),
                EndDateTime = now.AddDays(7).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new()
            {
                Id = SeedConstants.SlotMaxAvailable001,
                ActivityId = SeedConstants.ActivityMax001,
                StartDateTime = now.AddDays(8).Date.AddHours(10),
                EndDateTime = now.AddDays(8).Date.AddHours(12),
                Status = SlotStatus.Available,
                Type = SlotType.Activity,
                CreatedAt = now
            }
        };
    }

    private static List<ShelterUnavailabilitySlot> GetShelterUnavailabilitySlots(DateTime baseDate)
    {
        return new List<ShelterUnavailabilitySlot>
        {
            new()
            {
                Id = SeedConstants.UnavShort,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(1).AddHours(12),
                EndDateTime = baseDate.AddDays(1).AddHours(13),
                Reason = "Pausa de almoço",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavHalfDay,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(2).AddHours(9),
                EndDateTime = baseDate.AddDays(2).AddHours(14),
                Reason = "Formação de manhã",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavFullDay,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(3).AddHours(9),
                EndDateTime = baseDate.AddDays(3).AddHours(18),
                Reason = "Evento de dia inteiro",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavWeek,
                ShelterId = SeedConstants.Shelter2Id,
                StartDateTime = baseDate.AddDays(21),
                EndDateTime = baseDate.AddDays(28),
                Reason = "Semana de férias",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavMultiDay,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(14),
                EndDateTime = baseDate.AddDays(17),
                Reason = "Manutenção alargada",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavBeforeOpen,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(7).AddHours(7),
                EndDateTime = baseDate.AddDays(7).AddHours(10),
                Reason = "Preparação cedo",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavAfterClose,
                ShelterId = SeedConstants.Shelter2Id,
                StartDateTime = baseDate.AddDays(8).AddHours(16),
                EndDateTime = baseDate.AddDays(8).AddHours(20),
                Reason = "Evento ao final do dia",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavExactHours,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(9).AddHours(9),
                EndDateTime = baseDate.AddDays(9).AddHours(18),
                Reason = "Encerramento total no horário",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.UnavMidnight,
                ShelterId = SeedConstants.Shelter1Id,
                StartDateTime = baseDate.AddDays(10).AddHours(22),
                EndDateTime = baseDate.AddDays(11).AddHours(3),
                Reason = "Manutenção noturna",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}