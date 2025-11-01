using Application.Animals.Commands;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.Animals.Commands;

/// <summary>
/// Unit tests for EditAnimal.Handler using equivalence partitioning and boundary value analysis.
/// Tests the Handle method which updates an existing animal entity within a specific shelter context.
/// </summary>
public class EditAnimalTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly EditAnimal.Handler _handler;

    public EditAnimalTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Animal, Animal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        });
        _mapper = config.CreateMapper();

        _handler = new EditAnimal.Handler(_dbContext, _mapper);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test shelter with minimal required properties.
    /// </summary>
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

    /// <summary>
    /// Creates a test breed with minimal required properties.
    /// </summary>
    private Breed CreateBreed(string? id = null, string name = "Test Breed")
    {
        return new Breed
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = name,
            Description = "Test breed description"
        };
    }

    /// <summary>
    /// Creates a valid animal entity for testing.
    /// </summary>
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
            ShelterId = shelterId ?? Guid.NewGuid().ToString(),
            BreedId = breedId ?? Guid.NewGuid().ToString()
        };
    }

    #endregion

    #region Success Cases

    /// <summary>
    /// Tests successful edit of animal with valid data.
    /// Equivalence Class: Valid animal, valid breed exists, all fields valid.
    /// </summary>
    [Fact]
    public async Task Handle_ValidEdit_ReturnsSuccess()
    {
        var shelter = CreateShelter();
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

        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.Code);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value.Name);
        Assert.Equal("Updated description", result.Value.Description);
        Assert.Equal(75.50m, result.Value.Cost);
    }

    /// <summary>
    /// Tests successful edit changing all modifiable enum properties.
    /// Equivalence Class: Valid enum transitions for AnimalState, Species, Size, Sex.
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Available, Species.Dog, SizeType.Small, SexType.Male)]
    [InlineData(AnimalState.PartiallyFostered, Species.Cat, SizeType.Medium, SexType.Female)]
    [InlineData(AnimalState.TotallyFostered, Species.Dog, SizeType.Large, SexType.Male)]
    [InlineData(AnimalState.HasOwner, Species.Cat, SizeType.Small, SexType.Female)]
    [InlineData(AnimalState.Inactive, Species.Dog, SizeType.Medium, SexType.Male)]
    public async Task Handle_ValidEnumChanges_ReturnsSuccess(
        AnimalState state, Species species, SizeType size, SexType sex)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.AnimalState = state;
        updatedAnimal.Species = species;
        updatedAnimal.Size = size;
        updatedAnimal.Sex = sex;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(state, result.Value!.AnimalState);
        Assert.Equal(species, result.Value.Species);
        Assert.Equal(size, result.Value.Size);
        Assert.Equal(sex, result.Value.Sex);
    }

    /// <summary>
    /// Tests successful edit with breed change.
    /// Equivalence Class: Valid breed change, breed exists in database.
    /// </summary>
    [Fact]
    public async Task Handle_ChangeBreed_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var originalBreed = CreateBreed(name: "Original Breed");
        var newBreed = CreateBreed(name: "New Breed");
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: originalBreed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.AddRange(originalBreed, newBreed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: newBreed.Id);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newBreed.Id, result.Value!.BreedId);
        Assert.Equal("New Breed", result.Value.Breed.Name);
    }

    /// <summary>
    /// Tests successful toggle of Sterilized property.
    /// Equivalence Class: Boolean field transitions (true to false, false to true).
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task Handle_ToggleSterilized_ReturnsSuccess(bool original, bool updated)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);
        animal.Sterilized = original;

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Sterilized = updated;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(updated, result.Value!.Sterilized);
    }

    #endregion

    #region Failure Cases

    /// <summary>
    /// Tests edit with non-existent breed.
    /// Equivalence Class: BreedId references non-existent record.
    /// </summary>
    [Fact]
    public async Task Handle_BreedNotFound_ReturnsFailure()
    {
        var shelter = CreateShelter();
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
        Assert.Equal(404, result.Code);
        Assert.Equal("Breed not found", result.Error);
    }

    /// <summary>
    /// Tests edit with non-existent animal.
    /// Equivalence Class: AnimalId references non-existent record.
    /// </summary>
    [Fact]
    public async Task Handle_AnimalNotFound_ReturnsFailure()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: Guid.NewGuid().ToString(), shelterId: shelter.Id, breedId: breed.Id);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found or not owned by this shelter", result.Error);
    }

    /// <summary>
    /// Tests edit when database context is disposed.
    /// Equivalence Class: Database operation failure due to disposed context.
    /// </summary>
    [Fact]
    public async Task Handle_DisposedContext_ThrowsObjectDisposedException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var disposedContext = new AppDbContext(options);
        var handler = new EditAnimal.Handler(disposedContext, _mapper);

        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        disposedContext.Shelters.Add(shelter);
        disposedContext.Breeds.Add(breed);
        disposedContext.Animals.Add(animal);
        await disposedContext.SaveChangesAsync();

        disposedContext.Dispose();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        var command = new EditAnimal.Command { Animal = updatedAnimal };

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region Boundary Value Analysis - Cost

    /// <summary>
    /// Tests cost field at minimum boundary value (0).
    /// Boundary: Lower limit of valid cost range [0, 1000].
    /// </summary>
    [Fact]
    public async Task Handle_CostAtMinimumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Cost = 0m;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value!.Cost);
    }

    /// <summary>
    /// Tests cost field at maximum boundary value (1000).
    /// Boundary: Upper limit of valid cost range [0, 1000].
    /// </summary>
    [Fact]
    public async Task Handle_CostAtMaximumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Cost = 1000m;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1000m, result.Value!.Cost);
    }

    /// <summary>
    /// Tests cost field just inside boundaries.
    /// Boundary: Values just inside valid range (0.01 and 999.99).
    /// </summary>
    [Theory]
    [InlineData(0.01)]
    [InlineData(999.99)]
    public async Task Handle_CostJustInsideBoundaries_ReturnsSuccess(decimal cost)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Cost = cost;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(cost, result.Value!.Cost);
    }

    /// <summary>
    /// Tests cost field with typical valid values.
    /// Equivalence Class: Mid-range valid cost values.
    /// </summary>
    [Theory]
    [InlineData(25.50)]
    [InlineData(50.00)]
    [InlineData(100.75)]
    [InlineData(500.00)]
    public async Task Handle_CostValidMidRange_ReturnsSuccess(decimal cost)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Cost = cost;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(cost, result.Value!.Cost);
    }

    #endregion

    #region Boundary Value Analysis - Name Length

    /// <summary>
    /// Tests name field at minimum boundary length (2 characters).
    /// Boundary: Lower limit of valid name length [2, 100].
    /// </summary>
    [Theory]
    [InlineData("AB")]
    [InlineData("Bo")]
    [InlineData("Xi")]
    public async Task Handle_NameAtMinimumBoundary_ReturnsSuccess(string name)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Name = name;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(name, result.Value!.Name);
    }

    /// <summary>
    /// Tests name field at maximum boundary length (100 characters).
    /// Boundary: Upper limit of valid name length [2, 100].
    /// </summary>
    [Fact]
    public async Task Handle_NameAtMaximumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Name = new string('A', 100);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value!.Name.Length);
    }

    /// <summary>
    /// Tests name field just inside boundaries.
    /// Boundary: Values just inside valid range (3 and 99 characters).
    /// </summary>
    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    public async Task Handle_NameJustInsideBoundaries_ReturnsSuccess(int length)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Name = new string('A', length);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(length, result.Value!.Name.Length);
    }

    #endregion

    #region Boundary Value Analysis - Description Length

    /// <summary>
    /// Tests description field at maximum boundary length (250 characters).
    /// Boundary: Upper limit of valid description length [0, 250].
    /// </summary>
    [Fact]
    public async Task Handle_DescriptionAtMaximumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Description = new string('A', 250);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(250, result.Value!.Description!.Length);
    }

    /// <summary>
    /// Tests description field just inside maximum boundary.
    /// Boundary: Value just inside valid range (249 characters).
    /// </summary>
    [Fact]
    public async Task Handle_DescriptionJustInsideMaximum_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Description = new string('A', 249);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(249, result.Value!.Description!.Length);
    }

    #endregion

    #region Boundary Value Analysis - Features Length

    /// <summary>
    /// Tests features field at maximum boundary length (300 characters).
    /// Boundary: Upper limit of valid features length [0, 300].
    /// </summary>
    [Fact]
    public async Task Handle_FeaturesAtMaximumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Features = new string('A', 300);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, result.Value!.Features!.Length);
    }

    /// <summary>
    /// Tests features field just inside maximum boundary.
    /// Boundary: Value just inside valid range (299 characters).
    /// </summary>
    [Fact]
    public async Task Handle_FeaturesJustInsideMaximum_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Features = new string('A', 299);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(299, result.Value!.Features!.Length);
    }

    #endregion

    #region Boundary Value Analysis - Colour Length

    /// <summary>
    /// Tests colour field at maximum boundary length (50 characters).
    /// Boundary: Upper limit of valid colour length [1, 50].
    /// </summary>
    [Fact]
    public async Task Handle_ColourAtMaximumBoundary_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Colour = new string('A', 50);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value!.Colour.Length);
    }

    /// <summary>
    /// Tests colour field just inside maximum boundary.
    /// Boundary: Value just inside valid range (49 characters).
    /// </summary>
    [Fact]
    public async Task Handle_ColourJustInsideMaximum_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Colour = new string('A', 49);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(49, result.Value!.Colour.Length);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests edit with all optional fields set to null.
    /// Edge Case: Clearing all optional information.
    /// </summary>
    [Fact]
    public async Task Handle_ClearOptionalFields_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);
        animal.Description = "Original description";
        animal.Features = "Original features";

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Description = null;
        updatedAnimal.Features = null;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Description);
        Assert.Null(result.Value.Features);
    }

    /// <summary>
    /// Tests edit with empty strings for optional fields.
    /// Edge Case: Empty vs null optional fields.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyOptionalFields_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.Description = string.Empty;
        updatedAnimal.Features = string.Empty;

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value!.Description);
        Assert.Equal(string.Empty, result.Value.Features);
    }

    /// <summary>
    /// Tests edit with boundary birth dates.
    /// Edge Case: Very old dates and very recent dates.
    /// </summary>
    [Theory]
    [InlineData(1995, 1, 1)]
    [InlineData(2000, 6, 15)]
    [InlineData(2023, 12, 31)]
    public async Task Handle_BoundaryBirthDates_ReturnsSuccess(int year, int month, int day)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.BirthDate = new DateOnly(year, month, day);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(year, month, day), result.Value!.BirthDate);
    }

    /// <summary>
    /// Tests edit with today's date as birth date.
    /// Edge Case: Animal born today.
    /// </summary>
    [Fact]
    public async Task Handle_BirthDateToday_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);
        updatedAnimal.BirthDate = DateOnly.FromDateTime(DateTime.Today);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), result.Value!.BirthDate);
    }

    /// <summary>
    /// Tests edit without changing any data.
    /// Edge Case: No actual changes to the entity.
    /// </summary>
    [Fact]
    public async Task Handle_NoChanges_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateValidAnimal(shelterId: shelter.Id, breedId: breed.Id);

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var originalCreatedAt = animal.CreatedAt;

        var updatedAnimal = CreateValidAnimal(id: animal.Id, shelterId: shelter.Id, breedId: breed.Id);

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(originalCreatedAt, result.Value!.CreatedAt);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests realistic scenario with complete animal data update.
    /// Integration: Typical use case with multiple field updates.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticCompleteUpdate_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var originalBreed = CreateBreed(name: "Labrador");
        var newBreed = CreateBreed(name: "Golden Retriever");
        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Max",
            AnimalState = AnimalState.Available,
            Description = "Friendly dog",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Black",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = false,
            Cost = 60m,
            Features = "Good with kids",
            ShelterId = shelter.Id,
            BreedId = originalBreed.Id
        };

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.AddRange(originalBreed, newBreed);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var updatedAnimal = new Animal
        {
            Id = animal.Id,
            Name = "Maximus",
            AnimalState = AnimalState.PartiallyFostered,
            Description = "Very friendly and trained dog, loves children",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            Colour = "Golden",
            BirthDate = new DateOnly(2020, 3, 15),
            Sterilized = true,
            Cost = 85.50m,
            Features = "Trained, good with kids and other pets, vaccinated",
            ShelterId = shelter.Id,
            BreedId = newBreed.Id
        };

        var command = new EditAnimal.Command { Animal = updatedAnimal };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Maximus", result.Value!.Name);
        Assert.Equal(AnimalState.PartiallyFostered, result.Value.AnimalState);
        Assert.Equal("Golden", result.Value.Colour);
        Assert.True(result.Value.Sterilized);
        Assert.Equal(85.50m, result.Value.Cost);
        Assert.Equal(newBreed.Id, result.Value.BreedId);
        Assert.Contains("vaccinated", result.Value.Features);
    }

    #endregion
}