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
        const string platformAdmin = "PlatformAdmin";
        const string adminCaa = "AdminCAA";
        const string userRole = "User";
        const string user1Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"; // Alice
        const string user2Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"; // Bob
        const string user3Id = "cccccccc-cccc-cccc-cccc-cccccccccccc"; // Carlos
        const string user4Id = "dddddddd-dddd-dddd-dddd-dddddddddddd"; // Diana
        const string user5Id = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"; // Eduardo
        const string user6Id = "66666666-6666-6666-6666-666666666666"; // Filipe
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
                new ()
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

        // ======== SEED FAVORITES ========
        if (!dbContext.Favorites.Any())
        {
            var favorites = new List<Favorite>
            {
                new()
                {
                    Id = favorite1Id,
                    UserId = user7Id,  // Gustavo
                    AnimalId = animal12Id,  // Luna
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Id = favorite2Id,
                    UserId = user7Id,  // Gustavo
                    AnimalId = animal13Id,  // Rex
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = favorite3Id,
                    UserId = user7Id,  // Gustavo
                    AnimalId = animal14Id,  // Simba
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await dbContext.Favorites.AddRangeAsync(favorites);
            await dbContext.SaveChangesAsync();
        }
    }
}