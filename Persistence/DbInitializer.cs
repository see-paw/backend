using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Persistence;

/// <summary>
/// Provides initial seeding logic for the application's database.
/// 
/// This class is responsible for populating the database with essential data, 
/// including roles, users, shelters, breeds, animals, and images, ensuring 
/// the system starts with a consistent baseline dataset for development, testing, or demonstration.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with default data such as user roles, users, shelters, breeds, animals, and images.
    /// 
    /// This method ensures that core entities are created only when they do not already exist, 
    /// preventing duplication and maintaining idempotent execution.
    /// </summary>
    /// <param name="dbContext">The application's database context used to persist entities.</param>
    /// <param name="userManager">The <see cref="UserManager{TUser}"/> used to manage user creation and role assignment.</param>
    /// <param name="roleManager">The <see cref="RoleManager{TRole}"/> used to manage roles in the identity system.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging seeding operations and errors.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedData(AppDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory)
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
        const string platformAdmin = "PlatformAdmin";
        const string adminCaa = "AdminCAA";
        const string userRole = "User";
        const string user1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // Alice
        const string user2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // Bob
        const string user3Id = "cccccccc-cccc-cccc-cccc-cccccccccccc"; // Carlos
        const string user4Id = "dddddddd-dddd-dddd-dddd-dddddddddddd"; // Diana
        const string user5Id = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"; // Eduardo
        const string user6Id = "66666666-6666-6666-6666-666666666666"; // Filipe

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

        // ======== USERS SHELTERS ========

        if (!userManager.Users.Any())
        {
            var roles = new List<string> { platformAdmin, adminCaa, userRole };
            var logger = loggerFactory.CreateLogger("DbInitializer");

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            var users = new List<User>
            {
                new()
                {
                    Id = user2Id,
                    Name = "Bob Johnson",
                    UserName = "bob@test.com",
                    Email = "bob@test.com",
                    City = "Porto",
                    Street = "Rua das Flores 10",
                    PostalCode = "4000-123",
                    BirthDate = new DateTime(1995, 4, 12),
                    PhoneNumber = "912345678",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = user1Id,
                    Name = "Alice Ferreira",
                    UserName = "alice@test.com",
                    Email = "alice@test.com",
                    City = "Lisboa",
                    Street = "Avenida da Liberdade 55",
                    PostalCode = "1250-123",
                    BirthDate = new DateTime(1998, 11, 2),
                    PhoneNumber = "934567890",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = shelter1Id
                },
                new()
                {
                    Id = user3Id,
                    Name = "Carlos Santos",
                    UserName = "carlos@test.com",
                    Email = "carlos@test.com",
                    City = "Coimbra",
                    Street = "Rua do Penedo 32",
                    PostalCode = "3000-222",
                    BirthDate = new DateTime(1992, 6, 8),
                    PhoneNumber = "967123456",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = user4Id,
                    Name = "Diana Silva",
                    UserName = "diana@test.com",
                    Email = "diana@test.com",
                    City = "Faro",
                    Street = "Rua das Oliveiras 8",
                    PostalCode = "8000-333",
                    BirthDate = new DateTime(1990, 9, 30),
                    PhoneNumber = "925111333",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = user5Id,
                    Name = "Eduardo Lima",
                    UserName = "eduardo@test.com",
                    Email = "eduardo@test.com",
                    City = "Braga",
                    Street = "Rua Nova 42",
                    PostalCode = "4700-321",
                    BirthDate = new DateTime(1988, 2, 14),
                    PhoneNumber = "915222444",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = user6Id,
                    Name = "Filipe Marques",
                    UserName = "filipe@test.com",
                    Email = "filipe@test.com",
                    City = "Porto",
                    Street = "Rua das Oliveiras 99",
                    PostalCode = "4000-450",
                    BirthDate = new DateTime(1994, 5, 27),
                    PhoneNumber = "912345999",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = "22222222-2222-2222-2222-222222222222" // Test Shelter 2
                }
            };

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, "Pa$$w0rd");

                if (result.Succeeded)
                {
                    var role = user.Email switch
                    {
                        "bob@test.com" => platformAdmin,
                        "alice@test.com" => adminCaa,
                        "filipe@test.com" => adminCaa,
                        _ => userRole
                    };

                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    logger.LogWarning("Erro ao criar utilizador {Email}: {Errors}", user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
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
                Name = "Celinho",
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
            // ======== ANIMALS FOR ELIGIBITY TESTING  ========

            // 1. Animal Available for Ownership (200 OK)
            new()
            {
                Id = "available-animal-id-123",
                Name = "TestDog Available",
                AnimalState = AnimalState.Available,
                Description = "Animal de teste disponível para adoção",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Preto",
                BirthDate = new DateOnly(2022, 1, 15),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 40,
                Features = "Animal de teste - Estado: Available",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },

            // 2. Animal with owner (400 Bad Request)
            new()
            {
                Id = "animal-with-owner-id",
                Name = "TestDog HasOwner",
                AnimalState = AnimalState.HasOwner,
                Description = "Animal de teste que já tem dono",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Branco",
                BirthDate = new DateOnly(2021, 5, 10),
                Sterilized = true,
                BreedId = breed2Id,
                Cost = 35,
                Features = "Animal de teste - Estado: HasOwner",
                ShelterId = shelter1Id,
                OwnerId = user3Id, // Carlos
                OwnershipStartDate = DateTime.UtcNow.AddMonths(-2),
                Images = new List<Image>()
            },

            // 3. Animal Inactive (400 Bad Request)
            new()
            {
                Id = "inactive-animal-id",
                Name = "TestCat Inactive",
                AnimalState = AnimalState.Inactive,
                Description = "Animal de teste inativo",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Cinzento",
                BirthDate = new DateOnly(2020, 8, 20),
                Sterilized = true,
                BreedId = breed1Id,
                Cost = 25,
                Features = "Animal de teste - Estado: Inactive",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },

            // 4. Animal Partially Fostered (400 Bad Request)
            new()
            {
                Id = "partially-fostered-animal-id",
                Name = "TestDog PartiallyFostered",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Animal de teste parcialmente acolhido",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Castanho",
                BirthDate = new DateOnly(2021, 3, 5),
                Sterilized = false,
                BreedId = breed3Id,
                Cost = 60,
                Features = "Animal de teste - Estado: PartiallyFostered",
                ShelterId = shelter1Id,
                Images = new List<Image>()
            },

            // 5. Animal Totally Fostered (400 Bad Request)
            new()
            {
                Id = "totally-fostered-animal-id",
                Name = "TestCat TotallyFostered",
                AnimalState = AnimalState.TotallyFostered,
                Description = "Animal de teste totalmente acolhido",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Laranja",
                BirthDate = new DateOnly(2022, 7, 12),
                Sterilized = true,
                BreedId = breed1Id,
                Cost = 30,
                Features = "Animal de teste - Estado: TotallyFostered",
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
                Name = "Lunica",
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
                Name = "Rockito",
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

        // ======== SEED FOSTERINGS ========
        if (!dbContext.Fosterings.Any())
        {
            const string fostering2Id = "f0000000-0000-0000-0000-000000000002";
            const string fostering3Id = "f0000000-0000-0000-0000-000000000003";

            var fosterings = new List<Fostering>
            {
                new()
                {
                    Id = fostering2Id,
                    UserId = user4Id, // Diana
                    AnimalId = animal3Id, // Rocky
                    Amount = 20.00m,
                    Status = FosteringStatus.Active,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(50),
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = fostering3Id,
                    UserId = user4Id, // Diana
                    AnimalId = animal4Id, // Mika
                    Amount = 10.00m,
                    Status = FosteringStatus.Active,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(25),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await dbContext.Fosterings.AddRangeAsync(fosterings);
            await dbContext.SaveChangesAsync();
        }

    }
}