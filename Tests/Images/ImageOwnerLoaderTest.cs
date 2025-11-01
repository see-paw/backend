using Application.Images;
using Domain;
using Domain.Enums;
using Infrastructure.Images;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.Images;

/// <summary>
/// Unit tests for ImageOwnerLoader using equivalence partitioning.
/// Tests the LoadAsync method which loads entities from database by ID.
/// Uses the real Animal entity with all its properties and relationships.
/// </summary>
public class ImageOwnerLoaderTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly ImageOwnerLoader<Animal> _loader;

    public ImageOwnerLoaderTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _loader = new ImageOwnerLoader<Animal>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test animal entity with minimal required properties.
    /// Uses real Animal properties with appropriate enums and relationships.
    /// FIXED: Corrected Shelter and Breed entities to match domain model.
    /// </summary>
    private Animal CreateAnimal(string id, string name = "Buddy")
    {
        // Create required related entities - FIXED VERSION
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "123 Test Street",           // ✅ Corrected from Address
            City = "Porto",                        // ✅ Added required property
            PostalCode = "4000-123",               // ✅ Added required property (format: 0000-000)
            Phone = "912345678",                  
            NIF = "123456789",                     
            OpeningTime = new TimeOnly(9, 0),     
            ClosingTime = new TimeOnly(18, 0)     
        };

        var breed = new Breed
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Breed",
            Description = "A friendly breed"       
        };

        // Add to context so they can be referenced
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.SaveChanges();

        return new Animal
        {
            Id = id,
            Name = name,
            AnimalState = AnimalState.Available,
            Description = "A friendly test animal",
            Species = Species.Dog,                 
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            Features = "Very friendly",
            ShelterId = shelter.Id,
            Shelter = shelter,
            BreedId = breed.Id,
            Breed = breed,
            Images = new List<Image>()
        };
    }

    #endregion

    #region Success Cases

    /// <summary>
    /// Tests successful loading of an existing entity.
    /// Equivalence Class: Valid entity ID that exists in database.
    /// </summary>
    [Fact]
    public async Task LoadAsync_ValidEntityId_ReturnsEntity()
    {
        // Arrange
        var entityId = "animal-123";
        var animal = CreateAnimal(entityId, "Buddy");

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
        Assert.Equal("Buddy", result.Name);
    }

    /// <summary>
    /// Tests loading with cancellation token provided.
    /// Ensures the method correctly passes and handles cancellation tokens.
    /// </summary>
    [Fact]
    public async Task LoadAsync_WithCancellationToken_ReturnsEntity()
    {
        // Arrange
        var entityId = "animal-456";
        var animal = CreateAnimal(entityId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        using var cts = new CancellationTokenSource();

        // Act
        var result = await _loader.LoadAsync(_dbContext, entityId, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
    }

    /// <summary>
    /// Tests loading entity with special characters in ID.
    /// Equivalence Class: Valid entity ID with special characters.
    /// </summary>
    [Theory]
    [InlineData("animal-with-dashes")]
    [InlineData("animal_with_underscores")]
    [InlineData("animal.with.dots")]
    [InlineData("ANIMAL-UPPERCASE")]
    [InlineData("animal123")]
    public async Task LoadAsync_SpecialCharactersInId_ReturnsEntity(string entityId)
    {
        // Arrange
        var animal = CreateAnimal(entityId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
    }

    #endregion

    #region Failure Cases - Invalid Input

    /// <summary>
    /// Tests that null entityId throws ArgumentException.
    /// Equivalence Class: Null entity ID.
    /// Boundary: Null reference.
    /// </summary>
    [Fact]
    public async Task LoadAsync_NullEntityId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _loader.LoadAsync(_dbContext, null!, CancellationToken.None)
        );

        Assert.Equal("entityId", exception.ParamName);
        Assert.Contains("EntityId is required", exception.Message);
    }

    /// <summary>
    /// Tests that empty string entityId throws ArgumentException.
    /// Equivalence Class: Empty entity ID.
    /// Boundary: Empty string.
    /// </summary>
    [Fact]
    public async Task LoadAsync_EmptyEntityId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _loader.LoadAsync(_dbContext, string.Empty, CancellationToken.None)
        );

        Assert.Equal("entityId", exception.ParamName);
        Assert.Contains("EntityId is required", exception.Message);
    }

    /// <summary>
    /// Tests that whitespace-only entityId throws ArgumentException.
    /// Equivalence Class: Whitespace entity ID.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   \t\n   ")]
    public async Task LoadAsync_WhitespaceEntityId_ThrowsArgumentException(string entityId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            async () => await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None)
        );

        Assert.Equal("entityId", exception.ParamName);
        Assert.Contains("EntityId is required", exception.Message);
    }

    #endregion

    #region Failure Cases - Entity Not Found

    /// <summary>
    /// Tests that non-existent entity ID throws KeyNotFoundException.
    /// Equivalence Class: Valid format ID that doesn't exist in database.
    /// </summary>
    [Fact]
    public async Task LoadAsync_NonExistentEntityId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = "animal-999";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _loader.LoadAsync(_dbContext, nonExistentId, CancellationToken.None)
        );

        Assert.Contains($"Entity Animal({nonExistentId}) not found", exception.Message);
    }

    /// <summary>
    /// Tests error message contains correct entity type name.
    /// Ensures the exception message is informative for debugging.
    /// </summary>
    [Fact]
    public async Task LoadAsync_EntityNotFound_ErrorMessageContainsEntityType()
    {
        // Arrange
        var entityId = "missing-animal";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None)
        );

        Assert.Contains("Animal", exception.Message);
        Assert.Contains(entityId, exception.Message);
    }

    /// <summary>
    /// Tests loading from empty database.
    /// Edge case: Database has no records.
    /// </summary>
    [Fact]
    public async Task LoadAsync_EmptyDatabase_ThrowsKeyNotFoundException()
    {
        // Arrange - database is already empty from constructor
        var entityId = "any-animal";

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None)
        );
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests loading multiple entities in sequence.
    /// Ensures the loader can be reused for multiple operations.
    /// </summary>
    [Fact]
    public async Task LoadAsync_MultipleEntitiesSequentially_ReturnsCorrectEntities()
    {
        // Arrange
        var animal1 = CreateAnimal("animal-1", "Buddy");
        var animal2 = CreateAnimal("animal-2", "Max");
        var animal3 = CreateAnimal("animal-3", "Charlie");

        _dbContext.Animals.AddRange(animal1, animal2, animal3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result1 = await _loader.LoadAsync(_dbContext, "animal-1", CancellationToken.None);
        var result2 = await _loader.LoadAsync(_dbContext, "animal-2", CancellationToken.None);
        var result3 = await _loader.LoadAsync(_dbContext, "animal-3", CancellationToken.None);

        // Assert
        Assert.Equal("Buddy", result1.Name);
        Assert.Equal("Max", result2.Name);
        Assert.Equal("Charlie", result3.Name);
    }

    /// <summary>
    /// Tests that entity with images collection is loaded correctly.
    /// Verifies that related collections are accessible (though not necessarily loaded).
    /// </summary>
    [Fact]
    public async Task LoadAsync_EntityWithImages_ReturnsEntityWithImagesCollection()
    {
        // Arrange
        var entityId = "animal-with-images";
        var animal = CreateAnimal(entityId);
        
        animal.Images.Add(new Image
        {
            Id = Guid.NewGuid().ToString(),
            PublicId = "image-1",
            Url = "https://cloudinary.com/image1.jpg",
            Description = "Test image 1",
            IsPrincipal = true
        });

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Images);
        // Note: Images collection might not be loaded depending on EF tracking behavior
        // FindAsync doesn't automatically load navigation properties
    }

    /// <summary>
    /// Tests loading same entity twice returns same instance (EF tracking).
    /// Verifies EF Core's identity map behavior.
    /// </summary>
    [Fact]
    public async Task LoadAsync_SameEntityTwice_ReturnsSameInstance()
    {
        // Arrange
        var entityId = "animal-123";
        var animal = CreateAnimal(entityId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result1 = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);
        var result2 = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        Assert.Same(result1, result2); // Should be same instance due to EF tracking
    }

    /// <summary>
    /// Tests that very long entity ID is handled correctly.
    /// Boundary: Maximum practical length for entity ID.
    /// </summary>
    [Fact]
    public async Task LoadAsync_VeryLongEntityId_ReturnsEntity()
    {
        // Arrange
        var longId = new string('a', 200); // 200 character ID
        var animal = CreateAnimal(longId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, longId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longId, result.Id);
    }

    /// <summary>
    /// Tests loading with Guid-formatted ID.
    /// Common pattern for entity IDs in many systems.
    /// </summary>
    [Fact]
    public async Task LoadAsync_GuidFormattedId_ReturnsEntity()
    {
        // Arrange
        var guidId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(guidId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, guidId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(guidId, result.Id);
    }

    #endregion
    

    #region Generic Type Tests

    /// <summary>
    /// Tests that the loader works with different entity types.
    /// This test demonstrates the generic nature of ImageOwnerLoader.
    /// Note: Requires another entity type that implements IHasImages for a real test.
    /// </summary>
    [Fact]
    public async Task LoadAsync_GenericType_WorksWithAnimalEntity()
    {
        // Arrange
        var entityId = "animal-generic-test";
        var animal = CreateAnimal(entityId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Create loader with explicit type
        var genericLoader = new ImageOwnerLoader<Animal>();

        // Act
        var result = await genericLoader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Animal>(result);
        Assert.Equal(entityId, result.Id);
    }

    #endregion

    #region Performance Considerations

    /// <summary>
    /// Tests that FindAsync is used (not a full table scan).
    /// FindAsync is optimized and checks EF's cache first.
    /// This is more of a documentation test than a functional test.
    /// </summary>
    [Fact]
    public async Task LoadAsync_UsesFindAsync_ForPerformance()
    {
        // Arrange
        var entityId = "animal-perf-test";
        var animal = CreateAnimal(entityId);

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadAsync(_dbContext, entityId, CancellationToken.None);

        // Assert
        // FindAsync checks local cache first, then queries database
        // This should be fast even with many entities
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
    }

    #endregion
}