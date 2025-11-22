using Application.Images.Commands;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ImagesTests.Commands;

/// <summary>
/// Unit tests for SetAnimalPrincipalImage.Handler using equivalence partitioning and boundary value analysis.
/// Tests the Handle method which sets an image as the principal image for an animal.
/// </summary>
public class SetAnimalPrincipalImageTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly SetAnimalPrincipalImage.Handler _handler;
    private readonly string _testShelterId1;
    private readonly string _testShelterId2;
    private readonly User _testUser;

    public SetAnimalPrincipalImageTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
        
        _testShelterId1 = Guid.NewGuid().ToString();
        _testShelterId2 = Guid.NewGuid().ToString();
        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Name = "Test User",
            BirthDate = new DateTime(1990, 1, 1),
            Street = "Test Street",
            City = "Porto",
            PostalCode = "4000-123",
            PhoneNumber = "912345678",
            ShelterId = _testShelterId1
        };
        
        _mockUserAccessor.Setup(x => x.GetUserAsync())
            .ReturnsAsync(_testUser);
        
        _handler = new SetAnimalPrincipalImage.Handler(_dbContext, _mockUserAccessor.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test animal with minimal required properties.
    /// </summary>
    private Animal CreateAnimal(string id, string? shelterId = null)
    {
        var shelter = new Shelter
        {
            Id = shelterId ?? _testShelterId1,
            Name = "Test Shelter",
            Street = "123 Test Street",
            City = "Porto",
            PostalCode = "4000-123",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var breed = new Breed
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Breed"
        };

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        _dbContext.SaveChanges();

        return new Animal
        {
            Id = id,
            Name = "Test Animal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id,
            Images = new List<Image>()
        };
    }

    /// <summary>
    /// Creates a test image with specified properties.
    /// </summary>
    private static Image CreateImage(string id, string? animalId = null, bool isPrincipal = false)
    {
        return new Image
        {
            Id = id,
            PublicId = $"public-{id}",
            Url = $"https://cloudinary.com/{id}.jpg",
            Description = "Test image",
            IsPrincipal = isPrincipal,
            AnimalId = animalId
        };
    }

    #endregion

    #region Success Cases

    /// <summary>
    /// Tests setting principal image when no previous principal exists.
    /// Equivalence Class: Animal with images, none principal, valid image ID.
    /// </summary>
    [Fact]
    public async Task Handle_NoPreviousPrincipal_SetsPrincipalSuccessfully()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.Code);
        
        var updatedAnimal = await _dbContext.Animals
            .Include(a => a.Images)
            .FirstAsync(a => a.Id == animalId);
        var updatedImage = updatedAnimal.Images.First(i => i.Id == imageId);
        Assert.True(updatedImage.IsPrincipal);
    }

    /// <summary>
    /// Tests replacing existing principal image with a new one.
    /// Equivalence Class: Animal with existing principal, setting different image as principal.
    /// </summary>
    [Fact]
    public async Task Handle_WithExistingPrincipal_ReplacesSuccessfully()
    {
        var animalId = "animal-1";
        var oldPrincipalId = "image-old-principal";
        var newPrincipalId = "image-new-principal";
        
        var animal = CreateAnimal(animalId);
        var oldPrincipal = CreateImage(oldPrincipalId, animalId, isPrincipal: true);
        var newPrincipal = CreateImage(newPrincipalId, animalId, isPrincipal: false);
        animal.Images.Add(oldPrincipal);
        animal.Images.Add(newPrincipal);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = newPrincipalId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.Code);
        
        var updatedAnimal = await _dbContext.Animals
            .Include(a => a.Images)
            .FirstAsync(a => a.Id == animalId);
        
        var updatedOldPrincipal = updatedAnimal.Images.First(i => i.Id == oldPrincipalId);
        var updatedNewPrincipal = updatedAnimal.Images.First(i => i.Id == newPrincipalId);
        
        Assert.False(updatedOldPrincipal.IsPrincipal);
        Assert.True(updatedNewPrincipal.IsPrincipal);
    }

    /// <summary>
    /// Tests with multiple non-principal images.
    /// Equivalence Class: Animal with N images (N > 1), none principal.
    /// </summary>
    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(5, 2)]
    [InlineData(10, 5)]
    public async Task Handle_MultipleNonPrincipalImages_SetsCorrectOne(int totalImages, int indexToSetAsPrincipal)
    {
        var animalId = "animal-multi";
        var animal = CreateAnimal(animalId);
        
        var images = new List<Image>();
        for (int i = 0; i < totalImages; i++)
        {
            var image = CreateImage($"image-{i}", animalId, isPrincipal: false);
            images.Add(image);
            animal.Images.Add(image);
        }
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var targetImageId = $"image-{indexToSetAsPrincipal}";
        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = targetImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        var updatedAnimal = await _dbContext.Animals
            .Include(a => a.Images)
            .FirstAsync(a => a.Id == animalId);
        
        var principalCount = updatedAnimal.Images.Count(i => i.IsPrincipal);
        Assert.Equal(1, principalCount);
        
        var principalImage = updatedAnimal.Images.First(i => i.IsPrincipal);
        Assert.Equal(targetImageId, principalImage.Id);
    }

    /// <summary>
    /// Tests with various ID formats.
    /// Equivalence Class: Different valid ID formats.
    /// </summary>
    [Theory]
    [InlineData("animal-123", "image-456")]
    [InlineData("animal_underscore", "image_underscore")]
    [InlineData("ANIMAL-UPPER", "IMAGE-UPPER")]
    public async Task Handle_VariousIdFormats_WorksCorrectly(string animalId, string imageId)
    {
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        var updatedImage = await _dbContext.Images.FirstAsync(i => i.Id == imageId);
        Assert.True(updatedImage.IsPrincipal);
    }

    #endregion

    #region Authorization Tests

    /// <summary>
    /// Tests that user cannot set principal image for animals from different shelter.
    /// Authorization validation: ShelterId mismatch.
    /// </summary>
    [Fact]
    public async Task Handle_DifferentShelter_ReturnsForbidden()
    {
        var differentShelterId = Guid.NewGuid().ToString();
        var animalId = "animal-other-shelter";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId, differentShelterId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Contains("only change main image of animals from your shelter", result.Error);
    }

    /// <summary>
    /// Tests authorization check occurs before other validations.
    /// </summary>
    [Fact]
    public async Task Handle_AuthorizationCheckedBeforeImageValidation()
    {
        var differentShelterId = Guid.NewGuid().ToString();
        var animalId = "animal-other-shelter";
        var nonExistentImageId = "non-existent-image";
        
        var animal = CreateAnimal(animalId, differentShelterId);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = nonExistentImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
    }

    #endregion

    #region Error Cases

    /// <summary>
    /// Tests with non-existent animal ID.
    /// Equivalence Class: Invalid animal reference.
    /// </summary>
    [Fact]
    public async Task Handle_AnimalNotFound_ReturnsNotFound()
    {
        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = "non-existent",
            ImageId = "image-1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
    }

    /// <summary>
    /// Tests with non-existent image ID.
    /// Equivalence Class: Invalid image reference.
    /// </summary>
    [Fact]
    public async Task Handle_ImageNotFound_ReturnsNotFound()
    {
        var animalId = "animal-1";
        var animal = CreateAnimal(animalId);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = "non-existent-image"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Image not found", result.Error);
    }

    /// <summary>
    /// Tests attempting to set already-principal image as principal.
    /// Equivalence Class: Redundant operation.
    /// </summary>
    [Fact]
    public async Task Handle_ImageAlreadyPrincipal_ReturnsBadRequest()
    {
        var animalId = "animal-1";
        var imageId = "image-principal";
        
        var animal = CreateAnimal(animalId);
        var principalImage = CreateImage(imageId, animalId, isPrincipal: true);
        animal.Images.Add(principalImage);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("already is", result.Error);
    }

    /// <summary>
    /// Tests image that belongs to different animal.
    /// Equivalence Class: Mismatched ownership.
    /// </summary>
    [Fact]
    public async Task Handle_ImageBelongsToDifferentAnimal_ReturnsForbidden()
    {
        var animal1Id = "animal-1";
        var animal2Id = "animal-2";
        var imageId = "image-1";
        
        var animal1 = CreateAnimal(animal1Id, _testShelterId1);
        var animal2 = CreateAnimal(animal2Id, _testShelterId2);
        var image = CreateImage(imageId, animal2Id, isPrincipal: false);
        animal2.Images.Add(image);
        
        _dbContext.Animals.AddRange(animal1, animal2);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animal1Id,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Equal("Image does not belong to the specified animal.", result.Error);
    }

    /// <summary>
    /// Tests that already-principal check is after ownership validation.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationOrder_AlreadyPrincipalCheckedAfterOwnership()
    {
        var animalId = "animal-1";
        var imageId = "image-principal";
        
        var animal = CreateAnimal(animalId);
        var principalImage = CreateImage(imageId, animalId, isPrincipal: true);
        animal.Images.Add(principalImage);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Contains("already is", result.Error);
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that cancellation token is respected throughout the operation.
    /// Verifies proper async cancellation support.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesToDatabaseOperations()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        using var cts = new CancellationTokenSource();

        var result = await _handler.Handle(command, cts.Token);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Boundary Value Analysis

    /// <summary>
    /// Tests with very long IDs.
    /// Boundary: Maximum practical ID length.
    /// </summary>
    [Fact]
    public async Task Handle_VeryLongIds_HandlesCorrectly()
    {
        var longAnimalId = new string('a', 500);
        var longImageId = new string('b', 500);
        
        var animal = CreateAnimal(longAnimalId);
        var image = CreateImage(longImageId, longAnimalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = longAnimalId,
            ImageId = longImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests with animal having maximum practical number of images.
    /// Boundary: Large image collection.
    /// </summary>
    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Handle_AnimalWithManyImages_HandlesCorrectly(int imageCount)
    {
        const string animalId = "animal-many";
        var animal = CreateAnimal(animalId);
        
        for (var i = 0; i < imageCount; i++)
        {
            var image = CreateImage($"image-{i}", animalId, isPrincipal: i == 0);
            animal.Images.Add(image);
        }
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var targetImageId = "image-25";
        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = targetImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        
        var updatedAnimal = await _dbContext.Animals
            .Include(a => a.Images)
            .FirstAsync(a => a.Id == animalId);
        
        Assert.Single(updatedAnimal.Images, i => i.IsPrincipal);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests realistic scenario: changing principal image multiple times.
    /// Integration test covering typical use case sequence.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticScenario_ChangePrincipalMultipleTimes()
    {
        var animalId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(animalId);
        
        var img1 = CreateImage("img-1", animalId, isPrincipal: true);
        var img2 = CreateImage("img-2", animalId, isPrincipal: false);
        var img3 = CreateImage("img-3", animalId, isPrincipal: false);
        
        animal.Images.Add(img1);
        animal.Images.Add(img2);
        animal.Images.Add(img3);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command1 = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = "img-2"
        };
        await _handler.Handle(command1, CancellationToken.None);

        var command2 = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = "img-3"
        };
        await _handler.Handle(command2, CancellationToken.None);

        var finalAnimal = await _dbContext.Animals
            .Include(a => a.Images)
            .FirstAsync(a => a.Id == animalId);
        
        Assert.Single(finalAnimal.Images, i => i.IsPrincipal);
        Assert.True(finalAnimal.Images.First(i => i.Id == "img-3").IsPrincipal);
        Assert.False(finalAnimal.Images.First(i => i.Id == "img-1").IsPrincipal);
        Assert.False(finalAnimal.Images.First(i => i.Id == "img-2").IsPrincipal);
    }

    /// <summary>
    /// Tests scenario with GUID-formatted IDs.
    /// Common real-world case.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticScenario_WithGuidIds()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(image);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests setting principal when animal has exactly one image.
    /// Boundary: Minimum non-empty image collection.
    /// </summary>
    [Fact]
    public async Task Handle_AnimalWithSingleImage_SetsPrincipal()
    {
        var animalId = "animal-single";
        var imageId = "image-only";
        
        var animal = CreateAnimal(animalId);
        var onlyImage = CreateImage(imageId, animalId, isPrincipal: false);
        animal.Images.Add(onlyImage);
        
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new SetAnimalPrincipalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion
}