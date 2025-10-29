using Application.Images.Commands;
using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Images.Commands;

/// <summary>
/// Unit tests for DeleteAnimalImage.Handler using equivalence partitioning and boundary value analysis.
/// Tests the Handle method which deletes an image from an animal.
/// </summary>
public class DeleteAnimalImageTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImageAppService<Animal>> _mockImageService;
    private readonly DeleteAnimalImage.Handler _handler;

    public DeleteAnimalImageTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mockImageService = new Mock<IImageAppService<Animal>>();
        _handler = new DeleteAnimalImage.Handler(_dbContext, _mockImageService.Object);
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
    private Animal CreateAnimal(string id)
    {
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
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
    private static Image CreateImage(string id, string animalId, bool isPrincipal = false)
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

    /// <summary>
    /// Setups the mock image service to return successful deletion.
    /// </summary>
    private void SetupSuccessfulDeletion()
    {
        _mockImageService
            .Setup(s => s.DeleteImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 204));
    }

    /// <summary>
    /// Setups the mock image service to return failure.
    /// </summary>
    private void SetupFailedDeletion(string errorMessage, int statusCode = 500)
    {
        _mockImageService
            .Setup(s => s.DeleteImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure(errorMessage, statusCode));
    }

    #endregion

    #region Success Cases

    /// <summary>
    /// Tests successful deletion of a non-principal image.
    /// Equivalence Class: Valid animal, valid non-principal image, image belongs to animal.
    /// </summary>
    [Fact]
    public async Task Handle_ValidNonPrincipalImage_ReturnsSuccess()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.Code);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            _dbContext,
            animalId,
            image.PublicId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests deletion with different valid ID formats.
    /// Equivalence Class: Various valid ID formats.
    /// </summary>
    [Theory]
    [InlineData("animal-123", "image-456")]
    [InlineData("animal_with_underscore", "image_with_underscore")]
    [InlineData("ANIMAL-UPPERCASE", "IMAGE-UPPERCASE")]
    public async Task Handle_VariousIdFormats_DeletesSuccessfully(string animalId, string imageId)
    {
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            _dbContext,
            animalId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests deletion with GUID-formatted IDs.
    /// Common real-world scenario.
    /// </summary>
    [Fact]
    public async Task Handle_GuidFormattedIds_DeletesSuccessfully()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Failure Cases - Animal Not Found

    /// <summary>
    /// Tests failure when animal doesn't exist.
    /// Equivalence Class: Non-existent animal ID.
    /// </summary>
    [Fact]
    public async Task Handle_AnimalNotFound_ReturnsFailure()
    {
        var nonExistentAnimalId = "animal-999";
        var imageId = "image-1";

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = nonExistentAnimalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests with various invalid animal IDs.
    /// Equivalence Class: Invalid or non-existent animal IDs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-animal")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_InvalidAnimalId_ReturnsNotFound(string invalidAnimalId)
    {
        var imageId = "image-1";

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = invalidAnimalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases - Image Not Found

    /// <summary>
    /// Tests failure when image doesn't exist.
    /// Equivalence Class: Valid animal, non-existent image ID.
    /// </summary>
    [Fact]
    public async Task Handle_ImageNotFound_ReturnsFailure()
    {
        var animalId = "animal-1";
        var nonExistentImageId = "image-999";
        
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = nonExistentImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Image not found", result.Error);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests with various invalid image IDs.
    /// Equivalence Class: Valid animal, invalid image IDs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-image")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_InvalidImageId_ReturnsNotFound(string invalidImageId)
    {
        var animalId = "animal-1";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = invalidImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases - Principal Image Protection

    /// <summary>
    /// Tests failure when attempting to delete a principal image.
    /// Equivalence Class: Valid animal and image, but image is principal.
    /// Business rule: Cannot delete principal images.
    /// </summary>
    [Fact]
    public async Task Handle_PrincipalImage_ReturnsFailure()
    {
        var animalId = "animal-1";
        var imageId = "image-principal";
        
        var animal = CreateAnimal(animalId);
        var principalImage = CreateImage(imageId, animalId, isPrincipal: true);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(principalImage);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Cannot delete Animal's main image", result.Error);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that only principal images are protected, not non-principal ones.
    /// Verifies correct principal flag checking.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_PrincipalFlag_CheckedCorrectly(bool isPrincipal, bool shouldSucceed)
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: isPrincipal);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        if (shouldSucceed)
        {
            SetupSuccessfulDeletion();
        }

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(shouldSucceed, result.IsSuccess);
    }

    #endregion

    #region Failure Cases - Image Ownership Validation

    /// <summary>
    /// Tests failure when image belongs to a different animal.
    /// Equivalence Class: Valid animal and image, but image.AnimalId != animal.Id.
    /// Security check: Image must belong to specified animal.
    /// </summary>
    [Fact]
    public async Task Handle_ImageBelongsToDifferentAnimal_ReturnsFailure()
    {
        var animal1Id = "animal-1";
        var animal2Id = "animal-2";
        var imageId = "image-1";
        
        var animal1 = CreateAnimal(animal1Id);
        var animal2 = CreateAnimal(animal2Id);
        var imageOfAnimal2 = CreateImage(imageId, animal2Id, isPrincipal: false);
        
        _dbContext.Animals.AddRange(animal1, animal2);
        _dbContext.Images.Add(imageOfAnimal2);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal1Id,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Equal("Image does not belong to the specified animal.", result.Error);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that ownership validation uses correct animal ID.
    /// Verifies the security check is properly implemented.
    /// </summary>
    [Fact]
    public async Task Handle_CorrectOwnership_AllowsDeletion()
    {
        var animalId = "animal-correct";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests ownership validation with image having null AnimalId.
    /// Edge case: Orphaned image.
    /// </summary>
    [Fact]
    public async Task Handle_ImageWithNullAnimalId_ReturnsFailure()
    {
        var animalId = "animal-1";
        var imageId = "image-orphan";
        
        var animal = CreateAnimal(animalId);
        var orphanImage = new Image
        {
            Id = imageId,
            PublicId = $"public-{imageId}",
            Url = "https://cloudinary.com/orphan.jpg",
            Description = "Orphan image",
            IsPrincipal = false,
            AnimalId = null
        };
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(orphanImage);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases - Image Service Errors

    /// <summary>
    /// Tests failure when image service fails to delete.
    /// Equivalence Class: Valid request, but service layer fails.
    /// </summary>
    [Fact]
    public async Task Handle_ImageServiceFails_ReturnsFailure()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupFailedDeletion("Cloudinary deletion failed", 500);

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.Code);
        Assert.Equal("Cloudinary deletion failed", result.Error);
    }

    /// <summary>
    /// Tests various service error scenarios.
    /// Equivalence Classes: Different service error codes.
    /// </summary>
    [Theory]
    [InlineData(400, "Bad request")]
    [InlineData(500, "Internal server error")]
    [InlineData(503, "Service unavailable")]
    public async Task Handle_ImageServiceVariousErrors_PropagatesCorrectly(int errorCode, string errorMessage)
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupFailedDeletion(errorMessage, errorCode);

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorCode, result.Code);
        Assert.Equal(errorMessage, result.Error);
    }

    #endregion

    #region Validation Order Tests

    /// <summary>
    /// Tests that validations are performed in the correct order.
    /// Order: Animal exists -> Image exists -> Image is not principal -> Image belongs to animal -> Delete
    /// Ensures early validation failures don't trigger unnecessary operations.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationOrder_AnimalCheckedFirst()
    {
        var nonExistentAnimalId = "animal-999";
        var nonExistentImageId = "image-999";

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = nonExistentAnimalId,
            ImageId = nonExistentImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Animal not found", result.Error);
    }

    /// <summary>
    /// Tests that image existence is checked after animal existence.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationOrder_ImageCheckedAfterAnimal()
    {
        var animalId = "animal-1";
        var nonExistentImageId = "image-999";
        
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = nonExistentImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Image not found", result.Error);
    }

    /// <summary>
    /// Tests that principal check is performed before ownership check.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationOrder_PrincipalCheckedBeforeOwnership()
    {
        var animal1Id = "animal-1";
        var animal2Id = "animal-2";
        var imageId = "image-principal";
        
        var animal1 = CreateAnimal(animal1Id);
        var animal2 = CreateAnimal(animal2Id);
        var principalImageOfAnimal2 = CreateImage(imageId, animal2Id, isPrincipal: true);
        
        _dbContext.Animals.AddRange(animal1, animal2);
        _dbContext.Images.Add(principalImageOfAnimal2);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal1Id,
            ImageId = imageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Cannot delete Animal's main image", result.Error);
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that cancellation token is passed through the call chain.
    /// Verifies proper async cancellation support.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesToServices()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = CreateImage(imageId, animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        using var cts = new CancellationTokenSource();

        await _handler.Handle(command, cts.Token);

        _mockImageService.Verify(s => s.DeleteImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cts.Token), Times.Once);
    }

    #endregion

    #region PublicId Propagation Tests

    /// <summary>
    /// Tests that correct PublicId is passed to the image service.
    /// Verifies the handler extracts and passes the right identifier.
    /// </summary>
    [Fact]
    public async Task Handle_PassesCorrectPublicId_ToImageService()
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        var expectedPublicId = "public-image-1";
        
        var animal = CreateAnimal(animalId);
        var image = new Image
        {
            Id = imageId,
            PublicId = expectedPublicId,
            Url = "https://cloudinary.com/test.jpg",
            Description = "Test",
            IsPrincipal = false,
            AnimalId = animalId
        };
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.DeleteImageAsync(
            _dbContext,
            animalId,
            expectedPublicId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests deletion with various PublicId formats.
    /// Ensures PublicId is correctly handled regardless of format.
    /// </summary>
    [Theory]
    [InlineData("simple-public-id")]
    [InlineData("folder/subfolder/image-id")]
    [InlineData("SeePaw/Animals/animal-123")]
    public async Task Handle_VariousPublicIdFormats_PassedCorrectly(string publicId)
    {
        var animalId = "animal-1";
        var imageId = "image-1";
        
        var animal = CreateAnimal(animalId);
        var image = new Image
        {
            Id = imageId,
            PublicId = publicId,
            Url = "https://cloudinary.com/test.jpg",
            Description = "Test",
            IsPrincipal = false,
            AnimalId = animalId
        };
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.DeleteImageAsync(
            _dbContext,
            animalId,
            publicId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests realistic scenario with multiple images where one is deleted.
    /// Integration test covering typical use case.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticScenario_DeleteOneOfMultipleImages()
    {
        var animalId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(animalId);
        
        var principalImage = CreateImage("img-principal", animalId, isPrincipal: true);
        var secondaryImage1 = CreateImage("img-secondary-1", animalId, isPrincipal: false);
        var secondaryImage2 = CreateImage("img-secondary-2", animalId, isPrincipal: false);
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.AddRange(principalImage, secondaryImage1, secondaryImage2);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = "img-secondary-1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageService.Verify(s => s.DeleteImageAsync(
            _dbContext,
            animalId,
            "public-img-secondary-1",
            It.IsAny<CancellationToken>()), Times.Once);
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
        
        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = longAnimalId,
            ImageId = longImageId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion
}