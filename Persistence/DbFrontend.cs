using Domain;
using Domain.Enums;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Persistence;

public class DbFrontend
{
    public static async Task SeedData(AppDbContext dbContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory,
        bool resetDatabase = false)
    {

        if (!dbContext.Shelters.Any())
        {
            var shelters = new List<Shelter>
            {
                new()
                {
                    Id = "8f3c2e7b-9c45-4b0b-a21d-6f4b72a1d812",
                    Name = "Rua Sesamo",
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
                    Id = "c1e8a540-2f7d-47e4-b8bb-9a6c13f5e2fd",
                    Name = "Patudos Felizes",
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

        if (!dbContext.Breeds.Any())
        {
            var breeds = new List<Breed>
            {
                new()
                {
                    Id = "f2a7c4d1-8b39-4c9d-9e61-73b84c2f5a21", Name = "Siamês",
                    Description = "Raça de gato elegante e sociável."
                },
                new()
                {
                    Id = "0c5e7b92-3df4-4c0b-8f1b-9da31e8a7c44", Name = "Beagle",
                    Description = "Cão amigável, curioso e ativo."
                },
                new()
                {
                    Id = "a94b12ec-6f0a-4f8d-9acd-1e72c9fb3349", Name = "Rafeiro",
                    Description = "Cão leal, inteligente e protetor."
                }
            };

            await dbContext.Breeds.AddRangeAsync(breeds);
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.Animals.Any())
        {
            var animals = new List<Animal>
            {
                new()
                {
                    Id = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Name = "Maria",
                    AnimalState = AnimalState.Available,
                    Description =
                        "Esta cadelinha é basicamente uma bolinha de alegria com quatro patas. Super fofa, sempre pronta para brincar, e especialista em transformar dias normais em episódios de comédia romântica — ela faz a comédia, tu tratas da parte romântica com os snacks.\nAdora correr atrás de brinquedos (e ocasionalmente do próprio rabo, porque prioridades), mas também sabe tirar aquelas sestas dignas de um monge Zen. O lema dela é simples: brincar muito, dormir melhor e pedir mimo sempre que possível.",
                    Species = Species.Dog,
                    Size = SizeType.Small,
                    Sex = SexType.Female,
                    Colour = "Bege",
                    BirthDate = new DateOnly(2022, 4, 15),
                    Sterilized = true,
                    BreedId = "a94b12ec-6f0a-4f8d-9acd-1e72c9fb3349",
                    Cost = 30,
                    Features = "Puppy eyes, muito sociável",
                    ShelterId = "8f3c2e7b-9c45-4b0b-a21d-6f4b72a1d812",
                    Images = new List<Image>()
                },
                new()
                {
                    Id = "e4c18f03-7a5d-4bcf-8e8d-2f9a6f3140b1",
                    Name = "Leandro",
                    AnimalState = AnimalState.PartiallyFostered,
                    Description =
                        "Este gato é o verdadeiro mestre da traquinice — um especialista em planos secretos, acrobacias inesperadas e pequenos delitos felinos como roubar meias, derrubar objetos e fingir que não tem nada a ver com o assunto.",
                    Species = Species.Cat,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Castanho claro",
                    BirthDate = new DateOnly(2021, 11, 5),
                    Sterilized = true,
                    BreedId = "f2a7c4d1-8b39-4c9d-9e61-73b84c2f5a21",
                    Cost = 50,
                    Features = "Muito obediente e adora correr",
                    ShelterId = "8f3c2e7b-9c45-4b0b-a21d-6f4b72a1d812",
                    Images = new List<Image>()
                },
                new()
                {
                    Id = "b7f2d6ea-1c34-49ce-96f8-7a12c4e509c7",
                    Name = "Jose",
                    AnimalState = AnimalState.Available,
                    Description =
                        "Este cão tem oficialmente a expressão mais adoravelmente tonta que já existiu. Parece que está sempre meio confuso com a vida — do género “o quê? já é hora de comer outra vez?” — mas a verdade é que isso só o torna ainda mais irresistível.",
                    Species = Species.Dog,
                    Size = SizeType.Small,
                    Sex = SexType.Male,
                    Colour = "Preto",
                    BirthDate = new DateOnly(2025, 2, 10),
                    Sterilized = false,
                    BreedId = "0c5e7b92-3df4-4c0b-8f1b-9da31e8a7c44",
                    Cost = 80,
                    Features = "Sabe rebolar",
                    ShelterId = "c1e8a540-2f7d-47e4-b8bb-9a6c13f5e2fd",
                    Images = new List<Image>()
                },
                new()
                {
                    Id = "3fa91eb5-5edb-4ac3-9ea0-a2c94318e6d0",
                    Name = "Jéssica",
                    AnimalState = AnimalState.Available,
                    Description =
                        "Esta gata é o equivalente felino a um cobertor quentinho num dia frio. Pequena, elegante e com aquele olhar suave que diz “eu deixo-te fazer festas… mas só porque hoje estou de bom humor.",
                    Species = Species.Cat,
                    Size = SizeType.Small,
                    Sex = SexType.Female,
                    Colour = "Malhada",
                    BirthDate = new DateOnly(2025, 8, 22),
                    Sterilized = true,
                    BreedId = "f2a7c4d1-8b39-4c9d-9e61-73b84c2f5a21",
                    Cost = 25,
                    Features = "Olhos azuis intensos",
                    ShelterId = "c1e8a540-2f7d-47e4-b8bb-9a6c13f5e2fd",
                    Images = new List<Image>()
                }
            };
            await dbContext.Animals.AddRangeAsync(animals);
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.Images.Any())
        {
            var images = new List<Image>
            {
                // === Maria- 5 fotos ===
                new()
                {
                    Id = "4f9a2c1b-7e3d-45c8-9f11-5d83e2a4b9cc",
                    AnimalId = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763578598/IMG_0594_rifgqj.jpg",
                    Description = "Maria sendo Maria",
                    IsPrincipal = true,
                    PublicId = "IMG_0594_rifgqj"
                },
                new()
                {
                    Id = "c8e27f04-91a6-4dce-a8ab-6a3b0f51d2e9",
                    AnimalId = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763578594/IMG_2422_s4mbwt.jpg",
                    Description = "Maria sendo fofa",
                    IsPrincipal = false,
                    PublicId = "IMG_2422_s4mbwt"
                },
                new()
                {
                    Id = "b1f74e6d-0b3a-4f42-ae12-49c7c0f97f55",
                    AnimalId = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763578591/IMG_3661_ujgaa1.jpg",
                    Description = "Maria sendo maravilhosa",
                    IsPrincipal = false,
                    PublicId = "IMG_3661_ujgaa1"
                },
                new()
                {
                    Id = "e3d2a9c7-6f01-43f0-8572-0ad3c1e60782",
                    AnimalId = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763578589/IMG_3957_lysorq.jpg",
                    Description = "Maria sendo fantástica",
                    IsPrincipal = false,
                    PublicId = "IMG_3957_lysorq"
                },
                new()
                {
                    Id = "a7c5d3b2-2ac4-4d5b-8f8c-37e9f61e2771",
                    AnimalId = "9b3a6c41-2d85-4c7b-9051-fc1e2ab78d44",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763578587/IMG_4900_x2rqrq.jpg",
                    Description = "Maria sendo um piglet",
                    IsPrincipal = false,
                    PublicId = "IMG_4900_x2rqrq"
                },

                // === Leandro- 3 fotos ===
                new()
                {
                    Id = "d3a19c7e-4b52-49e2-9f8f-8a62d91c4f33",
                    AnimalId = "e4c18f03-7a5d-4bcf-8e8d-2f9a6f3140b1",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763306364/Unknown-2_d9amcf.jpg",
                    Description = "Leandro sendo Leandro",
                    IsPrincipal = true,
                    PublicId = "Unknown-2_d9amcf"
                },
                new()
                {
                    Id = "7f2b48e1-9c64-4bfe-bf26-1e5d7390c88b",
                    AnimalId = "e4c18f03-7a5d-4bcf-8e8d-2f9a6f3140b1",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763306364/Unknown-3_uiip6e.jpg",
                    Description = "Leandro sendo fofo",
                    IsPrincipal = false,
                    PublicId = "Unknown-3_uiip6e"
                },
                new()
                {
                    Id = "acf0e6d2-b31e-4b6c-815e-e0a5d74fae91",
                    AnimalId = "e4c18f03-7a5d-4bcf-8e8d-2f9a6f3140b1",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763306365/Unknown-4_ygwdpm.jpg",
                    Description = "Leandro sendo maravilhoso",
                    IsPrincipal = false,
                    PublicId = "Unknown-4_ygwdpm"
                },
                // === José- 2 fotos ===
                new()
                {
                    Id = "5e9d13bc-4f2a-4c0d-9c72-1a8b4c52f067",
                    AnimalId = "b7f2d6ea-1c34-49ce-96f8-7a12c4e509c7",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763306794/Unknown-5_blrqwy.jpg",
                    Description = "José sendo José",
                    IsPrincipal = true,
                    PublicId = "Unknown-5_blrqwy"
                },
                new()
                {
                    Id = "c47f0a21-96c4-4efc-8d25-3b7e92f6c8e4",
                    AnimalId = "b7f2d6ea-1c34-49ce-96f8-7a12c4e509c7",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763306795/Unknown-6_mstvsn.jpg",
                    Description = "José sendo fofo",
                    IsPrincipal = false,
                    PublicId = "Unknown-6_mstvsn"
                },
                // === Jéssica- 1 fotos ===
                new()
                {
                    Id = "8c7f1d2a-3e94-4bb2-94f0-2fa1cc7d4e51",
                    AnimalId = "3fa91eb5-5edb-4ac3-9ea0-a2c94318e6d0",
                    Url = "https://res.cloudinary.com/dnfgbodgr/image/upload/v1763307014/Unknown-7_jgfbn7.jpg",
                    Description = "Jéssica sendo Jéssica",
                    IsPrincipal = true,
                    PublicId = "Unknown-7_jgfbn7"
                }
            };
            await dbContext.Images.AddRangeAsync(images);
            await dbContext.SaveChangesAsync();
        }
    }
}
