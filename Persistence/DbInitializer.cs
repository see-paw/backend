using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

<<<<<<< HEAD
public static class DbInitializer
{

    public static async Task SeedData(AppDbContext dbContext)
    {
        const string breed1Id = "1a1a1111-1111-1111-1111-111111111111";
        const string breed2Id = "2b2b2222-2222-2222-2222-222222222222";
        const string breed3Id = "3c3c3333-3333-3333-3333-333333333333";
        const string shelter1Id = "11111111-1111-1111-1111-111111111111";
        const string shelter2Id = "22222222-2222-2222-2222-222222222222";
        const string animal1Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1b";
        const string animal2Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd2b";
        const string animal3Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd3b";
        const string animal4Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd4b";
        const string animal5Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd5b";
        const string animal6Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd6b";
        const string animal7Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd7b";
        const string animal8Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd8b";
        const string animal9Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd9b";
        const string animal10Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd0c";

        // ======== SEED SHELTERS ========
        if (!dbContext.Shelters.Any())
        {
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
        }

        // ======== SEED BREEDS ========
        if (!dbContext.Breeds.Any())
        {
            var breeds = new List<Breed>
            {
                new() { Id = breed1Id, Name = "Siamês", Description = "Raça de gato elegante e sociável." },
                new() { Id = breed2Id, Name = "Beagle", Description = "Cão amigável, curioso e ativo." },
                new() { Id = breed3Id, Name = "Pastor Alemão", Description = "Cão leal, inteligente e protetor." }
            };

            await dbContext.Breeds.AddRangeAsync(breeds);
            await dbContext.SaveChangesAsync();
        }

        // ======== SEED ANIMALS ========

        if (!dbContext.Animals.Any())
        {
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
                 BreedId = breed1Id,
                Cost = 30,
                Features = "Olhos verdes, muito sociável",
                ShelterId = shelter1Id,
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
                 BreedId = breed2Id,
                Cost = 50,
                Features = "Muito obediente e adora correr",
                ShelterId = shelter1Id,
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
                 BreedId = breed3Id,
                Cost = 70,
                Features = "Olhos azuis e muita energia",
                ShelterId = "22222222-2222-2222-2222-222222222222",
                Images = new List<Image>()
            },
            new()
            {
                Id = animal1Id,
                Name = "Bolinhas",
                AnimalState = AnimalState.Available,
                Description = "Gato muito meigo e brincalhão, gosta de dormir ao sol.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Branco e cinzento",
                BirthDate = new DateOnly(2022, 4, 15),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 30,
                Features = "Olhos verdes, muito sociável",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal2Id,
                Name = "Luna",
                AnimalState = AnimalState.Available,
                Description = "Cadela jovem e energética, ideal para famílias com crianças.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Female,
                Colour = "Castanho claro",
                BirthDate = new DateOnly(2021, 11, 5),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 50,
                Features = "Muito obediente e adora correr",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal3Id,
                Name = "Tico",
                AnimalState = AnimalState.Available,
                Description = "Papagaio falador que adora companhia humana.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Verde com azul",
                BirthDate = new DateOnly(2020, 2, 10),
                Sterilized = false,
                BreedId = breed2Id,
                Cost = 80,
                Features = "Sabe dizer 'Olá!' e assobiar",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal4Id,
                Name = "Mika",
                AnimalState = AnimalState.Available,
                Description = "Gata calma e dócil, procura um lar tranquilo.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Preto",
                BirthDate = new DateOnly(2020, 8, 22),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 25,
                Features = "Olhos azuis intensos",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal5Id,
                Name = "Thor",
                AnimalState = AnimalState.Available,
                Description = "Cão de guarda muito protetor, mas fiel à família.",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Preto e castanho",
                BirthDate = new DateOnly(2019, 6, 30),
                Sterilized = false,
                BreedId = breed2Id,
                Cost = 100,
                Features = "Muito atento e obediente",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal6Id,
                Name = "Nina",
                AnimalState = AnimalState.Available,
                Description = "Coelha curiosa e afetuosa, gosta de cenouras e de brincar.",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Branco com manchas castanhas",
                BirthDate = new DateOnly(2023, 3, 10),
                Sterilized = false,
                BreedId = breed2Id,
                Cost = 15,
                Features = "Orelhas pequenas e pelo macio",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal7Id,
                Name = "Rocky",
                AnimalState = AnimalState.Inactive,
                Description = "Cão atlético e leal, ideal para quem gosta de caminhadas.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Cinza",
                BirthDate = new DateOnly(2022, 7, 19),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 70,
                Features = "Olhos azuis e muita energia",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal8Id,
                Name = "Amora",
                AnimalState = AnimalState.HasOwner,
                Description = "Gata jovem e curiosa, adora caçar brinquedos.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Cinzento e branco",
                BirthDate = new DateOnly(2023, 5, 14),
                Sterilized = false,
                BreedId = breed2Id,
                Cost = 20,
                Features = "Bigodes longos e muito expressiva",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal9Id,
                Name = "Zeus",
                AnimalState = AnimalState.TotallyFostered,
                Description = "Cavalo calmo e bem treinado, ótimo para equitação.",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Castanho escuro",
                BirthDate = new DateOnly(2017, 9, 1),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 500,
                Features = "Crina longa e brilhante",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },
            new()
            {
                Id = animal10Id,
                Name = "Pipoca",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Hamster pequena e simpática, ideal para crianças.",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Dourado",
                BirthDate = new DateOnly(2024, 1, 12),
                Sterilized = false,
                BreedId = breed2Id,
                Cost = 10,
                Features = "Muito ativa e adora correr na roda",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            }
        };

            await dbContext.Animals.AddRangeAsync(animals);
            await dbContext.SaveChangesAsync();
        }

        // ======== SEED IMAGES ========

        if (!dbContext.Images.Any())
        {
            var images = new List<Image>
            {
                // === Shelter 1 ===
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ShelterId = shelter1Id,
                    Url = "https://placekitten.com/600/400",
                    Description = "Fachada principal do CAA Porto",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ShelterId = shelter1Id,
                    Url = "https://placekitten.com/601/401",
                    Description = "Área de recreio dos animais",
                    IsPrincipal = false
                },

                // === Shelter 2 ===
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ShelterId = "22222222-2222-2222-2222-222222222222",
                    Url = "https://placedog.net/600/400?id=2",
                    Description = "Instalações do CAA de Cima",
                    IsPrincipal = true
                },

                // === Animais ===
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal1Id,
                    Url = "https://placekitten.com/500/400",
                    Description = "Bolinhas deitado ao sol",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal2Id,
                    Url = "https://placedog.net/501/401?id=1",
                    Description = "Luna a correr no jardim",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal3Id,
                    Url = "https://placeparrot.com/400/300",
                    Description = "Tico no poleiro",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal4Id,
                    Url = "https://placekitten.com/401/301",
                    Description = "Mika deitada no sofá",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal5Id,
                    Url = "https://placedog.net/502/402?id=2",
                    Description = "Thor atento ao portão",
                    IsPrincipal = true
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    AnimalId = animal6Id,
                    Url = "https://placebunny.com/500/350",
                    Description = "Nina comendo cenoura",
                    IsPrincipal = true
                }
            };

            await dbContext.Images.AddRangeAsync(images);
            await dbContext.SaveChangesAsync();
        }
    }
}

=======
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
>>>>>>> feature/create-and-list-animals
