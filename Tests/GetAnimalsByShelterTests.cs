using Application.Animals.Queries;
using Application.Shelters.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests;

public class GetAnimalsByShelterTests
{
    // ===== Helper method =====
    // Creates an AnimalsController with an in-memory database.
    // This avoids the need for a real SQL Server and makes tests fast and isolated.
    private AppDbContext CreateInMemoryContext(List<Animal> animals, List<Shelter>? shelters = null, List<Breed>? breeds = null)
    {
        // Create in-memory database options.
        // Each test uses a unique database name (via Guid) so they never share data.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        // Create the in-memory context.
        var context = new AppDbContext(options);

        // Add breeds if provided.
        if (breeds != null)
            context.Breeds.AddRange(breeds);

        // Add a shelter if provided.
        if (shelters != null)
            context.Shelters.AddRange(shelters);

        // Add animals to the context.
        context.Animals.AddRange(animals);

        // Save all changes to the in-memory store.
        context.SaveChanges();

        // Return the ready-to-use context.
        return context;
    }


    // ===== Helper method =====
    // Creates a valid shelter that satisfies all validation rules from the Shelter entity.
    private Shelter CreateShelter(string shelterId)
    {
        return new Shelter
        {
            Id = shelterId,
            Name = "Happy Paws Shelter",
            Street = "123 Animal Street",
            City = "Porto",
            PostalCode = "4000-123",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0, 0),
            ClosingTime = new TimeOnly(18, 0, 0),
            CreatedAt = DateTime.UtcNow
        };
    }

    // ===== Helper method =====
    // Creates a valid breed that satisfies all validation rules from the Breed entity.
    private Breed CreateBreed(string breedId)
    {
        return new Breed
        {
            Id = breedId,
            Name = "Golden Retriever"
        };
    }

    // ===== Helper method =====
    // Creates a fully valid Animal for testing purposes.
    private Animal CreateAnimal(string name, string shelterId, string breedId, AnimalState state = AnimalState.Available)
    {
        return new Animal
        {
            Name = name,
            AnimalState = state,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            BreedId = breedId,
            Cost = 100m,
            Features = "Test animal",
            CreatedAt = DateTime.UtcNow,
            ShelterId = shelterId
        };
    }

    // ===== Tests =====

  
    // Test: Ensure animals are returned in alphabetical order by name.
    [Fact]
    public async Task GetAnimalsByShelterInAlphabeticalOrder()
    {
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = new List<Animal>
        {
            CreateAnimal("Zebra", shelterId, breedId),
            CreateAnimal("Alpha", shelterId, breedId),
            CreateAnimal("Mike", shelterId, breedId)
        };
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal("Alpha", result.Value.First().Name);
    }

    // Test: Ensure pagination works correctly.
    [Fact]
    public async Task PaginateResultsCorrectlly()
    {
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = Enumerable.Range(1, 30)
            .Select(i => CreateAnimal($"Animal{i}", shelterId, breedId))
            .ToList();
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 2 };

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(10, result.Value.Count);
    }

    // Test: Ensure total pages are calculated correctly.
    [Fact]
    public async Task CalculateTotalPagesCorrectly()
    {
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = Enumerable.Range(1, 30)
            .Select(i => CreateAnimal($"Animal{i}", shelterId, breedId))
            .ToList();
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Value.TotalPages);
    }

    // Test: Get all animals for a valid shelter and valid page number.
    [Fact]
    public async Task GetAnimalsByShelter()
    {
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = new List<Animal>
        {
            CreateAnimal("Available", shelterId, breedId, AnimalState.Available),
            CreateAnimal("Fostered", shelterId, breedId, AnimalState.PartiallyFostered),
            CreateAnimal("Owner", shelterId, breedId, AnimalState.HasOwner),
            CreateAnimal("Adopted", shelterId, breedId, AnimalState.Inactive)
        };
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(4, result.Value.Count);
    }

    // Test: No animals found for the given shelter.
    [Fact]
    public async Task NoAnimalsFoundForShelter()
    {
        var shelterId = "shelter1";
        var animals = new List<Animal>();
        var shelters = new List<Shelter> { CreateShelter(shelterId) };

        var context = CreateInMemoryContext(animals, shelters);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    // Test: Ensure animals from other shelters are not returned.
    [Fact]
    public async Task ShouldNotReturnAnimalsFromOtherShelters()
    {
        var shelter1Id = "shelter1";
        var shelter2Id = "shelter2";
        var breedId = "breed1";
        var animals = new List<Animal>
        {
            CreateAnimal("Charlie", shelter1Id, breedId),
            CreateAnimal("Buddy", shelter2Id, breedId)
        };
        var shelters = new List<Shelter>
        {
            CreateShelter(shelter1Id),
            CreateShelter(shelter2Id)
        };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelter1Id, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, result.Value.Count);
    }

    // Test: Shelter does not exist.
    [Fact]
    public async Task ShelterDoesNotExist()
    {
        var nonExistentShelterId = "shelter-does-not-exist";
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = new List<Animal>
    {
        CreateAnimal("Charlie", shelterId, breedId)
    };
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = nonExistentShelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}