using Domain;

namespace Persistence.Seeds;

/// <summary>
/// Seeds breeds into the database.
/// </summary>
internal static class BreedSeeder
{
    /// <summary>
    /// Seeds all breeds into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Breeds.Any())
        {
            var breeds = new List<Breed>();
            
            breeds.AddRange(GetMainBreeds());
            breeds.AddRange(GetOwnershipRequestTestBreeds());
            breeds.Add(GetFosteringTestBreed());
            breeds.Add(GetCancelFosteringTestBreed());
            
            await dbContext.Breeds.AddRangeAsync(breeds);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Breed> GetMainBreeds()
    {
        return new List<Breed>
        {
            new()
            {
                Id = SeedConstants.Breed1Id,
                Name = "Siamês",
                Description = "Raça de gato elegante e sociável."
            },
            new()
            {
                Id = SeedConstants.Breed2Id,
                Name = "Beagle",
                Description = "Cão amigável, curioso e ativo."
            },
            new()
            {
                Id = SeedConstants.Breed3Id,
                Name = "Pastor Alemão",
                Description = "Cão leal, inteligente e protetor."
            }
        };
    }

    private static List<Breed> GetOwnershipRequestTestBreeds()
    {
        return new List<Breed>
        {
            new()
            {
                Id = SeedConstants.OwnershipBreed1Id,
                Name = "Labrador Retriever",
                Description = "Cão de porte médio a grande, muito amigável"
            },
            new()
            {
                Id = SeedConstants.OwnershipBreed2Id,
                Name = "Golden Retriever",
                Description = "Cão grande, dócil e muito inteligente"
            },
            new()
            {
                Id = SeedConstants.OwnershipBreed3Id,
                Name = "Pastor Português",
                Description = "Cão grande, protetor e leal"
            },
            new()
            {
                Id = SeedConstants.OwnershipBreed4Id,
                Name = "Rafeiro",
                Description = "Cão de porte médio, curioso e enérgico"
            }
        };
    }

    private static Breed GetFosteringTestBreed()
    {
        return new Breed
        {
            Id = SeedConstants.FosteringBreedId,
            Name = "Test Breed"
        };
    }

    private static Breed GetCancelFosteringTestBreed()
    {
        return new Breed
        {
            Id = SeedConstants.CancelBreedId,
            Name = "Cancel Test Breed",
            CreatedAt = DateTime.UtcNow
        };
    }
}