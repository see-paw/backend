using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Images.Commands;

/// <summary>
/// Unit tests for AddImagesAnimal.Handler using equivalence partitioning and boundary value analysis.
/// Tests the Handle method which processes multiple image uploads for an animal.
/// </summary>
public class AddImagesAnimalTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImageManager<Animal>> _mockImageService;
    private readonly AddImagesAnimal.Handler _handler;

    public AddImagesAnimalTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mockImageService = new Mock<IImageManager<Animal>>();
        _handler = new AddImagesAnimal.Handler(_dbContext, _mockImageService.Object);
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
    /// Creates a mock IFormFile for testing.
    /// </summary>
    private static Mock<IFormFile> CreateMockFormFile(string fileName = "test.jpg", long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        return mockFile;
    }

    /// <summary>
    /// Creates image metadata for testing.
    /// </summary>
    private static Image CreateImageMetadata(string? description = "Test image", bool isPrincipal = false)
    {
        return new Image
        {
            Id = Guid.NewGuid().ToString(),
            PublicId = $"public-{Guid.NewGuid()}",
            Url = "https://cloudinary.com/temp.jpg",
            Description = description,
            IsPrincipal = isPrincipal
        };
    }

    /// <summary>
    /// Setups the mock image service to return successful results.
    /// </summary>
    private void SetupSuccessfulImageService(Image returnImage)
    {
        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Image>.Success(returnImage, 201));
    }

    /// <summary>
    /// Setups the mock image service to return failure results.
    /// </summary>
    private void SetupFailedImageService(string errorMessage, int statusCode = 400)
    {
        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Image>.Failure(errorMessage, statusCode));
    }

    #endregion

    #region Success Cases

    /// <summary>
    /// Tests successful upload of single image.
    /// Equivalence Class: Valid request with 1 file and 1 metadata.
    /// Boundary: Minimum valid collection size.
    /// </summary>
    [Fact]
    public async Task Handle_SingleImageUpload_ReturnsSuccess()
    {
        var animalId = "animal-1";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile("image1.jpg").Object;
        var imageMeta = CreateImageMetadata("First image", isPrincipal: true);
        var uploadedImage = CreateImageMetadata("First image", isPrincipal: true);

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            file,
            "First image",
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests successful upload of multiple images.
    /// Equivalence Class: Valid request with N files and N metadata (N > 1).
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Handle_MultipleImagesUpload_ReturnsSuccess(int imageCount)
    {
        var animalId = "animal-multi";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = new List<IFormFile>();
        var imageMetadata = new List<Image>();

        for (int i = 0; i < imageCount; i++)
        {
            files.Add(CreateMockFormFile($"image{i}.jpg").Object);
            imageMetadata.Add(CreateImageMetadata($"Image {i}", isPrincipal: i == 0));
        }

        var uploadedImage = CreateImageMetadata();
        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
        Assert.NotNull(result.Value);
        Assert.Equal(imageCount, result.Value.Count);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(imageCount));
    }

    /// <summary>
    /// Tests upload with null description (uses empty string).
    /// Equivalence Class: Image metadata with null optional field.
    /// </summary>
    [Fact]
    public async Task Handle_ImageWithNullDescription_UsesEmptyString()
    {
        var animalId = "animal-null-desc";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile().Object;
        var imageMeta = new Image
        {
            Id = Guid.NewGuid().ToString(),
            PublicId = $"public-{Guid.NewGuid()}",
            Url = "https://cloudinary.com/temp.jpg",
            Description = null,
            IsPrincipal = false
        };
        var uploadedImage = CreateImageMetadata();

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            file,
            string.Empty,
            false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests upload with various IsPrincipal states.
    /// Equivalence Classes: IsPrincipal = true and false.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_VariousPrincipalStates_PassedCorrectly(bool isPrincipal)
    {
        var animalId = "animal-principal";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata(isPrincipal: isPrincipal);
        var uploadedImage = CreateImageMetadata(isPrincipal: isPrincipal);

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            file,
            It.IsAny<string>(),
            isPrincipal,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Failure Cases - Validation Errors

    /// <summary>
    /// Tests failure when file count doesn't match metadata count.
    /// Equivalence Class: Mismatched collection sizes.
    /// </summary>
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 5)]
    [InlineData(5, 3)]
    public async Task Handle_MismatchedFileAndMetadataCount_ReturnsFailure(int fileCount, int metadataCount)
    {
        var animalId = "animal-mismatch";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = Enumerable.Range(0, fileCount)
            .Select(i => CreateMockFormFile($"file{i}.jpg").Object)
            .ToList();

        var imageMetadata = Enumerable.Range(0, metadataCount)
            .Select(i => CreateImageMetadata($"Image {i}"))
            .ToList();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Mismatch between files and image metadata", result.Error);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests failure when both collections are empty.
    /// Boundary: Zero elements in both collections.
    /// Edge case: Empty but equal count.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyCollections_ReturnsSuccess()
    {
        var animalId = "animal-empty";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile>(),
            Images = new List<Image>()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases - Animal Not Found

    /// <summary>
    /// Tests failure when animal doesn't exist.
    /// Equivalence Class: Non-existent entity ID.
    /// </summary>
    [Fact]
    public async Task Handle_AnimalNotFound_ReturnsFailure()
    {
        var nonExistentId = "animal-999";
        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = nonExistentId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests with various invalid animal IDs.
    /// Equivalence Class: Invalid or non-existent IDs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-id")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_InvalidAnimalId_ReturnsNotFound(string invalidId)
    {
        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = invalidId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Cases - Image Service Errors

    /// <summary>
    /// Tests failure when image service fails on first image.
    /// Equivalence Class: Image service returns failure on first attempt.
    /// </summary>
    [Fact]
    public async Task Handle_ImageServiceFailsOnFirst_ReturnsFailureImmediately()
    {
        var animalId = "animal-service-fail";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = new List<IFormFile>
        {
            CreateMockFormFile("img1.jpg").Object,
            CreateMockFormFile("img2.jpg").Object,
            CreateMockFormFile("img3.jpg").Object
        };

        var imageMetadata = new List<Image>
        {
            CreateImageMetadata("Image 1"),
            CreateImageMetadata("Image 2"),
            CreateImageMetadata("Image 3")
        };

        SetupFailedImageService("Upload failed", 400);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Image upload failed", result.Error);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests failure when image service succeeds partially then fails.
    /// Equivalence Class: Sequential operations with failure after N successes.
    /// </summary>
    [Fact]
    public async Task Handle_ImageServiceFailsOnSecond_StopsProcessing()
    {
        var animalId = "animal-partial-fail";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = new List<IFormFile>
        {
            CreateMockFormFile("img1.jpg").Object,
            CreateMockFormFile("img2.jpg").Object
        };

        var imageMetadata = new List<Image>
        {
            CreateImageMetadata("Image 1"),
            CreateImageMetadata("Image 2")
        };

        var callCount = 0;
        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? Result<Image>.Success(CreateImageMetadata(), 201)
                    : Result<Image>.Failure("Second upload failed", 400);
            });

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Tests failure when image service returns null value.
    /// Equivalence Class: Successful result but null value.
    /// Edge case: Unexpected null in successful result.
    /// </summary>
    [Fact]
    public async Task Handle_ImageServiceReturnsNullValue_ReturnsFailure()
    {
        var animalId = "animal-null-value";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata();

        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Image>.Success(null, 201));

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.Code);
        Assert.Contains("null value", result.Error);
    }

    #endregion

    #region Boundary Value Analysis

    /// <summary>
    /// Tests with large number of images.
    /// Boundary: Maximum practical batch size.
    /// </summary>
    [Theory]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Handle_LargeNumberOfImages_HandlesCorrectly(int imageCount)
    {
        var animalId = "animal-large-batch";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = Enumerable.Range(0, imageCount)
            .Select(i => CreateMockFormFile($"image{i}.jpg").Object)
            .ToList();

        var imageMetadata = Enumerable.Range(0, imageCount)
            .Select(i => CreateImageMetadata($"Image {i}"))
            .ToList();

        var uploadedImage = CreateImageMetadata();
        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(imageCount, result.Value!.Count);
    }

    /// <summary>
    /// Tests with very long description.
    /// Boundary: Maximum description length.
    /// </summary>
    [Fact]
    public async Task Handle_VeryLongDescription_HandlesCorrectly()
    {
        var animalId = "animal-long-desc";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var longDescription = new string('A', 1000);
        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata(longDescription);
        var uploadedImage = CreateImageMetadata(longDescription);

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            file,
            longDescription,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Cancellation Tests

    /// <summary>
    /// Tests that cancellation token is passed to underlying services.
    /// Verifies proper async cancellation support.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesToServices()
    {
        var animalId = "animal-cancel";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata();
        var uploadedImage = CreateImageMetadata();

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        using var cts = new CancellationTokenSource();

        await _handler.Handle(command, cts.Token);

        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            cts.Token), Times.Once);
    }

    #endregion

    #region Order and Sequence Tests

    /// <summary>
    /// Tests that images are processed in the order they are provided.
    /// Verifies sequential processing behavior.
    /// </summary>
    [Fact]
    public async Task Handle_ProcessesImagesInOrder_MaintainsSequence()
    {
        var animalId = "animal-order";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var processedDescriptions = new List<string>();

        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Callback<AppDbContext, string, IFormFile, string, bool, CancellationToken>(
                (_, _, _, desc, _, _) => processedDescriptions.Add(desc))
            .ReturnsAsync((AppDbContext _, string __, IFormFile ___, string desc, bool ____, CancellationToken _____) =>
                Result<Image>.Success(CreateImageMetadata(desc), 201));

        var files = new List<IFormFile>
        {
            CreateMockFormFile("first.jpg").Object,
            CreateMockFormFile("second.jpg").Object,
            CreateMockFormFile("third.jpg").Object
        };

        var imageMetadata = new List<Image>
        {
            CreateImageMetadata("First"),
            CreateImageMetadata("Second"),
            CreateImageMetadata("Third")
        };

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(new[] { "First", "Second", "Third" }, processedDescriptions);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests realistic scenario with mixed principal states.
    /// Integration test covering typical multi-image upload.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticScenario_MultipleImagesWithOnePrincipal()
    {
        var animalId = Guid.NewGuid().ToString();
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = new List<IFormFile>
        {
            CreateMockFormFile("principal.jpg", 2048).Object,
            CreateMockFormFile("secondary1.jpg", 1024).Object,
            CreateMockFormFile("secondary2.jpg", 1536).Object
        };

        var imageMetadata = new List<Image>
        {
            CreateImageMetadata("Main photo", isPrincipal: true),
            CreateImageMetadata("Side view", isPrincipal: false),
            CreateImageMetadata("Playing", isPrincipal: false)
        };

        var uploadedImages = imageMetadata.Select(m => CreateImageMetadata(m.Description, m.IsPrincipal)).ToList();
        var callIndex = 0;

        _mockImageService
            .Setup(s => s.AddImageAsync(
                It.IsAny<AppDbContext>(),
                It.IsAny<string>(),
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Result<Image>.Success(uploadedImages[callIndex++], 201));

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests with all images marked as principal.
    /// Edge case: Invalid business rule but should be handled by service.
    /// </summary>
    [Fact]
    public async Task Handle_AllImagesMarkedAsPrincipal_ServiceHandles()
    {
        var animalId = "animal-all-principal";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var files = Enumerable.Range(0, 3)
            .Select(i => CreateMockFormFile($"img{i}.jpg").Object)
            .ToList();

        var imageMetadata = Enumerable.Range(0, 3)
            .Select(i => CreateImageMetadata($"Image {i}", isPrincipal: true))
            .ToList();

        var uploadedImage = CreateImageMetadata(isPrincipal: true);
        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = files,
            Images = imageMetadata
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            true,
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    /// <summary>
    /// Tests with empty string descriptions.
    /// Edge case: Empty string vs null description.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyStringDescriptions_PassedAsIs()
    {
        var animalId = "animal-empty-desc";
        var animal = CreateAnimal(animalId);
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        var file = CreateMockFormFile().Object;
        var imageMeta = CreateImageMetadata(string.Empty);
        var uploadedImage = CreateImageMetadata(string.Empty);

        SetupSuccessfulImageService(uploadedImage);

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { imageMeta }
        };

        await _handler.Handle(command, CancellationToken.None);

        _mockImageService.Verify(s => s.AddImageAsync(
            _dbContext,
            animalId,
            file,
            string.Empty,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}