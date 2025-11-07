using Domain;
using Domain.Enums;

namespace Persistence.Seeds;

/// <summary>
/// Seeds animals into the database.
/// </summary>
internal static class AnimalSeeder
{
    /// <summary>
    /// Seeds all animals into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        if (!dbContext.Animals.Any())
        {
            var animals = new List<Animal>();
            
            animals.AddRange(GetMainAnimals());
            animals.AddRange(GetEligibilityTestAnimals());
            animals.AddRange(GetFavoriteTestAnimals());
            animals.AddRange(GetOwnershipRequestTestAnimals());
            animals.AddRange(GetFosteringTestAnimals());
            animals.AddRange(GetCancelFosteringTestAnimals());
            
            await dbContext.Animals.AddRangeAsync(animals);
            await dbContext.SaveChangesAsync();
        }
    }

    private static List<Animal> GetMainAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.Animal1Id,
                Name = "Bolinhas",
                AnimalState = AnimalState.Available,
                Description = "Gato muito meigo e brincalhão, gosta de dormir ao sol.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Branco e cinzento",
                BirthDate = new DateOnly(2022, 4, 15),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 30,
                Features = "Olhos verdes, muito sociável",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal2Id,
                Name = "Lunica",
                AnimalState = AnimalState.Available,
                Description = "Cadela jovem e energética, ideal para famílias com crianças.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Female,
                Colour = "Castanho claro",
                BirthDate = new DateOnly(2021, 11, 5),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 50,
                Features = "Muito obediente e adora correr",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal3Id,
                Name = "Tico",
                AnimalState = AnimalState.Available,
                Description = "Papagaio falador que adora companhia humana.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Verde com azul",
                BirthDate = new DateOnly(2025, 2, 10),
                Sterilized = false,
                BreedId = SeedConstants.Breed2Id,
                Cost = 80,
                Features = "Sabe dizer 'Olá!' e assobiar",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal4Id,
                Name = "Mika",
                AnimalState = AnimalState.Available,
                Description = "Gata calma e dócil, procura um lar tranquilo.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Preto",
                BirthDate = new DateOnly(2025, 8, 22),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 25,
                Features = "Olhos azuis intensos",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal5Id,
                Name = "Thor",
                AnimalState = AnimalState.Available,
                Description = "Cão de guarda muito protetor, mas fiel à família.",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Preto e castanho",
                BirthDate = new DateOnly(2025, 6, 30),
                Sterilized = false,
                BreedId = SeedConstants.Breed2Id,
                Cost = 100,
                Features = "Muito atento e obediente",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal6Id,
                Name = "Nina",
                AnimalState = AnimalState.Available,
                Description = "Coelha curiosa e afetuosa, gosta de cenouras e de brincar.",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Branco com manchas castanhas",
                BirthDate = new DateOnly(2023, 3, 10),
                Sterilized = false,
                BreedId = SeedConstants.Breed2Id,
                Cost = 15,
                Features = "Orelhas pequenas e pelo macio",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal7Id,
                Name = "Rockito",
                AnimalState = AnimalState.Inactive,
                Description = "Cão atlético e leal, ideal para quem gosta de caminhadas.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Cinza",
                BirthDate = new DateOnly(2022, 7, 19),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 70,
                Features = "Olhos azuis e muita energia",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal8Id,
                Name = "Amora",
                AnimalState = AnimalState.HasOwner,
                Description = "Gata jovem e curiosa, adora caçar brinquedos.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Cinzento e branco",
                BirthDate = new DateOnly(2023, 5, 14),
                Sterilized = false,
                BreedId = SeedConstants.Breed2Id,
                Cost = 20,
                Features = "Bigodes longos e muito expressiva",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal9Id,
                Name = "Zeus",
                AnimalState = AnimalState.TotallyFostered,
                Description = "Cavalo calmo e bem treinado, ótimo para equitação.",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Castanho escuro",
                BirthDate = new DateOnly(2017, 9, 1),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 500,
                Features = "Crina longa e brilhante",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal10Id,
                Name = "Pipoca",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Hamster pequena e simpática, ideal para crianças.",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Dourado",
                BirthDate = new DateOnly(2024, 1, 12),
                Sterilized = false,
                BreedId = SeedConstants.Breed2Id,
                Cost = 10,
                Features = "Muito ativa e adora correr na roda",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal11Id,
                Name = "Simão",
                AnimalState = AnimalState.Available,
                Description = "Gato jovem e brincalhão",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Mesclado",
                BirthDate = new DateOnly(2023, 3, 20),
                Sterilized = true,
                BreedId = SeedConstants.Breed1Id,
                Cost = 30,
                Features = "Brincalhão, curioso e adaptável a diferentes ambientes",
                ShelterId = SeedConstants.Shelter1Id,
            }
        };
    }

    private static List<Animal> GetEligibilityTestAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.AnimalAvailableId,
                Name = "TestDog Available",
                AnimalState = AnimalState.Available,
                Description = "Animal de teste disponível para adoção",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Preto",
                BirthDate = new DateOnly(2022, 1, 15),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 40,
                Features = "Animal de teste - Estado: Available",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.AnimalWithOwnerId,
                Name = "TestDog HasOwner",
                AnimalState = AnimalState.HasOwner,
                Description = "Animal de teste que já tem dono",
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Branco",
                BirthDate = new DateOnly(2021, 5, 10),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 35,
                Features = "Animal de teste - Estado: HasOwner",
                ShelterId = SeedConstants.Shelter1Id,
                OwnerId = SeedConstants.User3Id,
                OwnershipStartDate = DateTime.UtcNow.AddMonths(-2),
            },
            new()
            {
                Id = SeedConstants.AnimalInactiveId,
                Name = "TestCat Inactive",
                AnimalState = AnimalState.Inactive,
                Description = "Animal de teste inativo",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Cinzento",
                BirthDate = new DateOnly(2020, 8, 20),
                Sterilized = true,
                BreedId = SeedConstants.Breed1Id,
                Cost = 25,
                Features = "Animal de teste - Estado: Inactive",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.AnimalPartiallyFosteredId,
                Name = "TestDog PartiallyFostered",
                AnimalState = AnimalState.PartiallyFostered,
                Description = "Animal de teste parcialmente acolhido",
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Castanho",
                BirthDate = new DateOnly(2021, 3, 5),
                Sterilized = false,
                BreedId = SeedConstants.Breed3Id,
                Cost = 60,
                Features = "Animal de teste - Estado: PartiallyFostered",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.AnimalTotallyFosteredId,
                Name = "TestCat TotallyFostered",
                AnimalState = AnimalState.TotallyFostered,
                Description = "Animal de teste totalmente acolhido",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Laranja",
                BirthDate = new DateOnly(2022, 7, 12),
                Sterilized = true,
                BreedId = SeedConstants.Breed1Id,
                Cost = 30,
                Features = "Animal de teste - Estado: TotallyFostered",
                ShelterId = SeedConstants.Shelter1Id,
            }
        };
    }

    private static List<Animal> GetFavoriteTestAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.Animal12Id,
                Name = "Luna",
                AnimalState = AnimalState.Available,
                Description = "Gata carinhosa e tranquila, ideal para apartamento.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Cinza prateado",
                BirthDate = new DateOnly(2021, 2, 18),
                Sterilized = true,
                BreedId = SeedConstants.Breed1Id,
                Cost = 35,
                Features = "Pelagem sedosa, olhos verdes",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal13Id,
                Name = "Rex",
                AnimalState = AnimalState.Available,
                Description = "Cão ativo e brincalhão, adora crianças.",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Castanho avermelhado",
                BirthDate = new DateOnly(2020, 9, 5),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 55,
                Features = "Muito energético, gosta de correr",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal14Id,
                Name = "Simba",
                AnimalState = AnimalState.Available,
                Description = "Gato jovem e curioso, adora explorar.",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Male,
                Colour = "Laranja tigrado",
                BirthDate = new DateOnly(2022, 6, 10),
                Sterilized = false,
                BreedId = SeedConstants.Breed1Id,
                Cost = 28,
                Features = "Muito brincalhão e ativo",
                ShelterId = SeedConstants.Shelter1Id,
            },
            new()
            {
                Id = SeedConstants.Animal15Id,
                Name = "NotifTestDog",
                AnimalState = AnimalState.Available,
                Description = "Animal exclusivo para testes de notificações",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Preto e Branco",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = SeedConstants.Breed2Id,
                Cost = 50,
                Features = "Animal de teste isolado - Notifications",
                ShelterId = SeedConstants.Shelter3Id,
            }
        };
    }

    private static List<Animal> GetOwnershipRequestTestAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.OwnershipAnimal1Id,
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
                ShelterId = SeedConstants.OwnershipShelter1Id,
                BreedId = SeedConstants.OwnershipBreed1Id,
                OwnerId = null
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal2Id,
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
                ShelterId = SeedConstants.OwnershipShelter1Id,
                BreedId = SeedConstants.OwnershipBreed2Id,
                OwnerId = null
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal3Id,
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
                ShelterId = SeedConstants.OwnershipShelter2Id,
                BreedId = SeedConstants.OwnershipBreed3Id,
                OwnerId = null
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal7Id,
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
                ShelterId = SeedConstants.OwnershipShelter2Id,
                BreedId = SeedConstants.OwnershipBreed4Id,
                OwnerId = null
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal8Id,
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
                ShelterId = SeedConstants.OwnershipShelter1Id,
                BreedId = SeedConstants.OwnershipBreed1Id,
                OwnerId = null
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal4Id,
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
                ShelterId = SeedConstants.OwnershipShelter1Id,
                BreedId = SeedConstants.OwnershipBreed4Id,
                OwnerId = SeedConstants.OwnershipUser2Id,
                OwnershipStartDate = DateTime.UtcNow.AddMonths(-6)
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal5Id,
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
                ShelterId = SeedConstants.OwnershipShelter2Id,
                BreedId = SeedConstants.OwnershipBreed1Id,
                OwnerId = SeedConstants.OwnershipUser2Id,
                OwnershipStartDate = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = SeedConstants.OwnershipAnimal6Id,
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
                ShelterId = SeedConstants.OwnershipShelter1Id,
                BreedId = SeedConstants.OwnershipBreed2Id,
                OwnerId = SeedConstants.OwnershipUser2Id,
                OwnershipStartDate = DateTime.UtcNow.AddMonths(-10)
            }
        };
    }

    private static List<Animal> GetFosteringTestAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.AnimalF1Id,
                Name = "Rex",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalF2Id,
                Name = "Luna",
                AnimalState = AnimalState.TotallyFostered,
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "White",
                BirthDate = new DateOnly(2021, 6, 20),
                Sterilized = true,
                Cost = 40,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalF3Id,
                Name = "Max",
                AnimalState = AnimalState.TotallyFostered,
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Black",
                BirthDate = new DateOnly(2019, 11, 10),
                Sterilized = true,
                Cost = 60,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalFInactiveId,
                Name = "Inactive Dog",
                AnimalState = AnimalState.Inactive,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Gray",
                BirthDate = new DateOnly(2018, 1, 1),
                Sterilized = true,
                Cost = 45,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalFAvailableId,
                Name = "Available Dog",
                AnimalState = AnimalState.Available,
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Orange",
                BirthDate = new DateOnly(2022, 4, 5),
                Sterilized = false,
                Cost = 35.00m,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalWithSlotId,
                Name = "Busy Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Golden",
                BirthDate = new DateOnly(2020, 8, 12),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalWithActivityId,
                Name = "Active Dog",
                AnimalState = AnimalState.TotallyFostered,
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Spotted",
                BirthDate = new DateOnly(2021, 2, 28),
                Sterilized = true,
                Cost = 42.00m,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalNotFosteredId,
                Name = "Not Fostered Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "White",
                BirthDate = new DateOnly(2020, 5, 10),
                Sterilized = true,
                Cost = 45.00m,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalShelterTestId,
                Name = "Buddy - Shelter Test",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = SeedConstants.FosteringShelterId,
                BreedId = SeedConstants.FosteringBreedId,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Animal> GetCancelFosteringTestAnimals()
    {
        return new List<Animal>
        {
            new()
            {
                Id = SeedConstants.AnimalC1Id,
                Name = "Cancel Rex",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC2Id,
                Name = "Other User Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "White",
                BirthDate = new DateOnly(2021, 6, 20),
                Sterilized = true,
                Cost = 40,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC3Id,
                Name = "Cancelled Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Black",
                BirthDate = new DateOnly(2019, 11, 10),
                Sterilized = true,
                Cost = 60,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC4Id,
                Name = "Completed Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Orange",
                BirthDate = new DateOnly(2022, 4, 5),
                Sterilized = false,
                Cost = 35,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC5Id,
                Name = "Past Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Golden",
                BirthDate = new DateOnly(2020, 8, 12),
                Sterilized = true,
                Cost = 50,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC6Id,
                Name = "No Fostering Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "Spotted",
                BirthDate = new DateOnly(2021, 2, 28),
                Sterilized = true,
                Cost = 42,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC7Id,
                Name = "Ownership Dog",
                AnimalState = AnimalState.HasOwner,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "White",
                BirthDate = new DateOnly(2020, 5, 10),
                Sterilized = true,
                Cost = 45,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedConstants.AnimalC8Id,
                Name = "Available Slot Dog",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Gray",
                BirthDate = new DateOnly(2020, 7, 15),
                Sterilized = true,
                Cost = 50,
                ShelterId = SeedConstants.CancelShelterId,
                BreedId = SeedConstants.CancelBreedId,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}