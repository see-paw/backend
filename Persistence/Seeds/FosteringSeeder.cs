using Domain;
using Domain.Enums;

namespace Persistence.Seeds;

/// <summary>
/// Seeds fosterings into the database.
/// </summary>
internal static class FosteringSeeder
{
    /// <summary>
    /// Seeds all fosterings into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Fosterings.Any())
        {
            var fosterings = new List<Fostering>();
            
            fosterings.AddRange(GetMainFosterings());
            fosterings.AddRange(GetFosteringTestFosterings());
            fosterings.AddRange(GetCancelFosteringTestFosterings());
            
            await dbContext.Fosterings.AddRangeAsync(fosterings);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Fostering> GetMainFosterings()
    {
        return new List<Fostering>
        {
            new()
            {
                Id = SeedConstants.Fostering1Id,
                AnimalId = SeedConstants.Animal2Id,
                UserId = SeedConstants.User4Id,
                Amount = 10,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.Fostering2Id,
                UserId = SeedConstants.User4Id,
                AnimalId = SeedConstants.Animal3Id,
                Amount = 20.00m,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(50),
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.Fostering3Id,
                UserId = SeedConstants.User4Id,
                AnimalId = SeedConstants.Animal4Id,
                Amount = 10.00m,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(25),
                UpdatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Fostering> GetFosteringTestFosterings()
    {
        return new List<Fostering>
        {
            new()
            {
                Id = SeedConstants.FosteringF1Id,
                AnimalId = SeedConstants.AnimalF1Id,
                UserId = SeedConstants.FosterUserId,
                Amount = 50,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringF2Id,
                AnimalId = SeedConstants.AnimalF2Id,
                UserId = SeedConstants.FosterUserId,
                Amount = 40,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = SeedConstants.FosteringF3Id,
                AnimalId = SeedConstants.AnimalF3Id,
                UserId = SeedConstants.FosterUserId,
                Amount = 60,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringInactiveId,
                AnimalId = SeedConstants.AnimalFInactiveId,
                UserId = SeedConstants.FosterUserId,
                Amount = 45,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringWithSlotId,
                AnimalId = SeedConstants.AnimalWithSlotId,
                UserId = SeedConstants.FosterUserId,
                Amount = 50,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringWithActivityId,
                AnimalId = SeedConstants.AnimalWithActivityId,
                UserId = SeedConstants.FosterUserId,
                Amount = 42,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringShelterTestId,
                AnimalId = SeedConstants.AnimalShelterTestId,
                UserId = SeedConstants.FosterUserId,
                Amount = 50,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            }
        };
    }

    private static List<Fostering> GetCancelFosteringTestFosterings()
    {
        return new List<Fostering>
        {
            new()
            {
                Id = SeedConstants.FosteringC1Id,
                AnimalId = SeedConstants.AnimalC1Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 50,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringC2Id,
                AnimalId = SeedConstants.AnimalC2Id,
                UserId = SeedConstants.OtherCancelUserId,
                Amount = 40,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = SeedConstants.FosteringC3Id,
                AnimalId = SeedConstants.AnimalC3Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 60,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringC4Id,
                AnimalId = SeedConstants.AnimalC4Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 35,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringC5Id,
                AnimalId = SeedConstants.AnimalC5Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 50.00m,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringC6Id,
                AnimalId = SeedConstants.AnimalC6Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 42.00m,
                Status = FosteringStatus.Cancelled,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedConstants.FosteringC8Id,
                AnimalId = SeedConstants.AnimalC8Id,
                UserId = SeedConstants.CancelFosterUserId,
                Amount = 50,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-1)
            }
        };
    }
}