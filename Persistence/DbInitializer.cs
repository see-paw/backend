using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

/// <summary>
/// Seeds the database with initial data for shelters and animals (including images).
/// </summary>
public static class DbInitializer
{
    public static async Task SeedData(AppDbContext dbContext)
    {
        // Skip seeding if already populated
        if (dbContext.Shelters.Any() || dbContext.Animals.Any())
            return;

        // ======== SEED SHELTERS ========
        var shelters = new List<Shelter>
        {
            new()
            {
                Id = "11111111-1111-1111-1111-111111111111",
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
                Id = "22222222-2222-2222-2222-222222222222",
                Name = "Test Shelter 2",
                Street = "Rua de Cima 898",
                City = "Porto",
                PostalCode = "4000-125",
                Phone = "224589631",
                NIF = "999999999",
                OpeningTime = new TimeOnly(9, 0, 0),
                ClosingTime = new TimeOnly(18, 0, 0),
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Shelters.AddRangeAsync(shelters);
        await dbContext.SaveChangesAsync();

        // ======== SEED BREEDS ========
        var breeds = new List<Breed>
    {
        new() { Id = "b1", Name = "Siamês", Description = "Raça de gato elegante e sociável." },
        new() { Id = "b2", Name = "Beagle", Description = "Cão amigável, curioso e ativo." },
        new() { Id = "b3", Name = "Pastor Alemão", Description = "Cão leal, inteligente e protetor." }
    };

        await dbContext.Breeds.AddRangeAsync(breeds);
        await dbContext.SaveChangesAsync();

        // ======== SEED ANIMALS ========
        var animals = new List<Animal>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bolinhas",
                AnimalState = AnimalState.Available,
                Description = "Gato muito meigo e brincalhão, gosta de dormir ao sol.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Branco e cinzento",
                BirthDate = new DateOnly(2022, 4, 15),
                Sterilized = true,
                 BreedId = "b1",
                Cost = 30,
                Features = "Olhos verdes, muito sociável",
                ShelterId = "11111111-1111-1111-1111-111111111111",
                Images = new List<Image>()
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Luna",
                AnimalState = AnimalState.Available,
                Description = "Cadela jovem e energética, ideal para famílias com crianças.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Female,
                Colour = "Castanho claro",
                BirthDate = new DateOnly(2021, 11, 5),
                Sterilized = true,
                 BreedId = "b2",
                Cost = 50,
                Features = "Muito obediente e adora correr",
                ShelterId = "11111111-1111-1111-1111-111111111111",
                Images = new List<Image>()
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Rocky",
                AnimalState = AnimalState.Available,
                Description = "Cão atlético e leal, ideal para quem gosta de caminhadas.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Cinza",
                BirthDate = new DateOnly(2022, 7, 19),
                Sterilized = true,
                 BreedId = "b3",
                Cost = 70,
                Features = "Olhos azuis e muita energia",
                ShelterId = "22222222-2222-2222-2222-222222222222",
                Images = new List<Image>()
            }
        };

        await dbContext.Animals.AddRangeAsync(animals);
        await dbContext.SaveChangesAsync();
    }
}