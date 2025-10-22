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
        // Ensure database is created
        await dbContext.Database.EnsureCreatedAsync();


        if (await dbContext.Animals.AnyAsync() || await dbContext.Shelters.AnyAsync())
            return;


        // ========  SEED SHELTERS  ========
        var shelter1Id = "11111111-1111-1111-1111-111111111111";
        var shelter2Id = "22222222-2222-2222-2222-222222222222";


        var shelters = new List<Shelter>
        {
            new()
            {
                Id = shelter1Id,
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
                Id = shelter2Id,
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


        // ===== SEED SHELTERS' IMAGES  ========
        var imageShelter1 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            Url = "https://example.com/images/shelter1.jpg",
            Description = "Main image for Shelter 1",
            IsPrincipal = true,
            CreatedAt = DateTime.UtcNow,
            ShelterId = shelter1Id // Link to shelter
        };

        var imageShelter2 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            Url = "https://example.com/images/shelter2.jpg",
            Description = "Main image for Shelter 2",
            IsPrincipal = true,
            CreatedAt = DateTime.UtcNow,
            ShelterId = shelter2Id // Link to shelter
        };

        await dbContext.Images.AddRangeAsync(imageShelter1, imageShelter2);
        await dbContext.SaveChangesAsync();




        // ======== SEED BREEDS ========
        var breed1 = new Breed { Id = Guid.NewGuid().ToString(), Name = "Siamês", Description = "Raça de gato elegante e sociável." };
        var breed2 = new Breed { Id = Guid.NewGuid().ToString(), Name = "Beagle", Description = "Cão amigável, curioso e ativo." };
        var breed3 = new Breed { Id = Guid.NewGuid().ToString(), Name = "Pastor Alemão", Description = "Cão leal, inteligente e protetor." };

        var breeds = new List<Breed> { breed1, breed2, breed3 };

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
                BreedId = breed1.Id,
                Cost = 30,
                Features = "Olhos verdes, muito sociável",
                ShelterId = shelter1Id
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
                BreedId = breed2.Id,
                Cost = 50,
                Features = "Muito obediente e adora correr",
                ShelterId = shelter1Id
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
                BreedId = breed3.Id,
                Cost = 70,
                Features = "Olhos azuis e muita energia",
                ShelterId = shelter2Id
            }
        };

        await dbContext.Animals.AddRangeAsync(animals);
        await dbContext.SaveChangesAsync();

        // ======== SEED ANIMAL IMAGES ========
        var image1 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            IsPrincipal = true,
            Url = "https://example.com/images/bolinhas1.jpg",
            Description = "Bolinhas.",
            CreatedAt = DateTime.UtcNow,
            AnimalId = animals[0].Id
        };

        var image2 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            IsPrincipal = false,
            Url = "https://example.com/images/bolinhas2.jpg",
            Description = "Bolinhas a brincar.",
            CreatedAt = DateTime.UtcNow,
            AnimalId = animals[0].Id
        };

        var image3 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            IsPrincipal = true,
            Url = "https://example.com/images/luna.jpg",
            Description = "Luna.",
            CreatedAt = DateTime.UtcNow,
            AnimalId = animals[1].Id
        };

        var image4 = new Image
        {
            Id = Guid.NewGuid().ToString(),
            IsPrincipal = true,
            Url = "https://example.com/images/rocky1.jpg",
            Description = "Rocky.",
            CreatedAt = DateTime.UtcNow,
            AnimalId = animals[2].Id
        };

        await dbContext.Images.AddRangeAsync(image1, image2, image3, image4);
        await dbContext.SaveChangesAsync();
    }
}
