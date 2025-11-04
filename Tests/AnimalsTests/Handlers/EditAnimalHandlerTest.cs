using Application.Animals.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.AnimalsTests.Handlers;

public class EditAnimalTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IUserAccessor _userAccessor;
    private readonly EditAnimal.Handler _handler;
    private const string TestShelterId = "test-shelter-id";

    public EditAnimalTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Animal, Animal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        });
        _mapper = config.CreateMapper();

        var mockUserAccessor = new Mock<IUserAccessor>();
        mockUserAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test User",
                ShelterId = TestShelterId
            });

        _userAccessor = mockUserAccessor.Object;
        _handler = new EditAnimal.Handler(_dbContext, _mapper, _userAccessor);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private Shelter CreateShelter(string? id = null)
    {
        return new Shelter
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "123 Test Street",
            City = "Porto",
            PostalCode = "4000-123",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };
    }

    private Breed CreateBreed(string? id = null, string name = "Test Breed")
    {
        return new Breed
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name,
            Description = "Test breed description"
        };
    }

    private Animal CreateValidAnimal(string? id = null, string? shelterId = null, string? breedId = null)
    {
        return new Animal
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = "Test Animal",
            AnimalState = AnimalState.Available,
            Description = "Friendly test animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            Features = "Healthy and friendly",
            ShelterId = shelterId ?? TestShelterId,
            BreedId = breedId ?? Guid.NewGuid().ToString()
        };
    }

    [Fact]
    public async Task Handle_ValidEdit_ReturnsSuccess()
    {
        var shelter = CreateShelter(TestShelterId);
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Name = "Updated Name";
        updatedAnimal.Description = "Updated description";
        updatedAnimal.Cost = 75.50m;

        var command = new EditAnimal.Command { Animal = updatedAnimal };
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(200, result.Code);
        Assert.Equal("Updated Name", result.Value!.Name);
        Assert.Equal("Updated description", result.Value.Description);
        Assert.Equal(75.50m, result.Value.Cost);
    }

    [Fact]
    public async Task Handle_AnimalNotFound_ReturnsFailure()
    {
        var shelter = CreateShelter(TestShelterId);
        var breed = CreateBreed();

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: Guid.NewGuid().ToString(), shelterId: shelter.Id, breedId: breed.Id);
        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Animal not found", result.Error);
        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Handle_BreedNotFound_ReturnsFailure()
    {
        var shelter = CreateShelter(TestShelterId);
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: Guid.NewGuid().ToString());
        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Breed not found", result.Error);
        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Handle_DifferentShelterUser_ReturnsFailure()
    {
        var differentShelter = CreateShelter(Guid.NewGuid().ToString());
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: differentShelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(differentShelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: differentShelter.Id, breedId: breed.Id);
        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Animal not owned by this shelter", result.Error);
        Assert.Equal(404, result.Code);
    }
}
