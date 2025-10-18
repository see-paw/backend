using Application.Animals.Queries;
using Application.Shelters.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests;

public class GetAnimalsByShelterTests
{
    private AppDbContext CreateInMemoryContext(List<Animal> animals, List<Shelter>? shelters = null, List<Breed>? breeds = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;

        var context = new AppDbContext(options);

        if (breeds != null)
            context.Breeds.AddRange(breeds);

        if (shelters != null)
            context.Shelters.AddRange(shelters);

        context.Animals.AddRange(animals);
        context.SaveChanges();

        return context;
    }

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

    private Breed CreateBreed(string breedId)
    {
        return new Breed
        {
            Id = breedId,
            Name = "Golden Retriever"
        };
    }

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

    [Fact]
    public async Task GetAnimalsForValidShelter()
    {
        var shelterId = "shelter1";
        var breedId = "breed1";
        var animals = new List<Animal>
        {
            CreateAnimal("Charlie", shelterId, breedId),
            CreateAnimal("Buddy", shelterId, breedId)
        };
        var shelters = new List<Shelter> { CreateShelter(shelterId) };
        var breeds = new List<Breed> { CreateBreed(breedId) };

        var context = CreateInMemoryContext(animals, shelters, breeds);
        var handler = new GetAnimalsByShelter.Handler(context);
        var query = new GetAnimalsByShelter.Query { ShelterId = shelterId, PageNumber = 1 };//valid page number

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

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

    [Fact]
    public async Task ShouldCalculateTotalPagesCorrectly()
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