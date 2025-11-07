using Domain;
using Domain.Enums;

namespace Persistence.Seeds;

/// <summary>
/// Seeds activities into the database.
/// </summary>
internal static class ActivitySeeder
{
    /// <summary>
    /// Seeds all activities into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Activities.Any())
        {
            var activities = new List<Activity>();
            
            activities.AddRange(GetMainActivities());
            activities.AddRange(GetCancelFosteringTestActivities());
            
            await dbContext.Activities.AddRangeAsync(activities);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Activity> GetMainActivities()
    {
        var baseDate = new DateTime(2025, 11, 3, 0, 0, 0, DateTimeKind.Utc);
        
        return new List<Activity>
        {
            new()
            {
                Id = SeedConstants.ActivityAId,
                AnimalId = SeedConstants.Animal3Id,
                UserId = SeedConstants.User1Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityBId,
                AnimalId = SeedConstants.Animal4Id,
                UserId = SeedConstants.User2Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityCId,
                AnimalId = SeedConstants.Animal3Id,
                UserId = SeedConstants.User3Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityDId,
                AnimalId = SeedConstants.Animal4Id,
                UserId = SeedConstants.User4Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(4),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityEId,
                AnimalId = SeedConstants.Animal5Id,
                UserId = SeedConstants.User5Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityFId,
                AnimalId = SeedConstants.Animal3Id,
                UserId = SeedConstants.User2Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(2),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityGId,
                AnimalId = SeedConstants.Animal4Id,
                UserId = SeedConstants.User3Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(2),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityHId,
                AnimalId = SeedConstants.Animal6Id,
                UserId = SeedConstants.User1Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityIId,
                AnimalId = SeedConstants.Animal7Id,
                UserId = SeedConstants.User4Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityJId,
                AnimalId = SeedConstants.Animal8Id,
                UserId = SeedConstants.User5Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityKId,
                AnimalId = SeedConstants.Animal5Id,
                UserId = SeedConstants.User6Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Activity> GetCancelFosteringTestActivities()
    {
        var twoDaysFromNow = DateTime.UtcNow.Date.AddDays(2);
        
        return new List<Activity>
        {
            new()
            {
                Id = SeedConstants.ActivityC1Id,
                AnimalId = SeedConstants.AnimalC1Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = twoDaysFromNow.AddHours(10),
                EndDate = twoDaysFromNow.AddHours(12),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityC2Id,
                AnimalId = SeedConstants.AnimalC2Id,
                UserId = SeedConstants.OtherCancelUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = twoDaysFromNow.AddHours(14),
                EndDate = twoDaysFromNow.AddHours(16),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityC3Id,
                AnimalId = SeedConstants.AnimalC3Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Cancelled,
                StartDate = twoDaysFromNow.AddHours(10),
                EndDate = twoDaysFromNow.AddHours(12),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityC4Id,
                AnimalId = SeedConstants.AnimalC4Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Completed,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-2).AddHours(2),
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = SeedConstants.ActivityC5Id,
                AnimalId = SeedConstants.AnimalC5Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = DateTime.UtcNow.AddHours(-1),
                EndDate = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = SeedConstants.ActivityC6Id,
                AnimalId = SeedConstants.AnimalC6Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = twoDaysFromNow.AddHours(10),
                EndDate = twoDaysFromNow.AddHours(12),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityC7Id,
                AnimalId = SeedConstants.AnimalC7Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = twoDaysFromNow,
                EndDate = twoDaysFromNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.ActivityC8Id,
                AnimalId = SeedConstants.AnimalC8Id,
                UserId = SeedConstants.CancelFosterUserId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = twoDaysFromNow.AddHours(10),
                EndDate = twoDaysFromNow.AddHours(12),
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}