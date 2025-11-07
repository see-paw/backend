using Domain;

namespace Persistence.Seeds;

/// <summary>
/// Seeds images into the database.
/// </summary>
internal static class ImageSeeder
{
    /// <summary>
    /// Seeds all images into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Images.Any())
        {
            var images = new List<Image>();
            
            images.AddRange(GetMainShelterImages());
            images.AddRange(GetMainAnimalImages());
            images.AddRange(GetFavoriteAnimalImages());
            images.AddRange(GetOwnershipRequestShelterImages());
            images.AddRange(GetOwnershipRequestAnimalImages());
            
            await dbContext.Images.AddRangeAsync(images);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Image> GetMainShelterImages()
    {
        return new List<Image>
        {
            new()
            {
                Id = SeedConstants.ImageShelter1Img1Id,
                ShelterId = SeedConstants.Shelter1Id,
                Url = SeedConstants.ImageShelterUrl1_1,
                Description = "Fachada principal do CAA Porto",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdShelter1Img1
            },
            new()
            {
                Id = SeedConstants.ImageShelter1Img2Id,
                ShelterId = SeedConstants.Shelter1Id,
                Url = SeedConstants.ImageShelterUrl1_2,
                Description = "Área exterior do abrigo",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdShelter1Img2
            },
            new()
            {
                Id = SeedConstants.ImageShelter2Img1Id,
                ShelterId = SeedConstants.Shelter2Id,
                Url = SeedConstants.ImageShelterUrl2_1,
                Description = "Entrada principal do CAA de Cima",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdShelter2Img1
            },
            new()
            {
                Id = SeedConstants.ImageShelter2Img2Id,
                ShelterId = SeedConstants.Shelter2Id,
                Url = SeedConstants.ImageShelterUrl2_2,
                Description = "Zona de descanso dos animais",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdShelter2Img2
            }
        };
    }

    private static List<Image> GetMainAnimalImages()
    {
        return new List<Image>
        {
            new()
            {
                Id = SeedConstants.ImageAnimal1Img1Id,
                AnimalId = SeedConstants.Animal1Id,
                Url = SeedConstants.ImageUrl1_1,
                Description = "Bolinhas deitado ao sol",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal1Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal1Img2Id,
                AnimalId = SeedConstants.Animal1Id,
                Url = SeedConstants.ImageUrl1_2,
                Description = "Bolinhas a brincar com bola",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal1Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal2Img1Id,
                AnimalId = SeedConstants.Animal2Id,
                Url = SeedConstants.ImageUrl2_1,
                Description = "Luna a correr no jardim",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal2Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal2Img2Id,
                AnimalId = SeedConstants.Animal2Id,
                Url = SeedConstants.ImageUrl2_2,
                Description = "Luna a dormir tranquilamente",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal2Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal3Img1Id,
                AnimalId = SeedConstants.Animal3Id,
                Url = SeedConstants.ImageUrl3_1,
                Description = "Tico no poleiro",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal3Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal3Img2Id,
                AnimalId = SeedConstants.Animal3Id,
                Url = SeedConstants.ImageUrl3_2,
                Description = "Tico a abrir as asas",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal3Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal4Img1Id,
                AnimalId = SeedConstants.Animal4Id,
                Url = SeedConstants.ImageUrl4_1,
                Description = "Mika deitada no sofá",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal4Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal4Img2Id,
                AnimalId = SeedConstants.Animal4Id,
                Url = SeedConstants.ImageUrl4_2,
                Description = "Mika a brincar com uma corda",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal4Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal5Img1Id,
                AnimalId = SeedConstants.Animal5Id,
                Url = SeedConstants.ImageUrl5_1,
                Description = "Thor atento ao portão",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal5Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal5Img2Id,
                AnimalId = SeedConstants.Animal5Id,
                Url = SeedConstants.ImageUrl5_2,
                Description = "Thor a correr no pátio",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal5Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal6Img1Id,
                AnimalId = SeedConstants.Animal6Id,
                Url = SeedConstants.ImageUrl6_1,
                Description = "Nina a comer cenoura",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal6Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal6Img2Id,
                AnimalId = SeedConstants.Animal6Id,
                Url = SeedConstants.ImageUrl6_2,
                Description = "Nina a explorar o jardim",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal6Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal7Img1Id,
                AnimalId = SeedConstants.Animal7Id,
                Url = SeedConstants.ImageUrl7_1,
                Description = "Rockito a observar o horizonte",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal7Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal7Img2Id,
                AnimalId = SeedConstants.Animal7Id,
                Url = SeedConstants.ImageUrl7_2,
                Description = "Rockito a brincar no campo",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal7Img2
            }
        };
    }

    private static List<Image> GetFavoriteAnimalImages()
    {
        return new List<Image>
        {
            new()
            {
                Id = SeedConstants.ImageAnimal12Img1Id,
                AnimalId = SeedConstants.Animal12Id,
                Url = SeedConstants.ImageUrl1_1,
                Description = "Luna a descansar",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal1Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal12Img2Id,
                AnimalId = SeedConstants.Animal12Id,
                Url = SeedConstants.ImageUrl1_2,
                Description = "Luna a brincar",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal1Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal13Img1Id,
                AnimalId = SeedConstants.Animal13Id,
                Url = SeedConstants.ImageUrl1_1,
                Description = "Rex no jardim",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal1Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal13Img2Id,
                AnimalId = SeedConstants.Animal13Id,
                Url = SeedConstants.ImageUrl1_2,
                Description = "Rex a correr",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal1Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal14Img1Id,
                AnimalId = SeedConstants.Animal14Id,
                Url = SeedConstants.ImageUrl1_1,
                Description = "Simba a explorar",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal1Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal14Img2Id,
                AnimalId = SeedConstants.Animal14Id,
                Url = SeedConstants.ImageUrl1_2,
                Description = "Simba a dormir",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal1Img2
            },
            new()
            {
                Id = SeedConstants.ImageAnimal15Img1Id,
                AnimalId = SeedConstants.Animal15Id,
                Url = SeedConstants.ImageUrl1_1,
                Description = "NotifTestDog principal",
                IsPrincipal = true,
                PublicId = SeedConstants.PublicIdAnimal1Img1
            },
            new()
            {
                Id = SeedConstants.ImageAnimal15Img2Id,
                AnimalId = SeedConstants.Animal15Id,
                Url = SeedConstants.ImageUrl1_2,
                Description = "NotifTestDog secundário",
                IsPrincipal = false,
                PublicId = SeedConstants.PublicIdAnimal1Img2
            }
        };
    }

    private static List<Image> GetOwnershipRequestShelterImages()
    {
        return new List<Image>
        {
            new()
            {
                Id = SeedConstants.OwnershipShelterImg1Id,
                PublicId = "shelters/shelter1_main",
                Url = "https://example.com/shelter1.jpg",
                Description = "Foto principal do abrigo",
                IsPrincipal = true,
                ShelterId = SeedConstants.OwnershipShelter1Id
            },
            new()
            {
                Id = SeedConstants.OwnershipShelterImg2Id,
                PublicId = "shelters/shelter2_main",
                Url = "https://example.com/shelter2.jpg",
                Description = "Foto principal do abrigo",
                IsPrincipal = true,
                ShelterId = SeedConstants.OwnershipShelter2Id
            }
        };
    }

    private static List<Image> GetOwnershipRequestAnimalImages()
    {
        return new List<Image>
        {
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg1Id,
                PublicId = "animals/rex_1",
                Url = "https://example.com/rex1.jpg",
                Description = "Rex brincando",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal1Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg2Id,
                PublicId = "animals/rex_2",
                Url = "https://example.com/rex2.jpg",
                Description = "Rex descansando",
                IsPrincipal = false,
                AnimalId = SeedConstants.OwnershipAnimal1Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg3Id,
                PublicId = "animals/bella_1",
                Url = "https://example.com/bella1.jpg",
                Description = "Bella sentada",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal2Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg4Id,
                PublicId = "animals/thor_1",
                Url = "https://example.com/thor1.jpg",
                Description = "Thor em guarda",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal3Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg5Id,
                PublicId = "animals/max_1",
                Url = "https://example.com/max1.jpg",
                Description = "Max feliz",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal4Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg6Id,
                PublicId = "animals/luna_1",
                Url = "https://example.com/luna1.jpg",
                Description = "Luna adorável",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal5Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg7Id,
                PublicId = "animals/bobby_1",
                Url = "https://example.com/bobby1.jpg",
                Description = "Bobby sorrindo",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal6Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg8Id,
                PublicId = "animals/simba_1",
                Url = "https://example.com/simba1.jpg",
                Description = "Simba brincalhão",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal7Id
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimalImg9Id,
                PublicId = "animals/nina_1",
                Url = "https://example.com/nina1.jpg",
                Description = "Nina tranquila",
                IsPrincipal = true,
                AnimalId = SeedConstants.OwnershipAnimal8Id
            }
        };
    }
}