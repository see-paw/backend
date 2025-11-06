using Application.Animals.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests.AnimalControllerTest.cs;

/// <summary>
/// Contains unit tests for the GetAnimalDetails.Handler logic.
/// </summary>
/// <remarks>
/// Verifies correct handler behavior for various animal states,
/// including non-existent, non-retrievable, and valid animals.
/// </remarks>
public class GetAnimalDetailsHandlerTest
{
    private readonly DbContextOptions<AppDbContext> _options;

    /// <summary>
    /// Initializes an in-memory database for handler testing.
    /// </summary>
    public GetAnimalDetailsHandlerTest()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    /// <summary>
    /// Ensures that a non-existent animal ID returns a not found result.
    /// </summary>
    [Theory]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff", 404, "Animal not found")]
    public async Task NonExistentId_ReturnsNotFound(string invalidId, int expectedCode, string expectedError)
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var handler = new GetAnimalDetails.Handler(context);
        var query = new GetAnimalDetails.Query { Id = invalidId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.Code);
        Assert.Equal(expectedError, result.Error);
    }

    /// <summary>
    /// Ensures that animals in non-retrievable states return a not found result.
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Inactive)]
    [InlineData(AnimalState.HasOwner)]
    [InlineData(AnimalState.TotallyFostered)]
    public async Task NonRetrievableStates_ReturnsNotFound(AnimalState invalidState)
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var animalId = Guid.NewGuid().ToString();
        var breedId = Guid.NewGuid().ToString();

        var breed = new Breed { Id = breedId, Name = "Golden Retriever" };
        
        var animal = new Animal
        {
            Id = animalId,
            Name = "Unavailable Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            AnimalState = invalidState,
            Colour = "Brown",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            Sterilized = true,
            Cost = 100m,
            ShelterId = Guid.NewGuid().ToString(),
            BreedId = breedId
        };

        var images = new List<Image>
        {
            new() { Url = "https://example.com/max1.jpg",
                IsPrincipal = true,
                AnimalId = animalId,
                Description = "Gold and beautiful",
                PublicId = "images_cq2q0f"
            },
            new() { Url = "https://example.com/max2.jpg",
                IsPrincipal = false,
                AnimalId = animalId,
                Description = "Gold and beautiful",
                PublicId = "images_cq2q0f"
            }
        };

        context.Breeds.Add(breed);
        context.Animals.Add(animal);
        context.Images.AddRange(images);
        await context.SaveChangesAsync();

        var handler = new GetAnimalDetails.Handler(context);
        var query = new GetAnimalDetails.Query { Id = animalId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not retrievable", result.Error);
    }

    /// <summary>
    /// Ensures that valid animals in available or partially fostered states return success.
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    public async Task ValidAvailableAnimal_ReturnsSuccess(AnimalState animalState)
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var animalId = Guid.NewGuid().ToString();
        var breedId = Guid.NewGuid().ToString();

        var breed = new Breed
        {
            Id = breedId, 
            Name = "Golden Retriever",
            Description = "Gold and beautiful"
        };
        
        var animal = new Animal
        {
            Id = animalId,
            Name = "Max",
            Species = Species.Dog,
            Size = SizeType.Large,
            Sex = SexType.Male,
            AnimalState = animalState,
            Colour = "Golden",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
            Sterilized = true,
            Cost = 150m,
            ShelterId = Guid.NewGuid().ToString(),
            BreedId = breedId,
        };

        var images = new List<Image>
        {
            new()
            {
                Url = "https://example.com/max1.jpg", 
                IsPrincipal = true, 
                AnimalId = animalId,
                Description = "Image of a dog",
                PublicId = "images_cq2q0f"
            },
            new() { Url = "https://example.com/max2.jpg", 
                IsPrincipal = false, 
                AnimalId = animalId,
                Description = "Image of a dog",
                PublicId = "images_cq2q0f"
            }
        };

        context.Breeds.Add(breed);
        context.Animals.Add(animal);
        context.Images.AddRange(images);
        await context.SaveChangesAsync();

        var handler = new GetAnimalDetails.Handler(context);
        var query = new GetAnimalDetails.Query { Id = animalId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Golden Retriever", result.Value?.Breed.Name);
        if (result != null)
        {
            Assert.Equal(2, result.Value.Images.Count);
            Assert.Contains(result.Value.Images, img => img.IsPrincipal);
            Assert.Equal(200, result.Code);
            Assert.NotNull(result.Value);
            Assert.Equal(animalId, result.Value.Id);
        }
    }
}
