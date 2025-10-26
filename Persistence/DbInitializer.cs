using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedData(
            AppDbContext dbContext,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            // ======== CONSTANTES ========
            const string breed1Id = "1a1a1111-1111-1111-1111-111111111111";
            const string breed2Id = "2b2b2222-2222-2222-2222-222222222222";
            const string breed3Id = "3c3c3333-3333-3333-3333-333333333333";
            const string shelter1Id = "11111111-1111-1111-1111-111111111111";
            const string shelter2Id = "22222222-2222-2222-2222-222222222222";
            const string platformAdmin = "PlatformAdmin";
            const string adminCaa = "AdminCAA";
            const string userRole = "User";

            // Animais
            const string animal1Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1b"; // Bolinhas
            const string animal2Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd2b"; // Luna
            const string animal3Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd3b"; // Tico
            const string animal4Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd4b"; // Mika
            const string animal5Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd5b"; // Thor
            const string animal6Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd6b"; // Nina
            const string animal7Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd7b"; // Rocky
            const string animal8Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd8b"; // Amora
            const string animal9Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd9b"; // Zeus
            const string animal10Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd0c"; // Pipoca

            // Users
            const string user1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // Alice
            const string user2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // Bob
            const string user3Id = "cccccccc-cccc-cccc-cccc-cccccccccccc"; // Carlos
            const string user4Id = "dddddddd-dddd-dddd-dddd-dddddddddddd"; // Diana
            const string user5Id = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"; // Eduardo
            const string user6Id = "66666666-6666-6666-6666-666666666666"; // Filipe

            // ======== SEED SHELTERS ========
            if (!dbContext.Shelters.Any())
            {
                await dbContext.Shelters.AddRangeAsync(new List<Shelter>
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
                        OpeningTime = new TimeOnly(9, 0),
                        ClosingTime = new TimeOnly(18, 0),
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
                        OpeningTime = new TimeOnly(9, 0),
                        ClosingTime = new TimeOnly(18, 0),
                        CreatedAt = DateTime.UtcNow
                    }
                });
                await dbContext.SaveChangesAsync();
            }

            // ======== SEED USERS ========
            if (!userManager.Users.Any())
            {
                var roles = new[] { platformAdmin, adminCaa, userRole };
                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                var users = new List<User>
                {
                    new()
                    {
                        Id = user1Id,
                        Name = "Alice Ferreira",
                        UserName = "alice@test.com",
                        Email = "alice@test.com",
                        ShelterId = shelter1Id,
                        City = "Lisboa",
                        Street = "Avenida da Liberdade 55",
                        PostalCode = "1250-123",
                        BirthDate = new DateTime(1998, 11, 2),
                        PhoneNumber = "934567890",
                        CreatedAt = DateTime.UtcNow
                    },
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
                        ShelterId = shelter2Id,
                        City = "Porto",
                        Street = "Rua das Oliveiras 99",
                        PostalCode = "4000-450",
                        BirthDate = new DateTime(1994, 5, 27),
                        PhoneNumber = "912345999",
                        CreatedAt = DateTime.UtcNow
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
                            "alice@test.com" or "filipe@test.com" => adminCaa,
                            _ => userRole
                        };
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }

            // ======== SEED BREEDS ========
            if (!dbContext.Breeds.Any())
            {
                await dbContext.Breeds.AddRangeAsync(new List<Breed>
                {
                    new() { Id = breed1Id, Name = "Siamês", Description = "Raça de gato elegante e sociável." },
                    new() { Id = breed2Id, Name = "Beagle", Description = "Cão amigável, curioso e ativo." },
                    new() { Id = breed3Id, Name = "Pastor Alemão", Description = "Cão leal, inteligente e protetor." }
                });
                await dbContext.SaveChangesAsync();
            }

            // ======== SEED ANIMALS ========
            if (!dbContext.Animals.Any())
            {
                await dbContext.Animals.AddRangeAsync(new List<Animal>
                {
                    new() { Id = animal1Id, Name = "Bolinhas", Species = Species.Cat, BreedId = breed1Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Male, Colour = "Branco e cinzento", AnimalState = AnimalState.Available, Cost = 30, BirthDate = new DateOnly(2022, 4, 15), Description = "Gato meigo e brincalhão.", Features = "Olhos verdes" },
                    new() { Id = animal2Id, Name = "Luna", Species = Species.Dog, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Medium, Sex = SexType.Female, Colour = "Castanho claro", AnimalState = AnimalState.Available, Cost = 50, BirthDate = new DateOnly(2021, 11, 5), Description = "Cadela energética e dócil.", Features = "Muito obediente" },
                    new() { Id = animal3Id, Name = "Tico", Species = Species.Cat, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Male, Colour = "Verde com azul", AnimalState = AnimalState.Available, Cost = 80, BirthDate = new DateOnly(2020, 2, 10), Description = "Papagaio falador.", Features = "Sabe dizer 'Olá!'" },
                    new() { Id = animal4Id, Name = "Mika", Species = Species.Cat, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Female, Colour = "Preto", AnimalState = AnimalState.Available, Cost = 25, BirthDate = new DateOnly(2020, 8, 22), Description = "Gata calma e dócil.", Features = "Olhos azuis" },
                    new() { Id = animal5Id, Name = "Thor", Species = Species.Dog, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Large, Sex = SexType.Male, Colour = "Preto e castanho", AnimalState = AnimalState.Available, Cost = 100, BirthDate = new DateOnly(2019, 6, 30), Description = "Cão de guarda leal.", Features = "Muito atento" },
                    new() { Id = animal6Id, Name = "Nina", Species = Species.Dog, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Female, Colour = "Branco com castanho", AnimalState = AnimalState.Available, Cost = 15, BirthDate = new DateOnly(2023, 3, 10), Description = "Coelha curiosa.", Features = "Orelhas pequenas" },
                    new() { Id = animal7Id, Name = "Rocky", Species = Species.Dog, BreedId = breed3Id, ShelterId = shelter2Id, Size = SizeType.Medium, Sex = SexType.Male, Colour = "Cinza", AnimalState = AnimalState.Available, Cost = 70, BirthDate = new DateOnly(2022, 7, 19), Description = "Cão leal e energético.", Features = "Olhos azuis" },
                    new() { Id = animal8Id, Name = "Amora", Species = Species.Cat, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Female, Colour = "Cinzento e branco", AnimalState = AnimalState.HasOwner, Cost = 20, BirthDate = new DateOnly(2023, 5, 14), Description = "Gata curiosa.", Features = "Bigodes longos" },
                    new() { Id = animal9Id, Name = "Zeus", Species = Species.Dog, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Large, Sex = SexType.Male, Colour = "Castanho escuro", AnimalState = AnimalState.TotallyFostered, Cost = 500, BirthDate = new DateOnly(2017, 9, 1), Description = "Cavalo calmo e treinado.", Features = "Crina longa" },
                    new() { Id = animal10Id, Name = "Pipoca", Species = Species.Dog, BreedId = breed2Id, ShelterId = shelter1Id, Size = SizeType.Small, Sex = SexType.Female, Colour = "Dourado", AnimalState = AnimalState.PartiallyFostered, Cost = 10, BirthDate = new DateOnly(2024, 1, 12), Description = "Hamster simpática.", Features = "Adora correr na roda" }
                });
                await dbContext.SaveChangesAsync();
            }

            // ======== SEED IMAGES ========
            if (!dbContext.Images.Any())
            {
                await dbContext.Images.AddRangeAsync(new List<Image>
                {
                    new() { Id = Guid.NewGuid().ToString(), ShelterId = shelter1Id, Url = "https://placekitten.com/600/400", Description = "Fachada do abrigo Porto", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), ShelterId = shelter2Id, Url = "https://placedog.net/600/400?id=2", Description = "Abrigo de Cima", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal1Id, Url = "https://placekitten.com/500/400", Description = "Bolinhas deitado ao sol", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal2Id, Url = "https://placedog.net/501/401?id=1", Description = "Luna a correr no jardim", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal3Id, Url = "https://placeparrot.com/400/300", Description = "Tico no poleiro", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal4Id, Url = "https://placekitten.com/401/301", Description = "Mika no sofá", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal5Id, Url = "https://placedog.net/502/402?id=2", Description = "Thor atento ao portão", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal6Id, Url = "https://placebunny.com/500/350", Description = "Nina comendo cenoura", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal7Id, Url = "https://placedog.net/503/403?id=3", Description = "Rocky no parque", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal8Id, Url = "https://placekitten.com/402/302", Description = "Amora a brincar", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal9Id, Url = "https://placehorse.com/600/400", Description = "Zeus trotando no campo", IsPrincipal = true },
                    new() { Id = Guid.NewGuid().ToString(), AnimalId = animal10Id, Url = "https://placehamster.com/400/300", Description = "Pipoca na roda", IsPrincipal = true }
                });
                await dbContext.SaveChangesAsync();
            }

            // ======== SEED FOSTERINGS ========
            if (!dbContext.Fosterings.Any())
            {
                await dbContext.Fosterings.AddRangeAsync(new List<Fostering>
                {
                    new() { Id = "11111111-aaaa-4a4a-aaaa-111111111111", AnimalId = animal3Id, UserId = "38bd42ca-c819-4496-be10-0d312a08c837", Status = FosteringStatus.Active, StartDate = new DateTime(2025, 10, 1), Amount = 10 },
                    new() { Id = "22222222-bbbb-4b4b-bbbb-222222222222", AnimalId = animal7Id, UserId = "38bd42ca-c819-4496-be10-0d312a08c837", Status = FosteringStatus.Active, StartDate = new DateTime(2025, 9, 15), Amount = 15 },
                    new() { Id = "33333333-cccc-4c4c-cccc-333333333333", AnimalId = animal9Id, UserId = "38bd42ca-c819-4496-be10-0d312a08c837", Status = FosteringStatus.Active, StartDate = new DateTime(2025, 8, 20), Amount = 20 }
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
