using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    /// <param name="roleManager">The <see cref="RoleManager{TRole}"/> used to manage roles in the identity system.</param>f
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging seeding operations and errors.</param>
    /// <param name="resetDatabase">Boolean used to reset or not the database.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task SeedData(AppDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory,
        bool resetDatabase = false)
    {
        const string breed1Id = "1a1a1111-1111-1111-1111-111111111111";
        const string breed2Id = "2b2b2222-2222-2222-2222-222222222222";
        const string breed3Id = "3c3c3333-3333-3333-3333-333333333333";
        const string shelter1Id = "11111111-1111-1111-1111-111111111111";
        const string shelter2Id = "22222222-2222-2222-2222-222222222222";
        const string shelter3Id = "33333333-3333-3333-3333-333333333333"; // Notifications testing
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
        const string animal11Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1c";
        const string animal15Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd5d"; // Notifications testing
        const string platformAdmin = "PlatformAdmin";
        const string adminCaa = "AdminCAA";
        const string userRole = "User";
        const string user1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // Alice
        const string user2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // Bob
        const string user3Id = "cccccccc-cccc-cccc-cccc-cccccccccccc"; // Carlos
        const string user4Id = "dddddddd-dddd-dddd-dddd-dddddddddddd"; // Diana
        const string user5Id = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"; // Eduardo
        const string user6Id = "66666666-6666-6666-6666-666666666666"; // Filipe
        const string user8Id = "88888888-8888-8888-8888-888888888888"; // Alice Notifications (Notifications testing)
        const string user9Id = "99999999-9999-9999-9999-999999999999"; // Carlos Notifications (Notifications testing)
        const string imageShelterUrl1_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835433/shelter_qnix0r.jpg";
        const string imageShelterUrl1_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835501/shelter_lvjzl4.jpg";
        const string imageShelterUrl2_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835423/shelter_pypelc.jpg";
        const string imageShelterUrl2_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835552/shelter_q44gwo.jpg";
        const string imageUrl1_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835037/image2_gjkcko.jpg";
        const string imageUrl1_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835034/images_fcbmbh.jpg";
        const string imageUrl2_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835062/image2_da9jlw.jpg";
        const string imageUrl2_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835058/images_t0jnkr.jpg";
        const string imageUrl3_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835079/image2_fcck0q.jpg";
        const string imageUrl3_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835075/images_jfawej.jpg";
        const string imageUrl4_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835089/image2_qnjamf.jpg";
        const string imageUrl4_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835085/images_jofy7m.jpg";
        const string imageUrl5_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835098/image2_pxn6g2.jpg";
        const string imageUrl5_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835093/images_rn6vpn.jpg";
        const string imageUrl6_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761834737/image2_rugk8b.jpg";
        const string imageUrl6_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761834219/images_jop2o1.jpg";
        const string imageUrl7_1 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835978/images_mn2jce.jpg";
        const string imageUrl7_2 =
            "https://res.cloudinary.com/dnfgbodgr/image/upload/v1761835981/image2_kk3max.jpg";
        // ======== PUBLIC IDS ========

        var publicIdShelter1Img1 = "shelter_qnix0r";
        var publicIdShelter1Img2 = "shelter_lvjzl4";

        var publicIdShelter2Img1 = "shelter_pypelc";
        var publicIdShelter2Img2 = "shelter_q44gwo";

        var publicIdAnimal1Img1 = "image2_gjkcko";
        var publicIdAnimal1Img2 = "images_fcbmbh";

        var publicIdAnimal2Img1 = "image2_da9jlw";
        var publicIdAnimal2Img2 = "images_t0jnkr";

        var publicIdAnimal3Img1 = "image2_fcck0q";
        var publicIdAnimal3Img2 = "images_jfawej";

        var publicIdAnimal4Img1 = "image2_qnjamf";
        var publicIdAnimal4Img2 = "images_jofy7m";

        var publicIdAnimal5Img1 = "image2_pxn6g2";
        var publicIdAnimal5Img2 = "images_rn6vpn";

        var publicIdAnimal6Img1 = "image2_rugk8b";
        var publicIdAnimal6Img2 = "images_jop2o1";

        var publicIdAnimal7Img1 = "images_mn2jce";
        var publicIdAnimal7Img2 = "image2_kk3max";

        // Shelters
        const string imageShelter1Img1Id = "00000000-0000-0000-0000-000000000101";
        const string imageShelter1Img2Id = "00000000-0000-0000-0000-000000000102";
        const string imageShelter2Img1Id = "00000000-0000-0000-0000-000000000201";
        const string imageShelter2Img2Id = "00000000-0000-0000-0000-000000000202";

        // Animals
        const string imageAnimal1Img1Id = "00000000-0000-0000-0000-000000001101";
        const string imageAnimal1Img2Id = "00000000-0000-0000-0000-000000001102";
        const string imageAnimal2Img1Id = "00000000-0000-0000-0000-000000002101";
        const string imageAnimal2Img2Id = "00000000-0000-0000-0000-000000002102";
        const string imageAnimal3Img1Id = "00000000-0000-0000-0000-000000003101";
        const string imageAnimal3Img2Id = "00000000-0000-0000-0000-000000003102";
        const string imageAnimal4Img1Id = "00000000-0000-0000-0000-000000004101";
        const string imageAnimal4Img2Id = "00000000-0000-0000-0000-000000004102";
        const string imageAnimal5Img1Id = "00000000-0000-0000-0000-000000005101";
        const string imageAnimal5Img2Id = "00000000-0000-0000-0000-000000005102";
        const string imageAnimal6Img1Id = "00000000-0000-0000-0000-000000006101";
        const string imageAnimal6Img2Id = "00000000-0000-0000-0000-000000006102";
        const string imageAnimal7Img1Id = "00000000-0000-0000-0000-000000007101";
        const string imageAnimal7Img2Id = "00000000-0000-0000-0000-000000007102";
        const string imageAnimal15Img1Id = "00000000-0000-0000-0000-000000015101"; // Notifications testing
        const string imageAnimal15Img2Id = "00000000-0000-0000-0000-000000015102"; // Notifications testing

        // Favorites test
        const string user7Id = "77777777-7777-7777-7777-777777777777"; // Gustavo (user with favorites)
        const string animal12Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd1d";
        const string animal13Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd2d";
        const string animal14Id = "f055cc31-fdeb-4c65-bb73-4f558f67dd3d";

        const string favorite1Id = "fav00000-0000-0000-0000-000000000001";
        const string favorite2Id = "fav00000-0000-0000-0000-000000000002";
        const string favorite3Id = "fav00000-0000-0000-0000-000000000003";

        // Favorites test, image ids
        const string imageAnimal12Img1Id = "00000000-0000-0000-0000-000000012101";
        const string imageAnimal12Img2Id = "00000000-0000-0000-0000-000000012102";
        const string imageAnimal13Img1Id = "00000000-0000-0000-0000-000000013101";
        const string imageAnimal13Img2Id = "00000000-0000-0000-0000-000000013102";
        const string imageAnimal14Img1Id = "00000000-0000-0000-0000-000000014101";
        const string imageAnimal14Img2Id = "00000000-0000-0000-0000-000000014102";

        if (resetDatabase && dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
        }

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
                },
                new() // Notifications testing
                {
                    Id = shelter3Id,
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
                },
                new()
                {
                    Id = user7Id,
                    Name = "Gustavo Pereira",
                    UserName = "gustavo@test.com",
                    Email = "gustavo@test.com",
                    City = "Lisboa",
                    Street = "Rua dos Favoritos 15",
                    PostalCode = "1200-100",
                    BirthDate = new DateTime(1993, 8, 20),
                    PhoneNumber = "918888777",
                    CreatedAt = DateTime.UtcNow
                },
                new() // Notifications testing - AdminCAA
                {
                    Id = user8Id,
                    Name = "Alice Notifications Admin",
                    UserName = "alice.notif@test.com",
                    Email = "alice.notif@test.com",
                    City = "Porto",
                    Street = "Rua dos Testes 100",
                    PostalCode = "4100-100",
                    BirthDate = new DateTime(1990, 1, 1),
                    PhoneNumber = "910000001",
                    CreatedAt = DateTime.UtcNow,
                    ShelterId = shelter3Id
                },
                new() // Notifications testing - User
                {
                    Id = user9Id,
                    Name = "Carlos Notifications User",
                    UserName = "carlos.notif@test.com",
                    Email = "carlos.notif@test.com",
                    City = "Porto",
                    Street = "Rua dos Testes 200",
                    PostalCode = "4100-200",
                    BirthDate = new DateTime(1992, 6, 8),
                    PhoneNumber = "910000002",
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
                        "alice@test.com" => adminCaa,
                        "filipe@test.com" => adminCaa,
                        "alice.notif@test.com" => adminCaa, // Notifications testing
                        _ => userRole
                    };

                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    logger.LogWarning("Erro ao criar utilizador {Email}: {Errors}", user.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
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
                    BirthDate = new DateOnly(2025, 2, 10),
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
                    BirthDate = new DateOnly(2025, 8, 22),
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
                    BirthDate = new DateOnly(2025, 6, 30),
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
                },
                new()
                {
                    Id = animal11Id,
                    Name = "Tobias",
                    AnimalState = AnimalState.Available,
                    Description = "Cão de porte médio, muito sociável e adora passeios longos.",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Preto e branco",
                    BirthDate = new DateOnly(2020, 6, 12),
                    Sterilized = true,
                    BreedId = breed2Id,
                    Cost = 60,
                    Features = "Brincalhão, curioso e adaptável a diferentes ambientes",
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
                // ========== ANIMALS FOR FAVORITE TESTING =====================
                new()
                {
                    Id = animal12Id,
                    Name = "Luna",
                    AnimalState = AnimalState.Available,
                    Description = "Gata carinhosa e tranquila, ideal para apartamento.",
                    Species = Species.Cat,
                    Size = SizeType.Small,
                    Sex = SexType.Female,
                    Colour = "Cinza prateado",
                    BirthDate = new DateOnly(2021, 2, 18),
                    Sterilized = true,
                    BreedId = breed1Id,
                    Cost = 35,
                    Features = "Pelagem sedosa, olhos verdes",
                    ShelterId = shelter1Id,
                    Images = new List<Image>()
                },
                new()
                {
                    Id = animal13Id,
                    Name = "Rex",
                    AnimalState = AnimalState.Available,
                    Description = "Cão ativo e brincalhão, adora crianças.",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Castanho avermelhado",
                    BirthDate = new DateOnly(2020, 9, 5),
                    Sterilized = true,
                    BreedId = breed2Id,
                    Cost = 55,
                    Features = "Muito energético, gosta de correr",
                    ShelterId = shelter1Id,
                    Images = new List<Image>()
                },
                new()
                {
                    Id = animal14Id,
                    Name = "Simba",
                    AnimalState = AnimalState.Available,
                    Description = "Gato jovem e curioso, adora explorar.",
                    Species = Species.Cat,
                    Size = SizeType.Small,
                    Sex = SexType.Male,
                    Colour = "Laranja tigrado",
                    BirthDate = new DateOnly(2022, 6, 10),
                    Sterilized = false,
                    BreedId = breed1Id,
                    Cost = 28,
                    Features = "Muito brincalhão e ativo",
                    ShelterId = shelter1Id,
                    Images = new List<Image>()
                },
                new() // Notifications testing
                {
                    Id = animal15Id,
                    Name = "NotifTestDog",
                    AnimalState = AnimalState.Available,
                    Description = "Animal exclusivo para testes de notificações",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Preto e Branco",
                    BirthDate = new DateOnly(2020, 1, 1),
                    Sterilized = true,
                    BreedId = breed2Id,
                    Cost = 50,
                    Features = "Animal de teste isolado - Notifications",
                    ShelterId = shelter3Id,
                    Images = new List<Image>()
                }
            };

            await dbContext.Animals.AddRangeAsync(animals);
            await dbContext.SaveChangesAsync();
        }

        // ======== IMAGES SEED ========

        if (!dbContext.Images.Any())
        {
            var images = new List<Image>
            {
                // === SHELTER 1 ===
                new()
                {
                    Id = imageShelter1Img1Id,
                    ShelterId = shelter1Id,
                    Url = imageShelterUrl1_1,
                    Description = "Fachada principal do CAA Porto",
                    IsPrincipal = true,
                    PublicId = publicIdShelter1Img1
                },
                new()
                {
                    Id = imageShelter1Img2Id,
                    ShelterId = shelter1Id,
                    Url = imageShelterUrl1_2,
                    Description = "Área exterior do abrigo",
                    IsPrincipal = false,
                    PublicId = publicIdShelter1Img2
                },

                // === SHELTER 2 ===
                new()
                {
                    Id = imageShelter2Img1Id,
                    ShelterId = shelter2Id,
                    Url = imageShelterUrl2_1,
                    Description = "Entrada principal do CAA de Cima",
                    IsPrincipal = true,
                    PublicId = publicIdShelter2Img1
                },
                new()
                {
                    Id = imageShelter2Img2Id,
                    ShelterId = shelter2Id,
                    Url = imageShelterUrl2_2,
                    Description = "Zona de descanso dos animais",
                    IsPrincipal = false,
                    PublicId = publicIdShelter2Img2
                },

                // === ANIMAL 1 ===
                new()
                {
                    Id = imageAnimal1Img1Id,
                    AnimalId = animal1Id,
                    Url = imageUrl1_1,
                    Description = "Bolinhas deitado ao sol",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal1Img1
                },
                new()
                {
                    Id = imageAnimal1Img2Id,
                    AnimalId = animal1Id,
                    Url = imageUrl1_2,
                    Description = "Bolinhas a brincar com bola",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal1Img2
                },

                // === ANIMAL 2 ===
                new()
                {
                    Id = imageAnimal2Img1Id,
                    AnimalId = animal2Id,
                    Url = imageUrl2_1,
                    Description = "Luna a correr no jardim",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal2Img1
                },
                new()
                {
                    Id = imageAnimal2Img2Id,
                    AnimalId = animal2Id,
                    Url = imageUrl2_2,
                    Description = "Luna a dormir tranquilamente",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal2Img2
                },

                // === ANIMAL 3 ===
                new()
                {
                    Id = imageAnimal3Img1Id,
                    AnimalId = animal3Id,
                    Url = imageUrl3_1,
                    Description = "Tico no poleiro",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal3Img1
                },
                new()
                {
                    Id = imageAnimal3Img2Id,
                    AnimalId = animal3Id,
                    Url = imageUrl3_2,
                    Description = "Tico a abrir as asas",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal3Img2
                },

                // === ANIMAL 4 ===
                new()
                {
                    Id = imageAnimal4Img1Id,
                    AnimalId = animal4Id,
                    Url = imageUrl4_1,
                    Description = "Mika deitada no sofá",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal4Img1
                },
                new()
                {
                    Id = imageAnimal4Img2Id,
                    AnimalId = animal4Id,
                    Url = imageUrl4_2,
                    Description = "Mika a brincar com uma corda",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal4Img2
                },

                // === ANIMAL 5 ===
                new()
                {
                    Id = imageAnimal5Img1Id,
                    AnimalId = animal5Id,
                    Url = imageUrl5_1,
                    Description = "Thor atento ao portão",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal5Img1
                },
                new()
                {
                    Id = imageAnimal5Img2Id,
                    AnimalId = animal5Id,
                    Url = imageUrl5_2,
                    Description = "Thor a correr no pátio",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal5Img2
                },

                // === ANIMAL 6 ===
                new()
                {
                    Id = imageAnimal6Img1Id,
                    AnimalId = animal6Id,
                    Url = imageUrl6_1,
                    Description = "Nina a comer cenoura",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal6Img1
                },
                new()
                {
                    Id = imageAnimal6Img2Id,
                    AnimalId = animal6Id,
                    Url = imageUrl6_2,
                    Description = "Nina a explorar o jardim",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal6Img2
                },

                // === ANIMAL 7 ===
                new()
                {
                    Id = imageAnimal7Img1Id,
                    AnimalId = animal7Id,
                    Url = imageUrl7_1,
                    Description = "Rockito a observar o horizonte",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal7Img1
                },
                new()
                {
                    Id = imageAnimal7Img2Id,
                    AnimalId = animal7Id,
                    Url = imageUrl7_2,
                    Description = "Rockito a brincar no campo",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal7Img2
                },

                // =============== FAVORITES IMAGE SEEDS ==================


                // === ANIMAL 12 (Luna) ===
                new()
                {
                    Id = imageAnimal12Img1Id,
                    AnimalId = animal12Id,
                    Url = imageUrl1_1,
                    Description = "Luna a descansar",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal1Img1
                },
                new()
                {
                    Id = imageAnimal12Img2Id,
                    AnimalId = animal12Id,
                    Url = imageUrl1_2,
                    Description = "Luna a brincar",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal1Img2
                },

                // === ANIMAL 13 (Rex) ===
                new()
                {
                    Id = imageAnimal13Img1Id,
                    AnimalId = animal13Id,
                    Url = imageUrl1_1,
                    Description = "Rex no jardim",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal1Img1
                },
                new()
                {
                    Id = imageAnimal13Img2Id,
                    AnimalId = animal13Id,
                    Url = imageUrl1_2,
                    Description = "Rex a correr",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal1Img2
                },

                // === ANIMAL 14 (Simba) ===
                new()
                {
                    Id = imageAnimal14Img1Id,
                    AnimalId = animal14Id,
                    Url = imageUrl1_1,
                    Description = "Simba a explorar",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal1Img1
                },
                new()
                {
                    Id = imageAnimal14Img2Id,
                    AnimalId = animal14Id,
                    Url = imageUrl1_2,
                    Description = "Simba a dormir",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal1Img2
                },

                // === ANIMAL 15 (NotifTestDog) - Notifications testing ===
                new()
                {
                    Id = imageAnimal15Img1Id,
                    AnimalId = animal15Id,
                    Url = imageUrl1_1,
                    Description = "NotifTestDog principal",
                    IsPrincipal = true,
                    PublicId = publicIdAnimal1Img1
                },
                new()
                {
                    Id = imageAnimal15Img2Id,
                    AnimalId = animal15Id,
                    Url = imageUrl1_2,
                    Description = "NotifTestDog secundário",
                    IsPrincipal = false,
                    PublicId = publicIdAnimal1Img2
                }
            };

            await dbContext.Images.AddRangeAsync(images);
            await dbContext.SaveChangesAsync();
        }

        // ======== SEED FOSTERINGS ========
        if (!dbContext.Fosterings.Any())
        {
            const string fostering1Id = "f0000000-0000-0000-0000-000000000001";
            const string fostering2Id = "f0000000-0000-0000-0000-000000000002";
            const string fostering3Id = "f0000000-0000-0000-0000-000000000003";

            var fosterings = new List<Fostering>
            {
                new()
                {
                    Id = fostering1Id,
                    AnimalId = animal2Id,
                    UserId = user4Id,
                    Amount = 10,
                    Status = FosteringStatus.Active,
                    StartDate = DateTime.UtcNow
                },
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

//======== SEED FOR OWNERSHIPS CONTROLLER ========
        // Create Users
        var user1 = new User
        {
            Id = "user-1",
            UserName = "user1@test.com",
            Email = "user1@test.com",
            Name = "João Silva",
            BirthDate = new DateTime(1990, 5, 15),
            Street = "Rua das Flores, 123",
            City = "Porto",
            PostalCode = "4000-001",
            PhoneNumber = "912345678",
            EmailConfirmed = true
        };


        var user2 = new User
        {
            Id = "user-2",
            UserName = "user2@test.com",
            Email = "user2@test.com",
            Name = "Maria Santos",
            BirthDate = new DateTime(1985, 8, 20),
            Street = "Avenida da Liberdade, 456",
            City = "Lisboa",
            PostalCode = "1250-001",
            PhoneNumber = "923456789",
            EmailConfirmed = true
        };

        var user3 = new User
        {
            Id = "user-3",
            UserName = "user3@test.com",
            Email = "user3@test.com",
            Name = "Carlos Pereira",
            BirthDate = new DateTime(1995, 3, 10),
            Street = "Rua do Comércio, 789",
            City = "Braga",
            PostalCode = "4700-001",
            PhoneNumber = "934567890",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user1, "Test@123");
        await userManager.CreateAsync(user2, "Test@123");
        await userManager.CreateAsync(user3, "Test@123");
        await userManager.AddToRoleAsync(user1, userRole);
        await userManager.AddToRoleAsync(user2, userRole);
        await userManager.AddToRoleAsync(user3, userRole);


        // Create Shelters
        var shelter1 = new Shelter
        {
            Id = "shelter-1",
            Name = "Associação Protetora dos Animais do Porto",
            Street = "Rua dos Animais, 100",
            City = "Porto",
            PostalCode = "4100-001",
            Phone = "222333444",
            NIF = "501234567",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var shelter2 = new Shelter
        {
            Id = "shelter-2",
            Name = "Centro de Recolha Animal de Lisboa",
            Street = "Avenida dos Bichos, 200",
            City = "Lisboa",
            PostalCode = "1300-001",
            Phone = "213444555",
            NIF = "502345678",
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(19, 0)
        };

        await dbContext.Shelters.AddRangeAsync(shelter1, shelter2);

        // Create Shelter Images
        var shelterImage1 = new Image
        {
            Id = "shelter-img-1",
            PublicId = "shelters/shelter1_main",
            Url = "https://example.com/shelter1.jpg",
            Description = "Foto principal do abrigo",
            IsPrincipal = true,
            ShelterId = shelter1.Id
        };

        var shelterImage2 = new Image
        {
            Id = "shelter-img-2",
            PublicId = "shelters/shelter2_main",
            Url = "https://example.com/shelter2.jpg",
            Description = "Foto principal do abrigo",
            IsPrincipal = true,
            ShelterId = shelter2.Id
        };

        await dbContext.Images.AddRangeAsync(shelterImage1, shelterImage2);

        // Create Breeds
        var breed1 = new Breed
        {
            Id = "breed-1",
            Name = "Labrador Retriever",
            Description = "Cão de porte médio a grande, muito amigável"
        };

        var breed2 = new Breed
        {
            Id = "breed-2",
            Name = "Golden Retriever",
            Description = "Cão grande, dócil e muito inteligente"
        };

        var breed3 = new Breed
        {
            Id = "breed-3",
            Name = "Pastor Português",
            Description = "Cão grande, protetor e leal"
        };

        var breed4 = new Breed
        {
            Id = "breed-4",
            Name = "Rafeiro",
            Description = "Cão de porte médio, curioso e enérgico"
        };

        await dbContext.Breeds.AddRangeAsync(breed1, breed2, breed3, breed4);
        await dbContext.SaveChangesAsync();

        // Create Animals WITHOUT owners (available for ownership requests)
        var animal1 = new Animal
        {
            Id = "animal-1",
            Name = "Rex",
            AnimalState = AnimalState.Available,
            Description = "Cão muito amigável e brincalhão",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Dourado",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = true,
            Cost = 50,
            Features = "Adora crianças, muito energético",
            ShelterId = shelter1.Id,
            BreedId = breed1.Id,
            OwnerId = null // No owner yet
        };

        var animal2 = new Animal
        {
            Id = "animal-2",
            Name = "Bella",
            AnimalState = AnimalState.Available,
            Description = "Cadela dócil e carinhosa",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Female,
            Colour = "Castanho",
            BirthDate = new DateOnly(2019, 7, 20),
            Sterilized = true,
            Cost = 45,
            Features = "Muito calma, ideal para apartamento",
            ShelterId = shelter1.Id,
            BreedId = breed2.Id,
            OwnerId = null
        };

        var animal3 = new Animal
        {
            Id = "animal-3",
            Name = "Thor",
            AnimalState = AnimalState.Available,
            Description = "Cão protetor e leal",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Preto e castanho",
            BirthDate = new DateOnly(2018, 11, 10),
            Sterilized = false,
            Cost = 60,
            Features = "Precisa de espaço, bom guarda",
            ShelterId = shelter2.Id,
            BreedId = breed3.Id,
            OwnerId = null
        };

        // Additional animals for testing different ownership request scenarios
        var animal7 = new Animal
        {
            Id = "animal-7",
            Name = "Simba",
            AnimalState = AnimalState.Available,
            Description = "Cão jovem e cheio de energia",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Laranja",
            BirthDate = new DateOnly(2022, 6, 8),
            Sterilized = false,
            Cost = 38,
            Features = "Adora brincar, precisa de treino",
            ShelterId = shelter2.Id,
            BreedId = breed4.Id,
            OwnerId = null
        };

        var animal8 = new Animal
        {
            Id = "animal-8",
            Name = "Nina",
            AnimalState = AnimalState.Available,
            Description = "Cadela idosa e calma",
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Cinzento",
            BirthDate = new DateOnly(2015, 2, 14),
            Sterilized = true,
            Cost = 30,
            Features = "Perfeita para lares tranquilos",
            ShelterId = shelter1.Id,
            BreedId = breed1.Id,
            OwnerId = null
        };


        // Create Animals WITH owners (user2 owns these)
        var animal4 = new Animal
        {
            Id = "animal-4",
            Name = "Max",
            AnimalState = AnimalState.HasOwner,
            Description = "Cão adorável já adotado",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Branco e preto",
            BirthDate = new DateOnly(2021, 1, 5),
            Sterilized = true,
            Cost = 40,
            Features = "Ama correr, muito sociável",
            ShelterId = shelter1.Id,
            BreedId = breed4.Id,
            OwnerId = user2.Id,
            OwnershipStartDate = DateTime.UtcNow.AddMonths(-6)
        };

        var animal5 = new Animal
        {
            Id = "animal-5",
            Name = "Luna",
            AnimalState = AnimalState.HasOwner,
            Description = "Cadela já com família",
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Dourado claro",
            BirthDate = new DateOnly(2022, 4, 12),
            Sterilized = true,
            Cost = 35,
            Features = "Pequena e adorável, boa com gatos",
            ShelterId = shelter2.Id,
            BreedId = breed1.Id,
            OwnerId = user2.Id,
            OwnershipStartDate = DateTime.UtcNow.AddMonths(-2)
        };

        var animal6 = new Animal
        {
            Id = "animal-6",
            Name = "Bobby",
            AnimalState = AnimalState.HasOwner,
            Description = "Cão feliz com novo dono",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Dourado",
            BirthDate = new DateOnly(2019, 9, 25),
            Sterilized = true,
            Cost = 55,
            Features = "Calmo, ideal para idosos",
            ShelterId = shelter1.Id,
            BreedId = breed2.Id,
            OwnerId = user2.Id,
            OwnershipStartDate = DateTime.UtcNow.AddMonths(-10)
        };

        await dbContext.Animals.AddRangeAsync(animal1, animal2, animal3, animal4, animal5, animal6, animal7, animal8);


        // Create Animal Images
        var animalImages = new List<Image>
        {
            new Image
            {
                Id = "img-1", PublicId = "animals/rex_1", Url = "https://example.com/rex1.jpg",
                Description = "Rex brincando", IsPrincipal = true, AnimalId = animal1.Id
            },
            new Image
            {
                Id = "img-2", PublicId = "animals/rex_2", Url = "https://example.com/rex2.jpg",
                Description = "Rex descansando", IsPrincipal = false, AnimalId = animal1.Id
            },
            new Image
            {
                Id = "img-3", PublicId = "animals/bella_1", Url = "https://example.com/bella1.jpg",
                Description = "Bella sentada", IsPrincipal = true, AnimalId = animal2.Id
            },
            new Image
            {
                Id = "img-4", PublicId = "animals/thor_1", Url = "https://example.com/thor1.jpg",
                Description = "Thor em guarda", IsPrincipal = true, AnimalId = animal3.Id
            },
            new Image
            {
                Id = "img-5", PublicId = "animals/max_1", Url = "https://example.com/max1.jpg",
                Description = "Max feliz", IsPrincipal = true, AnimalId = animal4.Id
            },
            new Image
            {
                Id = "img-6", PublicId = "animals/luna_1", Url = "https://example.com/luna1.jpg",
                Description = "Luna adorável", IsPrincipal = true, AnimalId = animal5.Id
            },
            new Image
            {
                Id = "img-7", PublicId = "animals/bobby_1", Url = "https://example.com/bobby1.jpg",
                Description = "Bobby sorrindo", IsPrincipal = true, AnimalId = animal6.Id
            },
            new Image
            {
                Id = "img-8", PublicId = "animals/simba_1", Url = "https://example.com/simba1.jpg",
                Description = "Simba brincalhão", IsPrincipal = true, AnimalId = animal7.Id
            },
            new Image
            {
                Id = "img-9", PublicId = "animals/nina_1", Url = "https://example.com/nina1.jpg",
                Description = "Nina tranquila", IsPrincipal = true, AnimalId = animal8.Id
            }
        };

        await dbContext.Images.AddRangeAsync(animalImages);
        await dbContext.SaveChangesAsync();

        // Create Ownership Requests for user1 (multiple states)
        // Each user can only have ONE ownership request per animal (unique constraint on UserId + AnimalId)

        // Pending request for Rex

        var ownershipRequest1 = new OwnershipRequest
        {
            Id = "or-1",
            AnimalId = animal1.Id, // Rex
            UserId = user1.Id,
            Amount = 100,
            Status = OwnershipStatus.Pending,
            RequestInfo = "Tenho experiência com cães desta raça",
            RequestedAt = DateTime.UtcNow.AddDays(-5)
        };

        // Pending request for Bella
        var ownershipRequest2 = new OwnershipRequest
        {
            Id = "or-2",
            AnimalId = animal2.Id, // Bella
            UserId = user1.Id,
            Amount = 90,
            Status = OwnershipStatus.Pending,
            RequestInfo = "Procuro uma companheira calma",
            RequestedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Recent rejected request for Thor (within last month)
        var ownershipRequest3 = new OwnershipRequest
        {
            Id = "or-3",
            AnimalId = animal3.Id, // Thor
            UserId = user1.Id,
            Amount = 120,
            Status = OwnershipStatus.Rejected,
            RequestInfo = "Tenho quintal grande",
            RequestedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-15) // Rejected 15 days ago (within last month)
        };

        // Old rejected request for Simba (should NOT appear - more than 1 month old)
        var ownershipRequest4 = new OwnershipRequest
        {
            Id = "or-4",
            AnimalId = animal7.Id, // Simba (different animal!)
            UserId = user1.Id,
            Amount = 80,
            Status = OwnershipStatus.Rejected,
            RequestInfo = "Quero um cão jovem",
            RequestedAt = DateTime.UtcNow.AddDays(-50),
            UpdatedAt = DateTime.UtcNow.AddDays(-40) // Rejected 40 days ago (more than 1 month)
        };

        // Approved request for Nina (should NOT appear - approved requests excluded)
        var ownershipRequest5 = new OwnershipRequest
        {
            Id = "or-5",
            AnimalId = animal8.Id, // Nina (different animal!)
            UserId = user1.Id,
            Amount = 95,
            Status = OwnershipStatus.Approved,
            RequestInfo = "Perfeita para o meu estilo de vida",
            RequestedAt = DateTime.UtcNow.AddDays(-60),
            ApprovedAt = DateTime.UtcNow.AddDays(-55),
            UpdatedAt = DateTime.UtcNow.AddDays(-55)
        };

        await dbContext.OwnershipRequests.AddRangeAsync(
            ownershipRequest1,
            ownershipRequest2,
            ownershipRequest3,
            ownershipRequest4,
            ownershipRequest5
        );
        await dbContext.SaveChangesAsync();

        // ======== SEED FAVORITES ========
        if (!dbContext.Favorites.Any())
        {
            const string favorite4Id = "fav00000-0000-0000-0000-000000000004";

            var favorites = new List<Favorite>
            {
                new()
                {
                    Id = favorite1Id,
                    UserId = user7Id, // Gustavo
                    AnimalId = animal12Id, // Luna
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = favorite2Id,
                    UserId = user7Id, // Gustavo
                    AnimalId = animal13Id, // Rex
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = favorite3Id,
                    UserId = user7Id, // Gustavo
                    AnimalId = animal14Id, // Simba
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = favorite4Id,
                    UserId = user7Id, // Gustavo
                    AnimalId = "f055cc31-fdeb-4c65-bb73-4f558f67dd1b", // Bolinhas
                    IsActive = false, // INATIVO
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                }
            };

            await dbContext.Favorites.AddRangeAsync(favorites);
            await dbContext.SaveChangesAsync();
        }

// ======== ACTIVITIES & SLOTS ========
        var baseDate = new DateTime(2025, 11, 3, 0, 0, 0, DateTimeKind.Utc);

        const string activityAId = "activity-seed-a";
        const string activityBId = "activity-seed-b";
        const string activityCId = "activity-seed-c";
        const string activityDId = "activity-seed-d";
        const string activityEId = "activity-seed-e";
        const string activityFId = "activity-seed-f";
        const string activityGId = "activity-seed-g";
        const string activityHId = "activity-seed-h";
        const string activityIId = "activity-seed-i";
        const string activityJId = "activity-seed-j";
        const string activityKId = "activity-seed-k";

        const string slotNormal1 = "slot-normal-1";
        const string slotNormal2 = "slot-normal-2";
        const string slotNormal3 = "slot-normal-3";
        const string slotNormal4 = "slot-normal-4";
        const string slotNormal5 = "slot-normal-5";

        const string slotEdgeStartBefore = "slot-edge-start-before";
        const string slotEdgeEndAfter = "slot-edge-end-after";
        const string slotEdgeExactOpen = "slot-edge-exact-open";
        const string slotEdgeExactClose = "slot-edge-exact-close";

        const string slotMultiDay1 = "slot-multiday-1";
        const string slotMultiDay2 = "slot-multiday-2";

        const string slotShort1 = "slot-short-1";
        const string slotShort2 = "slot-short-2";

        const string slotOverlap1 = "slot-overlap-1";
        const string slotOverlap2 = "slot-overlap-2";

        const string slotSameDay1 = "slot-same-day-1";
        const string slotSameDay2 = "slot-same-day-2";
        const string slotSameDay3 = "slot-same-day-3";

        const string unavShort = "unav-short";
        const string unavHalfDay = "unav-half-day";
        const string unavFullDay = "unav-full-day";
        const string unavWeek = "unav-week";
        const string unavMultiDay = "unav-multi-day";
        const string unavBeforeOpen = "unav-before-open";
        const string unavAfterClose = "unav-after-close";
        const string unavExactHours = "unav-exact-hours";
        const string unavMidnight = "unav-midnight";

        var activities = new List<Activity>
        {
            new Activity
            {
                Id = activityAId,
                AnimalId = animal3Id,
                UserId = user1Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityBId,
                AnimalId = animal4Id,
                UserId = user2Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityCId,
                AnimalId = animal3Id,
                UserId = user3Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityDId,
                AnimalId = animal4Id,
                UserId = user4Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(4),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityEId,
                AnimalId = animal5Id,
                UserId = user5Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate,
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityFId,
                AnimalId = animal3Id,
                UserId = user2Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(2),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityGId,
                AnimalId = animal4Id,
                UserId = user3Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(2),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityHId,
                AnimalId = animal6Id,
                UserId = user1Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityIId,
                AnimalId = animal7Id,
                UserId = user4Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityJId,
                AnimalId = animal8Id,
                UserId = user5Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            },
            new Activity
            {
                Id = activityKId,
                AnimalId = animal5Id,
                UserId = user6Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(1),
                EndDate = baseDate.AddMonths(3),
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Activities.AddRangeAsync(activities);

        var activitySlots = new List<ActivitySlot>
        {
            new ActivitySlot
            {
                Id = slotNormal1,
                ActivityId = activityAId,
                StartDateTime = baseDate.AddDays(1).AddHours(10),
                EndDateTime = baseDate.AddDays(1).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotNormal2,
                ActivityId = activityBId,
                StartDateTime = baseDate.AddDays(1).AddHours(14),
                EndDateTime = baseDate.AddDays(1).AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotNormal3,
                ActivityId = activityCId,
                StartDateTime = baseDate.AddDays(2).AddHours(9).AddMinutes(30),
                EndDateTime = baseDate.AddDays(2).AddHours(11).AddMinutes(30),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotNormal4,
                ActivityId = activityDId,
                StartDateTime = baseDate.AddDays(3).AddHours(13),
                EndDateTime = baseDate.AddDays(3).AddHours(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotNormal5,
                ActivityId = activityEId,
                StartDateTime = baseDate.AddDays(4).AddHours(10),
                EndDateTime = baseDate.AddDays(4).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },

            // Edge cases
            new ActivitySlot
            {
                Id = slotEdgeStartBefore,
                ActivityId = activityFId,
                StartDateTime = baseDate.AddDays(5).AddHours(7), // antes de abrir
                EndDateTime = baseDate.AddDays(5).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotEdgeEndAfter,
                ActivityId = activityGId,
                StartDateTime = baseDate.AddDays(5).AddHours(17),
                EndDateTime = baseDate.AddDays(5).AddHours(20), // depois de fechar
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotEdgeExactOpen,
                ActivityId = activityHId,
                StartDateTime = baseDate.AddDays(6).AddHours(9), // exatamente na abertura
                EndDateTime = baseDate.AddDays(6).AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotEdgeExactClose,
                ActivityId = activityIId,
                StartDateTime = baseDate.AddDays(6).AddHours(16),
                EndDateTime = baseDate.AddDays(6).AddHours(18), // exatamente no fecho
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },

            // Slots multi-dia
            new ActivitySlot
            {
                Id = slotMultiDay1,
                ActivityId = activityJId,
                StartDateTime = baseDate.AddDays(7).AddHours(15),
                EndDateTime = baseDate.AddDays(8).AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotMultiDay2,
                ActivityId = activityKId,
                StartDateTime = baseDate.AddDays(9).AddHours(16),
                EndDateTime = baseDate.AddDays(10).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },

            // Slots curtos (15min)
            new ActivitySlot
            {
                Id = slotShort1,
                ActivityId = activityAId,
                StartDateTime = baseDate.AddDays(11).AddHours(11),
                EndDateTime = baseDate.AddDays(11).AddHours(11).AddMinutes(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotShort2,
                ActivityId = activityBId,
                StartDateTime = baseDate.AddDays(11).AddHours(14).AddMinutes(30),
                EndDateTime = baseDate.AddDays(11).AddHours(14).AddMinutes(45),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },

            // Overlaps
            new ActivitySlot
            {
                Id = slotOverlap1,
                ActivityId = activityCId,
                StartDateTime = baseDate.AddDays(12).AddHours(10),
                EndDateTime = baseDate.AddDays(12).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotOverlap2,
                ActivityId = activityCId,
                StartDateTime = baseDate.AddDays(12).AddHours(11), // sobrepõe com o anterior
                EndDateTime = baseDate.AddDays(12).AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },

            // Vários no mesmo dia
            new ActivitySlot
            {
                Id = slotSameDay1,
                ActivityId = activityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(9),
                EndDateTime = baseDate.AddDays(13).AddHours(10),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotSameDay2,
                ActivityId = activityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(11),
                EndDateTime = baseDate.AddDays(13).AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            },
            new ActivitySlot
            {
                Id = slotSameDay3,
                ActivityId = activityDId,
                StartDateTime = baseDate.AddDays(13).AddHours(15),
                EndDateTime = baseDate.AddDays(13).AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Set<ActivitySlot>().AddRangeAsync(activitySlots);

        // ====== SHELTER UNAVAILABILITY SLOTS (usar shelters existentes) ======
        var shelterUnavailabilitySlots = new List<ShelterUnavailabilitySlot>
        {
            new ShelterUnavailabilitySlot
            {
                Id = unavShort,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(1).AddHours(12),
                EndDateTime = baseDate.AddDays(1).AddHours(13),
                Reason = "Pausa de almoço",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavHalfDay,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(2).AddHours(9),
                EndDateTime = baseDate.AddDays(2).AddHours(14),
                Reason = "Formação de manhã",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavFullDay,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(3).AddHours(9),
                EndDateTime = baseDate.AddDays(3).AddHours(18),
                Reason = "Evento de dia inteiro",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavWeek,
                ShelterId = shelter2Id,
                StartDateTime = baseDate.AddDays(21),
                EndDateTime = baseDate.AddDays(28),
                Reason = "Semana de férias",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavMultiDay,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(14),
                EndDateTime = baseDate.AddDays(17),
                Reason = "Manutenção alargada",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavBeforeOpen,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(7).AddHours(7),
                EndDateTime = baseDate.AddDays(7).AddHours(10),
                Reason = "Preparação cedo",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavAfterClose,
                ShelterId = shelter2Id,
                StartDateTime = baseDate.AddDays(8).AddHours(16),
                EndDateTime = baseDate.AddDays(8).AddHours(20),
                Reason = "Evento ao final do dia",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavExactHours,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(9).AddHours(9),
                EndDateTime = baseDate.AddDays(9).AddHours(18),
                Reason = "Encerramento total no horário",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            },
            new ShelterUnavailabilitySlot
            {
                Id = unavMidnight,
                ShelterId = shelter1Id,
                StartDateTime = baseDate.AddDays(10).AddHours(22),
                EndDateTime = baseDate.AddDays(11).AddHours(3),
                Reason = "Manutenção noturna",
                Status = SlotStatus.Unavailable,
                Type = SlotType.ShelterUnavailable,
                CreatedAt = DateTime.UtcNow
            }
        };

        await dbContext.Set<ShelterUnavailabilitySlot>().AddRangeAsync(shelterUnavailabilitySlots);

        await dbContext.SaveChangesAsync();

        // ============================================
        // DATA FOR TESTING FOSTERING ACTIVITIES
        // ============================================

        // Skip if data already exists
        if (dbContext.Users.Any(u => u.Email == "foster@test.com")) return;

        // ============================================
        // 1. CREATE USERS
        // ============================================

        // Foster User - has active fosterings
        var fosterUser = new User
        {
            Id = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
            UserName = "foster@test.com",
            Email = "foster@test.com",
            EmailConfirmed = true,
            Name = "Foster Test User",
            BirthDate = new DateTime(1990, 1, 1),
            Street = "Rua dos Fosters 123",
            City = "Porto",
            PostalCode = "4000-001",
            PhoneNumber = "912345678",
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(fosterUser, "Pa$$w0rd");
        await userManager.AddToRoleAsync(fosterUser, userRole);


        // Regular User - no fosterings
        var regularUser = new User
        {
            Id = "b2c3d4e5-f6a7-4b6c-9d0e-1f2a3b4c5d6e",
            UserName = "regular@test.com",
            Email = "regular@test.com",
            EmailConfirmed = true,
            Name = "Regular Test User",
            BirthDate = new DateTime(1992, 5, 15),
            Street = "Rua Regular 456",
            City = "Lisboa",
            PostalCode = "1000-001",
            PhoneNumber = "913456789",
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(regularUser, "Pa$$w0rd");
        await userManager.AddToRoleAsync(regularUser, userRole);

        // ============================================
        // 2. CREATE SHELTER
        // ============================================

        var shelter = new Shelter
        {
            Id = "c3d4e5f6-a7b8-4c7d-0e1f-2a3b4c5d6e7f",
            Name = "Test Shelter Porto",
            Street = "Rua do Abrigo 789",
            City = "Porto",
            PostalCode = "4100-001",
            Phone = "223456789",
            NIF = "674498653",
            OpeningTime = new TimeOnly(9, 0), // 09:00
            ClosingTime = new TimeOnly(18, 0), // 18:00
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Shelters.Add(shelter);

        // ============================================
        // 3. CREATE BREED
        // ============================================

        var breed = new Breed
        {
            Id = "d4e5f6a7-b8c9-4d8e-1f2a-3b4c5d6e7f8a",
            Name = "Test Breed",
        };
        dbContext.Breeds.Add(breed);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 4. CREATE ANIMALS
        // ============================================

        // Animal 1: Valid for fostering - no conflicts
        var animalF1 = new Animal
        {
            Id = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            Name = "Rex",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = true,
            Cost = 50,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalF1);

        // Animal 2: Valid for fostering - no conflicts
        var animalF2 = new Animal
        {
            Id = "f6a7b8c9-d0e1-4f0a-3b4c-5d6e7f8a9b0c",
            Name = "Luna",
            AnimalState = AnimalState.TotallyFostered,
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "White",
            BirthDate = new DateOnly(2021, 6, 20),
            Sterilized = true,
            Cost = 40,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalF2);

        // Animal 3: Valid for fostering - no conflicts
        var animalF3 = new Animal
        {
            Id = "animal-foster-003",
            Name = "Max",
            AnimalState = AnimalState.TotallyFostered,
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Black",
            BirthDate = new DateOnly(2019, 11, 10),
            Sterilized = true,
            Cost = 60,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalF3);

        // Animal 4: Inactive state (should fail validation)
        var animalInactive = new Animal
        {
            Id = "a7b8c9d0-e1f2-4a1b-4c5d-6e7f8a9b0c1d",
            Name = "Inactive Dog",
            AnimalState = AnimalState.Inactive,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Gray",
            BirthDate = new DateOnly(2018, 1, 1),
            Sterilized = true,
            Cost = 45,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalInactive);

        // Animal 5: Available state (should fail validation)
        var animalAvailable = new Animal
        {
            Id = "b8c9d0e1-f2a3-4b2c-5d6e-7f8a9b0c1d2e",
            Name = "Available Dog",
            AnimalState = AnimalState.Available,
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Orange",
            BirthDate = new DateOnly(2022, 4, 5),
            Sterilized = false,
            Cost = 35.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalAvailable);

        // Animal 6: Has existing activity slot (conflict test)
        var animalWithSlot = new Animal
        {
            Id = "c9d0e1f2-a3b4-4c3d-6e7f-8a9b0c1d2e3f",
            Name = "Busy Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Golden",
            BirthDate = new DateOnly(2020, 8, 12),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalWithSlot);

        // Animal 7: Has existing activity (conflict test)
        var animalWithActivity = new Animal
        {
            Id = "d0e1f2a3-b4c5-4d4e-7f8a-9b0c1d2e3f4a",
            Name = "Active Dog",
            AnimalState = AnimalState.TotallyFostered,
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Spotted",
            BirthDate = new DateOnly(2021, 2, 28),
            Sterilized = true,
            Cost = 42.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalWithActivity);

        var animalNotFostered = new Animal
        {
            Id = "e1f2a3b4-c5d6-4e5f-8a9b-0c1d2e3f4a5b",
            Name = "Not Fostered Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "White",
            BirthDate = new DateOnly(2020, 5, 10),
            Sterilized = true,
            Cost = 45.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalNotFostered);

        // Animal para teste de shelter unavailability (SEM conflitos)
        var animalShelterTest = new Animal
        {
            Id = "f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c",
            Name = "Buddy - Shelter Test",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalShelterTest);
        await dbContext.SaveChangesAsync();

        // ============================================
        // 5. CREATE IMAGES
        // ============================================

        var image1 = new Image
        {
            Id = "f2a3b4c5-d6e7-4f6a-9b0c-1d2e3f4a5b6c",
            PublicId = "test/rex-image",
            Url = "https://example.com/rex.jpg",
            Description = "Rex principal image",
            IsPrincipal = true,
            AnimalId = animalF1.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Images.Add(image1);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 5. CREATE FOSTERINGS
        // ============================================


        // Active fostering for animal 1
        var fostering1 = new Fostering
        {
            Id = "a3b4c5d6-e7f8-4a7b-0c1d-2e3f4a5b6c7d",
            AnimalId = animalF1.Id,
            UserId = fosterUser.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fostering1);

        // Active fostering for animal 2
        var fostering2 = new Fostering
        {
            Id = "b4c5d6e7-f8a9-4b8c-1d2e-3f4a5b6c7d8e",
            AnimalId = animalF2.Id,
            UserId = fosterUser.Id,
            Amount = 40,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-2)
        };
        dbContext.Fosterings.Add(fostering2);

        // Active fostering for animal 3
        var fostering3 = new Fostering
        {
            Id = "fostering-003",
            AnimalId = animalF3.Id,
            UserId = fosterUser.Id,
            Amount = 60,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fostering3);

        // Active fostering for inactive animal (to test state validation)
        var fosteringInactive = new Fostering
        {
            Id = "c5d6e7f8-a9b0-4c9d-2e3f-4a5b6c7d8e9f",
            AnimalId = animalInactive.Id,
            UserId = fosterUser.Id,
            Amount = 45,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringInactive);

        // Active fostering for animal with slot
        var fosteringWithSlot = new Fostering
        {
            Id = "d6e7f8a9-b0c1-4d0e-3f4a-5b6c7d8e9f0a",
            AnimalId = animalWithSlot.Id,
            UserId = fosterUser.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringWithSlot);

        // Active fostering for animal with activity
        var fosteringWithActivity = new Fostering
        {
            Id = "e7f8a9b0-c1d2-4e1f-4a5b-6c7d8e9f0a1b",
            AnimalId = animalWithActivity.Id,
            UserId = fosterUser.Id,
            Amount = 42,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringWithActivity);

        // Fostering para este animal
        var fosteringShelterTest = new Fostering
        {
            Id = "f9a0b1c2-d3e4-4f5a-6b7c-8d9e0f1a2b3c",
            AnimalId = animalShelterTest.Id,
            UserId = fosterUser.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringShelterTest);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 6. CREATE SHELTER UNAVAILABILITY SLOT
        // ============================================
        var twoDaysFromNow = DateTime.UtcNow.Date.AddDays(2);
        // Shelter unavailable tomorrow from 14:00 to 16:00
        var shelterUnavailability = new ShelterUnavailabilitySlot
        {
            Id = "f8a9b0c1-d2e3-4f2a-5b6c-7d8e9f0a1b2c",
            ShelterId = shelter.Id,
            StartDateTime = twoDaysFromNow.AddHours(14),
            EndDateTime = twoDaysFromNow.AddHours(16),
            Status = SlotStatus.Reserved,
            Type = SlotType.ShelterUnavailable,
            Reason = "Maintenance",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ShelterUnavailabilitySlots.Add(shelterUnavailability);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 7. CREATE EXISTING ACTIVITY + SLOT (for conflict tests)
        // ============================================

        // Activity for animalWithSlot - tomorrow 10:00 to 12:00
        var existingActivity1 = new Activity
        {
            Id = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d",
            AnimalId = animalWithSlot.Id,
            UserId = fosterUser.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(existingActivity1);

        await dbContext.SaveChangesAsync();

        // ActivitySlot for the existing activity
        var existingSlot1 = new ActivitySlot
        {
            Id = "b0c1d2e3-f4a5-4b4c-7d8e-9f0a1b2c3d4e",
            ActivityId = existingActivity1.Id,
            StartDateTime = twoDaysFromNow.AddHours(10),
            EndDateTime = twoDaysFromNow.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(existingSlot1);

        // Activity for animalWithActivity - tomorrow 10:00 to 14:00
        var existingActivity2 = new Activity
        {
            Id = "c1d2e3f4-a5b6-4c5d-8e9f-0a1b2c3d4e5f",
            AnimalId = animalWithActivity.Id,
            UserId = fosterUser.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(14),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(existingActivity2);

        await dbContext.SaveChangesAsync();

        Console.WriteLine("✅ Foster Activity test seed data created successfully!");

        // ============================================
        // SEED DATA FOR CANCELING FOSTERING ACTIVITY
        // ============================================

        // ============================================
        // 1. CREATE USERS
        // ============================================

        var fosterUserC = new User
        {
            Id = "c1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c",
            UserName = "cancel-foster@test.com",
            Email = "cancel-foster@test.com",
            EmailConfirmed = true,
            Name = "Cancel Foster User",
            BirthDate = new DateTime(1990, 1, 1),
            Street = "Rua Cancel 123",
            City = "Porto",
            PostalCode = "4000-001",
            PhoneNumber = "912345678",
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(fosterUserC, "Pa$$w0rd");
        await userManager.AddToRoleAsync(fosterUserC, userRole);

        var otherUser = new User
        {
            Id = "d2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d",
            UserName = "other-cancel@test.com",
            Email = "other-cancel@test.com",
            EmailConfirmed = true,
            Name = "Other Cancel User",
            BirthDate = new DateTime(1992, 5, 15),
            Street = "Rua Other 456",
            City = "Lisboa",
            PostalCode = "1000-001",
            PhoneNumber = "913456789",
            CreatedAt = DateTime.UtcNow
        };
        await userManager.CreateAsync(otherUser, "Pa$$w0rd");
        await userManager.AddToRoleAsync(otherUser, userRole);

        // ============================================
        // 2. CREATE SHELTER
        // ============================================

        var shelterC = new Shelter
        {
            Id = "e3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
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
        dbContext.Shelters.Add(shelterC);

        // ============================================
        // 3. CREATE BREED
        // ============================================

        var breedC = new Breed
        {
            Id = "f4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f",
            Name = "Cancel Test Breed",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Breeds.Add(breedC);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 4. CREATE ANIMALS
        // ============================================


        // Animal 1: Valid cancellation - active fostering, future activity
        var animalC1 = new Animal
        {
            Id = "a5e6f7a8-b9c0-4d1e-2f3a-4b5c6d7e8f9a",
            Name = "Cancel Rex",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC1);

        // Animal 2: For testing "not my activity"
        var animalC2 = new Animal
        {
            Id = "b6f7a8b9-c0d1-4e2f-3a4b-5c6d7e8f9a0b",
            Name = "Other User Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "White",
            BirthDate = new DateOnly(2021, 6, 20),
            Sterilized = true,
            Cost = 40,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC2);

        // Animal 3: For testing already cancelled activity
        var animalC3 = new Animal
        {
            Id = "c7a8b9c0-d1e2-4f3a-4b5c-6d7e8f9a0b1c",
            Name = "Cancelled Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Black",
            BirthDate = new DateOnly(2019, 11, 10),
            Sterilized = true,
            Cost = 60,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC3);

        // Animal 4: For testing completed activity
        var animalC4 = new Animal
        {
            Id = "d8b9c0d1-e2f3-4a4b-5c6d-7e8f9a0b1c2d",
            Name = "Completed Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Orange",
            BirthDate = new DateOnly(2022, 4, 5),
            Sterilized = false,
            Cost = 35,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC4);

        // Animal 5: For testing past activity (already started)
        var animalC5 = new Animal
        {
            Id = "e9c0d1e2-f3a4-4b5c-6d7e-8f9a0b1c2d3e",
            Name = "Past Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Golden",
            BirthDate = new DateOnly(2020, 8, 12),
            Sterilized = true,
            Cost = 50,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC5);

        // Animal 6: For testing without active fostering
        var animalC6 = new Animal
        {
            Id = "f0d1e2f3-a4b5-4c6d-7e8f-9a0b1c2d3e4f",
            Name = "No Fostering Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Small,
            Sex = SexType.Female,
            Colour = "Spotted",
            BirthDate = new DateOnly(2021, 2, 28),
            Sterilized = true,
            Cost = 42,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC6);

        // Animal 7: For testing ownership activity (not fostering)
        var animalC7 = new Animal
        {
            Id = "a1e2f3a4-b5c6-4d7e-8f9a-0b1c2d3e4f5a",
            Name = "Ownership Dog",
            AnimalState = AnimalState.HasOwner,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "White",
            BirthDate = new DateOnly(2020, 5, 10),
            Sterilized = true,
            Cost = 45,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC7);

        // Animal 8: For testing slot with Available status
        var animalC8 = new Animal
        {
            Id = "b2f3a4b5-c6d7-4e8f-9a0b-1c2d3e4f5a6b",
            Name = "Available Slot Dog",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Gray",
            BirthDate = new DateOnly(2020, 7, 15),
            Sterilized = true,
            Cost = 50,
            ShelterId = shelterC.Id,
            BreedId = breedC.Id,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Animals.Add(animalC8);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 5. CREATE FOSTERINGS
        // ============================================

        // Active fostering for animal 1
        var fosteringC1 = new Fostering
        {
            Id = "c3a4b5c6-d7e8-4f9a-0b1c-2d3e4f5a6b7c",
            AnimalId = animalC1.Id,
            UserId = fosterUserC.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC1);

        // Active fostering for animal 2 (other user)
        var fosteringC2 = new Fostering
        {
            Id = "d4b5c6d7-e8f9-4a0b-1c2d-3e4f5a6b7c8d",
            AnimalId = animalC2.Id,
            UserId = otherUser.Id,
            Amount = 40,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-2)
        };
        dbContext.Fosterings.Add(fosteringC2);

        // Active fostering for animal 3
        var fosteringC3 = new Fostering
        {
            Id = "e5c6d7e8-f9a0-4b1c-2d3e-4f5a6b7c8d9e",
            AnimalId = animalC3.Id,
            UserId = fosterUserC.Id,
            Amount = 60,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC3);

        // Active fostering for animal 4
        var fosteringC4 = new Fostering
        {
            Id = "f6d7e8f9-a0b1-4c2d-3e4f-5a6b7c8d9e0f",
            AnimalId = animalC4.Id,
            UserId = fosterUserC.Id,
            Amount = 35,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC4);

        // Active fostering for animal 5
        var fosteringC5 = new Fostering
        {
            Id = "a7e8f9a0-b1c2-4d3e-4f5a-6b7c8d9e0f1a",
            AnimalId = animalC5.Id,
            UserId = fosterUserC.Id,
            Amount = 50.00m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC5);

        // Cancelled fostering for animal 6 (to test no active fostering)
        var fosteringC6 = new Fostering
        {
            Id = "b8f9a0b1-c2d3-4e4f-5a6b-7c8d9e0f1a2b",
            AnimalId = animalC6.Id,
            UserId = fosterUserC.Id,
            Amount = 42.00m,
            Status = FosteringStatus.Cancelled,
            StartDate = DateTime.UtcNow.AddMonths(-2),
            EndDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC6);

        // Active fostering for animal 8
        var fosteringC8 = new Fostering
        {
            Id = "c9a0b1c2-d3e4-4f5a-6b7c-8d9e0f1a2b3c",
            AnimalId = animalC8.Id,
            UserId = fosterUserC.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
        dbContext.Fosterings.Add(fosteringC8);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 6. CREATE ACTIVITIES
        // ============================================

        // Activity 1: Valid - future, active, fostering
        var activity1 = new Activity
        {
            Id = "d0b1c2d3-e4f5-4a6b-7c8d-9e0f1a2b3c4d",
            AnimalId = animalC1.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity1);

        // Activity 2: Other user's activity
        var activity2 = new Activity
        {
            Id = "e1c2d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e",
            AnimalId = animalC2.Id,
            UserId = otherUser.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(14),
            EndDate = twoDaysFromNow.AddHours(16),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity2);

        // Activity 3: Already cancelled
        var activity3 = new Activity
        {
            Id = "f2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
            AnimalId = animalC3.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Cancelled,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity3);

        // Activity 4: Already completed
        var activity4 = new Activity
        {
            Id = "a3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
            AnimalId = animalC4.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Completed,
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(-2).AddHours(2),
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };
        dbContext.Activities.Add(activity4);

        // Activity 5: Past (already started)
        var activity5 = new Activity
        {
            Id = "b4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
            AnimalId = animalC5.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        dbContext.Activities.Add(activity5);

        // Activity 6: With cancelled fostering
        var activity6 = new Activity
        {
            Id = "c5a6b7c8-d9e0-4f1a-2b3c-4d5e6f7a8b9c",
            AnimalId = animalC6.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity6);

        // Activity 7: Ownership type (not fostering)
        var activity7 = new Activity
        {
            Id = "d6b7c8d9-e0f1-4a2b-3c4d-5e6f7a8b9c0d",
            AnimalId = animalC7.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow,
            EndDate = twoDaysFromNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity7);

        // Activity 8: With available slot
        var activity8 = new Activity
        {
            Id = "e7c8d9e0-f1a2-4b3c-4d5e-6f7a8b9c0d1e",
            AnimalId = animalC8.Id,
            UserId = fosterUserC.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = twoDaysFromNow.AddHours(10),
            EndDate = twoDaysFromNow.AddHours(12),
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Activities.Add(activity8);

        await dbContext.SaveChangesAsync();

        // ============================================
        // 7. CREATE ACTIVITY SLOTS
        // ============================================

        var slot1 = new ActivitySlot
        {
            Id = "f8d9e0f1-a2b3-4c4d-5e6f-7a8b9c0d1e2f",
            ActivityId = activity1.Id,
            StartDateTime = twoDaysFromNow.AddHours(10),
            EndDateTime = twoDaysFromNow.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot1);

        var slot2 = new ActivitySlot
        {
            Id = "a9e0f1a2-b3c4-4d5e-6f7a-8b9c0d1e2f3a",
            ActivityId = activity2.Id,
            StartDateTime = twoDaysFromNow.AddHours(14),
            EndDateTime = twoDaysFromNow.AddHours(16),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot2);

        var slot3 = new ActivitySlot
        {
            Id = "b0f1a2b3-c4d5-4e6f-7a8b-9c0d1e2f3a4b",
            ActivityId = activity3.Id,
            StartDateTime = twoDaysFromNow.AddHours(10),
            EndDateTime = twoDaysFromNow.AddHours(12),
            Status = SlotStatus.Available,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot3);

        var slot4 = new ActivitySlot
        {
            Id = "c1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c",
            ActivityId = activity4.Id,
            StartDateTime = DateTime.UtcNow.AddDays(-2),
            EndDateTime = DateTime.UtcNow.AddDays(-2).AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };
        dbContext.ActivitySlots.Add(slot4);

        var slot5 = new ActivitySlot
        {
            Id = "d2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d",
            ActivityId = activity5.Id,
            StartDateTime = DateTime.UtcNow.AddHours(-1),
            EndDateTime = DateTime.UtcNow.AddHours(1),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        dbContext.ActivitySlots.Add(slot5);

        var slot6 = new ActivitySlot
        {
            Id = "e3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
            ActivityId = activity6.Id,
            StartDateTime = twoDaysFromNow.AddHours(10),
            EndDateTime = twoDaysFromNow.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot6);

        var slot7 = new ActivitySlot
        {
            Id = "f4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f",
            ActivityId = activity7.Id,
            StartDateTime = twoDaysFromNow,
            EndDateTime = twoDaysFromNow.AddMonths(1),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot7);

        var slot8 = new ActivitySlot
        {
            Id = "a5e6f7a8-b9c0-4d1e-2f3a-4b5c6d7e8f9a",
            ActivityId = activity8.Id,
            StartDateTime = twoDaysFromNow.AddHours(10),
            EndDateTime = twoDaysFromNow.AddHours(12),
            Status = SlotStatus.Available, // Already available (to test canceling available slot)
            Type = SlotType.Activity,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.ActivitySlots.Add(slot8);

        await dbContext.SaveChangesAsync();


        // =====================================================
        // SEED DATA FOR GET FOSTERING ACTIVITIES BY USER
        // =====================================================

        // Check if test data already exists
        if (await dbContext.Users.AnyAsync(u => u.Email == "testuser@example.com"))
        {
            return; // Data already seeded
        }

        var now = DateTime.UtcNow;

        // =====================================================
        // 1. TEST USER
        // =====================================================
        var testUser = new User
        {
            Id = "test-user-id-001",
            UserName = "testuser@example.com",
            Email = "testuser@example.com",
            EmailConfirmed = true,
            PhoneNumber = "912345678",
            PhoneNumberConfirmed = true,
            Name = "Test User For Fostering",
            BirthDate = new DateTime(1990, 1, 15),
            Street = "Rua de Teste 123",
            City = "Porto",
            PostalCode = "4000-123",
            CreatedAt = now,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        await userManager.CreateAsync(testUser, "Test@123");
        await userManager.AddToRoleAsync(testUser, "User");
        

        // =====================================================
        // 2. TEST SHELTER
        // =====================================================
        var testShelterGF = new Shelter
        {
            Id = "test-shelter-id-001",
            Name = "Test Animal Shelter",
            Street = "Avenida dos Animais 456",
            City = "Lisboa",
            PostalCode = "1000-456",
            Phone = "213456789",
            NIF = "500123456",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0),
            CreatedAt = now
        };

        await dbContext.Shelters.AddAsync(testShelterGF);

        // Add shelter image (principal)
        var shelterImage = new Image
        {
            Id = "shelter-image-001",
            PublicId = "shelter_test_001",
            IsPrincipal = true,
            ShelterId = testShelterGF.Id,
            Url = "https://res.cloudinary.com/test/shelter_test_001.jpg",
            Description = "Test Shelter Main Image",
            CreatedAt = now
        };

        await dbContext.Images.AddAsync(shelterImage);

        // =====================================================
        // 3. TEST BREEDS
        // =====================================================
        var breedsGF = new List<Breed>
        {
            new Breed
            {
                Id = "breed-bull1-001",
                Name = "Bulldog Francês",
                Description = "Friendly and outgoing breed",
                CreatedAt = now
            },
            new Breed
            {
                Id = "breed-bull2-001",
                Name = "Bulldog Inglês",
                Description = "Intelligent and friendly breed",
                CreatedAt = now
            },
            new Breed
            {
                Id = "breed-pincher-001",
                Name = "Pincher",
                Description = "Small to medium-sized breed",
                CreatedAt = now
            }
        };

        await dbContext.Breeds.AddRangeAsync(breedsGF);

        // =====================================================
        // 4. TEST ANIMALS
        // =====================================================
        var animalsGF = new List<Animal>
        {
            // Animal 1: Max
            new Animal
            {
                Id = "animal-max-001",
                Name = "Max",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Friendly and energetic Labrador",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Golden",
                BirthDate = DateOnly.FromDateTime(now.AddYears(-4)),
                Sterilized = true,
                Cost = 50.00m,
                Features = "Loves to play fetch, good with children",
                CreatedAt = now,
                ShelterId = testShelterGF.Id,
                BreedId = "breed-bull1-001"
            },
            // Animal 2: Luna
            new Animal
            {
                Id = "animal-luna-001",
                Name = "Luna",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Sweet and calm Golden Retriever",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Female,
                Colour = "Cream",
                BirthDate = DateOnly.FromDateTime(now.AddYears(-5)),
                Sterilized = true,
                Cost = 60.00m,
                Features = "Gentle, great with other pets",
                CreatedAt = now,
                ShelterId = testShelterGF.Id,
                BreedId = "breed-bull2-001"
            },
            // Animal 3: Charlie
            new Animal
            {
                Id = "animal-charlie-001",
                Name = "Charlie",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Playful and curious Beagle",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Tricolor",
                BirthDate = DateOnly.FromDateTime(now.AddYears(-3)),
                Sterilized = false,
                Cost = 40.00m,
                Features = "Very active, loves treats",
                CreatedAt = now,
                ShelterId = testShelterGF.Id,
                BreedId = "breed-pincher-001"
            },
            // Animal 4: Bella
            new Animal
            {
                Id = "animal-bella-001",
                Name = "Bella",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Affectionate Labrador",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Female,
                Colour = "Black",
                BirthDate = DateOnly.FromDateTime(now.AddYears(-6)),
                Sterilized = true,
                Cost = 55.00m,
                Features = "Calm temperament, loves cuddles",
                CreatedAt = now,
                ShelterId = testShelterGF.Id,
                BreedId = "breed-bull1-001"
            },
            // Animal 5: Rocky
            new Animal
            {
                Id = "animal-rocky-001",
                Name = "Rocky",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Strong and loyal Golden Retriever",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Red",
                BirthDate = DateOnly.FromDateTime(now.AddYears(-7)),
                Sterilized = true,
                Cost = 65.00m,
                Features = "Well-trained, obedient",
                CreatedAt = now,
                ShelterId = testShelterGF.Id,
                BreedId = "breed-bull2-001"
            }
        };

        await dbContext.Animals.AddRangeAsync(animalsGF);

        // =====================================================
        // 5. ANIMAL IMAGES (Principal)
        // =====================================================
        var animalImagesGF = new List<Image>
        {
            new Image
            {
                Id = "image-max-001",
                PublicId = "animal_max_001",
                IsPrincipal = true,
                AnimalId = "animal-max-001",
                Url = "https://res.cloudinary.com/test/animal_max_001.jpg",
                Description = "Max Main Photo",
                CreatedAt = now
            },
            new Image
            {
                Id = "image-luna-001",
                PublicId = "animal_luna_001",
                IsPrincipal = true,
                AnimalId = "animal-luna-001",
                Url = "https://res.cloudinary.com/test/animal_luna_001.jpg",
                Description = "Luna Main Photo",
                CreatedAt = now
            },
            new Image
            {
                Id = "image-charlie-001",
                PublicId = "animal_charlie_001",
                IsPrincipal = true,
                AnimalId = "animal-charlie-001",
                Url = "https://res.cloudinary.com/test/animal_charlie_001.jpg",
                Description = "Charlie Main Photo",
                CreatedAt = now
            },
            new Image
            {
                Id = "image-bella-001",
                PublicId = "animal_bella_001",
                IsPrincipal = true,
                AnimalId = "animal-bella-001",
                Url = "https://res.cloudinary.com/test/animal_bella_001.jpg",
                Description = "Bella Main Photo",
                CreatedAt = now
            },
            new Image
            {
                Id = "image-rocky-001",
                PublicId = "animal_rocky_001",
                IsPrincipal = true,
                AnimalId = "animal-rocky-001",
                Url = "https://res.cloudinary.com/test/animal_rocky_001.jpg",
                Description = "Rocky Main Photo",
                CreatedAt = now
            }
        };

        await dbContext.Images.AddRangeAsync(animalImagesGF);

        // =====================================================
        // 6. ACTIVE FOSTERINGS
        // =====================================================
        var fosteringsGF = new List<Fostering>
        {
            new Fostering
            {
                Id = "fostering-max-001",
                AnimalId = "animal-max-001",
                UserId = testUser.Id,
                Amount = 25.00m,
                Status = FosteringStatus.Active,
                StartDate = now.AddDays(-30)
            },
            new Fostering
            {
                Id = "fostering-luna-001",
                AnimalId = "animal-luna-001",
                UserId = testUser.Id,
                Amount = 30.00m,
                Status = FosteringStatus.Active,
                StartDate = now.AddDays(-60)
            },
            new Fostering
            {
                Id = "fostering-charlie-001",
                AnimalId = "animal-charlie-001",
                UserId = testUser.Id,
                Amount = 20.00m,
                Status = FosteringStatus.Active,
                StartDate = now.AddDays(-45)
            },
            new Fostering
            {
                Id = "fostering-bella-001",
                AnimalId = "animal-bella-001",
                UserId = testUser.Id,
                Amount = 27.50m,
                Status = FosteringStatus.Active,
                StartDate = now.AddDays(-90)
            },
            new Fostering
            {
                Id = "fostering-rocky-001",
                AnimalId = "animal-rocky-001",
                UserId = testUser.Id,
                Amount = 32.50m,
                Status = FosteringStatus.Active,
                StartDate = now.AddDays(-15)
            }
        };

        await dbContext.Fosterings.AddRangeAsync(fosteringsGF);

        // =====================================================
        // 7. ACTIVE FOSTERING ACTIVITIES
        // =====================================================
        var activitiesGF = new List<Activity>
        {
            new Activity
            {
                Id = "activity-max-001",
                AnimalId = "animal-max-001",
                UserId = testUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(335),
                CreatedAt = now.AddDays(-35)
            },
            new Activity
            {
                Id = "activity-luna-001",
                AnimalId = "animal-luna-001",
                UserId = testUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-60),
                EndDate = now.AddDays(305),
                CreatedAt = now.AddDays(-65)
            },
            new Activity
            {
                Id = "activity-charlie-001",
                AnimalId = "animal-charlie-001",
                UserId = testUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-45),
                EndDate = now.AddDays(320),
                CreatedAt = now.AddDays(-50)
            },
            new Activity
            {
                Id = "activity-bella-001",
                AnimalId = "animal-bella-001",
                UserId = testUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-90),
                EndDate = now.AddDays(275),
                CreatedAt = now.AddDays(-95)
            },
            new Activity
            {
                Id = "activity-rocky-001",
                AnimalId = "animal-rocky-001",
                UserId = testUser.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-15),
                EndDate = now.AddDays(350),
                CreatedAt = now.AddDays(-20)
            }
        };

        await dbContext.Activities.AddRangeAsync(activitiesGF);

        // Save before adding slots (need activity IDs)
        await dbContext.SaveChangesAsync();

        // =====================================================
        // 8. FUTURE ACTIVITY SLOTS (15 total - 3 per animal)
        // =====================================================
        var activitySlotsGF = new List<ActivitySlot>
        {
            // Slots for Max (3 visits)
            new ActivitySlot
            {
                Id = "slot-max-001",
                ActivityId = "activity-max-001",
                StartDateTime = now.AddDays(2).Date.AddHours(10),
                EndDateTime = now.AddDays(2).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-max-002",
                ActivityId = "activity-max-001",
                StartDateTime = now.AddDays(9).Date.AddHours(14),
                EndDateTime = now.AddDays(9).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-max-003",
                ActivityId = "activity-max-001",
                StartDateTime = now.AddDays(16).Date.AddHours(10),
                EndDateTime = now.AddDays(16).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },

            // Slots for Luna (3 visits)
            new ActivitySlot
            {
                Id = "slot-luna-001",
                ActivityId = "activity-luna-001",
                StartDateTime = now.AddDays(3).Date.AddHours(15),
                EndDateTime = now.AddDays(3).Date.AddHours(17),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-luna-002",
                ActivityId = "activity-luna-001",
                StartDateTime = now.AddDays(10).Date.AddHours(11),
                EndDateTime = now.AddDays(10).Date.AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-luna-003",
                ActivityId = "activity-luna-001",
                StartDateTime = now.AddDays(17).Date.AddHours(15),
                EndDateTime = now.AddDays(17).Date.AddHours(17),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },

            // Slots for Charlie (3 visits)
            new ActivitySlot
            {
                Id = "slot-charlie-001",
                ActivityId = "activity-charlie-001",
                StartDateTime = now.AddDays(4).Date.AddHours(9),
                EndDateTime = now.AddDays(4).Date.AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-charlie-002",
                ActivityId = "activity-charlie-001",
                StartDateTime = now.AddDays(11).Date.AddHours(13),
                EndDateTime = now.AddDays(11).Date.AddHours(15),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-charlie-003",
                ActivityId = "activity-charlie-001",
                StartDateTime = now.AddDays(18).Date.AddHours(9),
                EndDateTime = now.AddDays(18).Date.AddHours(11),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },

            // Slots for Bella (3 visits)
            new ActivitySlot
            {
                Id = "slot-bella-001",
                ActivityId = "activity-bella-001",
                StartDateTime = now.AddDays(5).Date.AddHours(16),
                EndDateTime = now.AddDays(5).Date.AddHours(18),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-bella-002",
                ActivityId = "activity-bella-001",
                StartDateTime = now.AddDays(12).Date.AddHours(10),
                EndDateTime = now.AddDays(12).Date.AddHours(12),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-bella-003",
                ActivityId = "activity-bella-001",
                StartDateTime = now.AddDays(19).Date.AddHours(16),
                EndDateTime = now.AddDays(19).Date.AddHours(18),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },

            // Slots for Rocky (3 visits)
            new ActivitySlot
            {
                Id = "slot-rocky-001",
                ActivityId = "activity-rocky-001",
                StartDateTime = now.AddDays(6).Date.AddHours(14),
                EndDateTime = now.AddDays(6).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-rocky-002",
                ActivityId = "activity-rocky-001",
                StartDateTime = now.AddDays(13).Date.AddHours(11),
                EndDateTime = now.AddDays(13).Date.AddHours(13),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            },
            new ActivitySlot
            {
                Id = "slot-rocky-003",
                ActivityId = "activity-rocky-001",
                StartDateTime = now.AddDays(20).Date.AddHours(14),
                EndDateTime = now.AddDays(20).Date.AddHours(16),
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity,
                CreatedAt = now
            }
        };

        await dbContext.Set<ActivitySlot>().AddRangeAsync(activitySlotsGF);

        // =====================================================
        // 9. EDGE CASE DATA (should NOT be returned)
        // =====================================================

        // Past activity slot (should NOT be returned)
        var pastSlot = new ActivitySlot
        {
            Id = "slot-max-past-001",
            ActivityId = "activity-max-001",
            StartDateTime = now.AddDays(-5).Date.AddHours(10),
            EndDateTime = now.AddDays(-5).Date.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = now
        };
        await dbContext.Set<ActivitySlot>().AddAsync(pastSlot);

        // Cancelled activity with future slot (should NOT be returned)
        var cancelledActivity = new Activity
        {
            Id = "activity-cancelled-001",
            AnimalId = "animal-max-001",
            UserId = testUser.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Cancelled,
            StartDate = now.AddDays(-10),
            EndDate = now.AddDays(355),
            CreatedAt = now.AddDays(-15)
        };
        await dbContext.Activities.AddAsync(cancelledActivity);
        await dbContext.SaveChangesAsync();

        var cancelledSlot = new ActivitySlot
        {
            Id = "slot-cancelled-001",
            ActivityId = "activity-cancelled-001",
            StartDateTime = now.AddDays(7).Date.AddHours(10),
            EndDateTime = now.AddDays(7).Date.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity,
            CreatedAt = now
        };
        await dbContext.Set<ActivitySlot>().AddAsync(cancelledSlot);

        // Available slot (not reserved, should NOT be returned)
        var availableSlot = new ActivitySlot
        {
            Id = "slot-max-available-001",
            ActivityId = "activity-max-001",
            StartDateTime = now.AddDays(8).Date.AddHours(10),
            EndDateTime = now.AddDays(8).Date.AddHours(12),
            Status = SlotStatus.Available,
            Type = SlotType.Activity,
            CreatedAt = now
        };
        await dbContext.Set<ActivitySlot>().AddAsync(availableSlot);

        // =====================================================
        // SAVE ALL CHANGES
        // =====================================================
        await dbContext.SaveChangesAsync();
    }


}