using Domain;

namespace Persistence.Seeds;

/// <summary>
/// Seeds shelters into the database.
/// </summary>
internal static class ShelterSeeder
{
    /// <summary>
    /// Seeds all shelters into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Shelters.Any())
        {
            var shelters = new List<Shelter>();
            
            shelters.AddRange(GetMainShelters());
            shelters.AddRange(GetOwnershipRequestTestShelters());
            shelters.Add(GetFosteringTestShelter());
            shelters.Add(GetCancelFosteringTestShelter());
            
            await dbContext.Shelters.AddRangeAsync(shelters);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Shelter> GetMainShelters()
    {
        return new List<Shelter>
        {
            new()
            {
                Id = SeedConstants.Shelter1Id,
                Name = "Test Shelter",
                Street = "Rua das Flores 123",
                City = "Porto",
                PostalCode = "4000-123",
                Phone = "912345678",
                NIF = "123456789",
                OpeningTime = new TimeOnly(9, 0, 0),
                ClosingTime = new TimeOnly(18, 0, 0),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.Shelter2Id,
                Name = "Test Shelter 2",
                Street = "Rua de Cima 898",
                City = "Porto",
                PostalCode = "4000-125",
                Phone = "224589631",
                NIF = "999999999",
                OpeningTime = new TimeOnly(9, 0, 0),
                ClosingTime = new TimeOnly(18, 0, 0),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.Shelter3Id,
                Name = "Notifications Test Shelter",
                Street = "Rua dos Testes 999",
                City = "Porto",
                PostalCode = "4100-999",
                Phone = "910000000",
                NIF = "888888888",
                OpeningTime = new TimeOnly(9, 0, 0),
                ClosingTime = new TimeOnly(18, 0, 0),
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Shelter> GetOwnershipRequestTestShelters()
    {
        return new List<Shelter>
        {
            new()
            {
                Id = SeedConstants.OwnershipShelter1Id,
                Name = "Associação Protetora dos Animais do Porto",
                Street = "Rua dos Animais, 100",
                City = "Porto",
                PostalCode = "4100-001",
                Phone = "222333444",
                NIF = "501234567",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            },
            new()
            {
                Id = SeedConstants.OwnershipShelter2Id,
                Name = "Centro de Recolha Animal de Lisboa",
                Street = "Avenida dos Bichos, 200",
                City = "Lisboa",
                PostalCode = "1300-001",
                Phone = "213444555",
                NIF = "502345678",
                OpeningTime = new TimeOnly(10, 0),
                ClosingTime = new TimeOnly(19, 0)
            }
        };
    }

    private static Shelter GetFosteringTestShelter()
    {
        return new Shelter
        {
            Id = SeedConstants.FosteringShelterId,
            Name = "Test Shelter Porto",
            Street = "Rua do Abrigo 789",
            City = "Porto",
            PostalCode = "4100-001",
            Phone = "223456789",
            NIF = "674498653",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Shelter GetCancelFosteringTestShelter()
    {
        return new Shelter
        {
            Id = SeedConstants.CancelShelterId,
            Name = "Cancel Test Shelter",
            Street = "Rua Cancel Shelter 789",
            City = "Porto",
            PostalCode = "4100-001",
            Phone = "223456789",
            NIF = "295582693",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0),
            CreatedAt = DateTime.UtcNow
        };
    }
}