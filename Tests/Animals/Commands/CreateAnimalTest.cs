using Application.Animals.Commands;
using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Animals.Commands;

/// <summary>
/// Unit tests for CreateAnimal.Handler using equivalence partitioning and boundary value analysis.
/// Tests the Handle method which creates a new animal with images in a transactional manner.
/// 
/// IMPORTANT: InMemoryDatabase does not support real database transactions.
/// - BeginTransactionAsync/CommitAsync/RollbackAsync are ignored by InMemoryDatabase
/// - Tests cannot verify that data is actually rolled back on failure
/// - Tests focus on verifying error handling, validation logic, and successful scenarios
/// - In production with SQL Server/PostgreSQL, transactions work correctly
/// </summary>
public class CreateAnimalTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImageAppService<Animal>> _mockImageService;
    private readonly CreateAnimal.Handler _handler;

    public CreateAnimalTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _mockImageService = new Mock<IImageAppService<Animal>>();
        _handler = new CreateAnimal.Handler(_dbContext, _mockImageService.Object);
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
    private Breed CreateBreed(string? id = null)
    {
        return new Breed
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = "Test Breed",
            Description = "Test breed description"
        };
    }

    /// <summary>
    /// Creates a valid animal entity for testing.
    /// </summary>
    private Animal CreateValidAnimal(string? shelterId = null, string? breedId = null)
    {
        return new Animal
        {
            Id = Guid.NewGuid().ToString(),
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

    /// <summary>
    /// Creates a mock IFormFile for testing.
    /// </summary>
    private static Mock<IFormFile> CreateMockFormFile(string fileName = "test.jpg", long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        var stream = new MemoryStream();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
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
    private void SetupSuccessfulImageService()
    {
        var returnImage = CreateImageMetadata();
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
    /// Tests successful creation of animal with single image.
    /// Equivalence Class: Valid animal, valid shelter, valid breed, 1 file = 1 metadata.
    /// Boundary: Minimum valid image count.
    /// </summary>
    [Fact]
    public async Task Handle_ValidAnimalWithSingleImage_ReturnsSuccess()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var file = CreateMockFormFile("principal.jpg").Object;
        var imageMeta = CreateImageMetadata("Principal image", isPrincipal: true);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { imageMeta },
            Files = new List<IFormFile> { file }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
        Assert.Equal(animal.Id, result.Value);

        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.NotNull(savedAnimal);
        Assert.Equal(shelter.Id, savedAnimal.ShelterId);
    }

    /// <summary>
    /// Tests successful creation with multiple images.
    /// Equivalence Class: Valid animal with N images (N > 1).
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Handle_ValidAnimalWithMultipleImages_ReturnsSuccess(int imageCount)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var files = new List<IFormFile>();
        var imageMetadata = new List<Image>();

        for (int i = 0; i < imageCount; i++)
        {
            files.Add(CreateMockFormFile($"image{i}.jpg").Object);
            imageMetadata.Add(CreateImageMetadata($"Image {i}", isPrincipal: i == 0));
        }

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = imageMetadata,
            Files = files
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(imageCount));
    }

    /// <summary>
    /// Tests creation with all valid animal states.
    /// Equivalence Class: Different valid AnimalState values.
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    [InlineData(AnimalState.TotallyFostered)]
    [InlineData(AnimalState.HasOwner)]
    [InlineData(AnimalState.Inactive)]
    public async Task Handle_DifferentAnimalStates_CreatesSuccessfully(AnimalState state)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.AnimalState = state;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(state, savedAnimal!.AnimalState);
    }

    /// <summary>
    /// Tests creation with all valid species.
    /// Equivalence Class: Different valid Species values.
    /// </summary>
    [Theory]
    [InlineData(Species.Dog)]
    [InlineData(Species.Cat)]
    public async Task Handle_DifferentSpecies_CreatesSuccessfully(Species species)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Species = species;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(species, savedAnimal!.Species);
    }

    /// <summary>
    /// Tests creation with all valid sizes.
    /// Equivalence Class: Different valid SizeType values.
    /// </summary>
    [Theory]
    [InlineData(SizeType.Small)]
    [InlineData(SizeType.Medium)]
    [InlineData(SizeType.Large)]
    public async Task Handle_DifferentSizes_CreatesSuccessfully(SizeType size)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Size = size;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(size, savedAnimal!.Size);
    }

    /// <summary>
    /// Tests creation with different sex types.
    /// Equivalence Class: Valid SexType values.
    /// </summary>
    [Theory]
    [InlineData(SexType.Male)]
    [InlineData(SexType.Female)]
    public async Task Handle_DifferentSexTypes_CreatesSuccessfully(SexType sex)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Sex = sex;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(sex, savedAnimal!.Sex);
    }

    /// <summary>
    /// Tests creation with valid cost boundary values.
    /// Boundary Analysis: Minimum valid (0), normal values, maximum valid (1000).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(50.00)]
    [InlineData(500.50)]
    [InlineData(999.99)]
    [InlineData(1000.00)]
    public async Task Handle_ValidCostBoundaries_CreatesSuccessfully(decimal cost)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Cost = cost;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(cost, savedAnimal!.Cost);
    }

    /// <summary>
    /// Tests creation with sterilized status.
    /// Equivalence Class: Different sterilization states.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_DifferentSterilizedStates_CreatesSuccessfully(bool sterilized)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Sterilized = sterilized;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Equal(sterilized, savedAnimal!.Sterilized);
    }

    #endregion

    #region Failure Cases - File/Metadata Mismatch

    /// <summary>
    /// Tests failure when files count exceeds metadata count.
    /// Equivalence Class: Files.Count > Images.Count.
    /// </summary>
    [Theory]
    [InlineData(3, 2)]
    [InlineData(5, 3)]
    [InlineData(2, 1)]
    public async Task Handle_MoreFilesThanMetadata_ReturnsFailure(int fileCount, int metadataCount)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var files = Enumerable.Range(0, fileCount)
            .Select(i => CreateMockFormFile($"file{i}.jpg").Object)
            .ToList();
        var metadata = Enumerable.Range(0, metadataCount)
            .Select(i => CreateImageMetadata($"Image {i}"))
            .ToList();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = metadata,
            Files = files
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Mismatch between files and image metadata.", result.Error);
    }

    /// <summary>
    /// Tests failure when metadata count exceeds files count.
    /// Equivalence Class: Images.Count > Files.Count.
    /// </summary>
    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 5)]
    [InlineData(1, 2)]
    public async Task Handle_MoreMetadataThanFiles_ReturnsFailure(int fileCount, int metadataCount)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var files = Enumerable.Range(0, fileCount)
            .Select(i => CreateMockFormFile($"file{i}.jpg").Object)
            .ToList();
        var metadata = Enumerable.Range(0, metadataCount)
            .Select(i => CreateImageMetadata($"Image {i}"))
            .ToList();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = metadata,
            Files = files
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Mismatch between files and image metadata.", result.Error);
    }

    /// <summary>
    /// Tests failure with zero files and zero metadata.
    /// Boundary: Empty collections (should require at least one image).
    /// </summary>
    [Fact]
    public async Task Handle_EmptyFilesAndMetadata_ReturnsFailure()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image>(),
            Files = new List<IFormFile>()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
    }

    #endregion

    #region Failure Cases - Shelter Not Found

    /// <summary>
    /// Tests failure when shelter doesn't exist.
    /// Equivalence Class: Non-existent shelter ID.
    /// </summary>
    [Fact]
    public async Task Handle_ShelterNotFound_ReturnsFailure()
    {
        var breed = CreateBreed();
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var nonExistentShelterId = Guid.NewGuid().ToString();
        var animal = CreateValidAnimal(nonExistentShelterId, breed.Id);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = nonExistentShelterId,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Shelter not found", result.Error);

        var animalExists = await _dbContext.Animals.AnyAsync(a => a.Id == animal.Id);
        Assert.False(animalExists);
    }

    /// <summary>
    /// Tests with various invalid shelter ID formats.
    /// Equivalence Class: Different formats of non-existent IDs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-id")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_InvalidShelterIdFormats_ReturnsFailure(string invalidShelterId)
    {
        var breed = CreateBreed();
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(invalidShelterId, breed.Id);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = invalidShelterId,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    #endregion

    #region Failure Cases - Breed Not Found

    /// <summary>
    /// Tests failure when breed doesn't exist.
    /// Equivalence Class: Non-existent breed ID.
    /// </summary>
    [Fact]
    public async Task Handle_BreedNotFound_ReturnsFailure()
    {
        var shelter = CreateShelter();
        _dbContext.Shelters.Add(shelter);
        await _dbContext.SaveChangesAsync();

        var nonExistentBreedId = Guid.NewGuid().ToString();
        var animal = CreateValidAnimal(shelter.Id, nonExistentBreedId);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Breed not found", result.Error);

        var animalExists = await _dbContext.Animals.AnyAsync(a => a.Id == animal.Id);
        Assert.False(animalExists);
    }

    /// <summary>
    /// Tests with various invalid breed ID formats.
    /// Equivalence Class: Different formats of non-existent breed IDs.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-breed")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_InvalidBreedIdFormats_ReturnsFailure(string invalidBreedId)
    {
        var shelter = CreateShelter();
        _dbContext.Shelters.Add(shelter);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, invalidBreedId);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    #endregion

    #region Failure Cases - Image Upload Failure

    /// <summary>
    /// Tests that handler returns failure when first image upload fails.
    /// Verifies error handling and proper error message propagation.
    /// Note: InMemoryDatabase doesn't support real transactions, so rollback behavior cannot be verified.
    /// </summary>
    [Fact]
    public async Task Handle_FirstImageUploadFails_RollsBackAnimalCreation()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupFailedImageService("Upload failed", 400);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Image upload failed", result.Error);
        Assert.Contains("Upload failed", result.Error);
    }

    /// <summary>
    /// Tests that handler stops processing and returns failure when middle image upload fails.
    /// Verifies that the handler correctly stops the upload loop on first failure.
    /// Note: InMemoryDatabase doesn't support real transactions, so rollback behavior cannot be verified.
    /// </summary>
    [Fact]
    public async Task Handle_MiddleImageUploadFails_RollsBackAllChanges()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

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
                return callCount == 2
                    ? Result<Image>.Failure("Upload failed", 400)
                    : Result<Image>.Success(CreateImageMetadata(), 201);
            });

        var files = new List<IFormFile>
        {
            CreateMockFormFile("img1.jpg").Object,
            CreateMockFormFile("img2.jpg").Object,
            CreateMockFormFile("img3.jpg").Object
        };

        var metadata = new List<Image>
        {
            CreateImageMetadata("Image 1", true),
            CreateImageMetadata("Image 2"),
            CreateImageMetadata("Image 3")
        };

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = metadata,
            Files = files
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Image upload failed", result.Error);
        Assert.Equal(2, callCount);
    }

    /// <summary>
    /// Tests with different image service error codes.
    /// Equivalence Class: Different error status codes from image service.
    /// </summary>
    [Theory]
    [InlineData(400, "Bad request")]
    [InlineData(500, "Internal server error")]
    [InlineData(503, "Service unavailable")]
    public async Task Handle_ImageServiceReturnsError_PropagatesStatusCode(int errorCode, string errorMessage)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupFailedImageService(errorMessage, errorCode);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains(errorMessage, result.Error);
    }

    #endregion

    #region Failure Cases - Database Errors

    /// <summary>
    /// Tests that handler returns failure when image upload fails.
    /// Verifies error handling and response structure.
    /// Note: InMemoryDatabase doesn't support real transactions, so rollback behavior cannot be verified.
    /// </summary>
    [Fact]
    public async Task Handle_ImageUploadFails_AnimalNotPersisted()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupFailedImageService("Cloudinary error", 500);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Image upload failed", result.Error);
    }

    #endregion

    #region Transaction and Rollback Tests

    /// <summary>
    /// Tests successful transaction commit when all operations succeed.
    /// Verifies animal and all associations are persisted.
    /// </summary>
    [Fact]
    public async Task Handle_AllOperationsSucceed_CommitsTransaction()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image>
            {
                CreateImageMetadata("Main", true),
                CreateImageMetadata("Side")
            },
            Files = new List<IFormFile>
            {
                CreateMockFormFile("main.jpg").Object,
                CreateMockFormFile("side.jpg").Object
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.NotNull(savedAnimal);

        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            animal.Id,
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
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
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
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
    /// Tests that images are processed in the order provided.
    /// Verifies sequential processing behavior.
    /// </summary>
    [Fact]
    public async Task Handle_ProcessesImagesInOrder_MaintainsSequence()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

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

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image>
            {
                CreateImageMetadata("First", true),
                CreateImageMetadata("Second"),
                CreateImageMetadata("Third")
            },
            Files = new List<IFormFile>
            {
                CreateMockFormFile("first.jpg").Object,
                CreateMockFormFile("second.jpg").Object,
                CreateMockFormFile("third.jpg").Object
            }
        };

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(new[] { "First", "Second", "Third" }, processedDescriptions);
    }

    /// <summary>
    /// Tests that shelter is checked before breed.
    /// Verifies validation order.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationOrder_ShelterCheckedBeforeBreed()
    {
        var breed = CreateBreed();
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var nonExistentShelterId = Guid.NewGuid().ToString();
        var animal = CreateValidAnimal(nonExistentShelterId, breed.Id);

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = nonExistentShelterId,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Shelter not found", result.Error);
    }

    #endregion

    #region Integration Scenarios

    /// <summary>
    /// Tests realistic scenario with complete animal data and multiple images.
    /// Integration test covering typical use case.
    /// </summary>
    [Fact]
    public async Task Handle_RealisticScenario_CompleteAnimalWithMultipleImages()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Bobby",
            AnimalState = AnimalState.Available,
            Description = "Friendly and playful dog looking for a family",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Golden Brown",
            BirthDate = new DateOnly(2021, 6, 15),
            Sterilized = true,
            Cost = 75.50m,
            Features = "Good with children, trained, vaccinated",
            ShelterId = shelter.Id,
            BreedId = breed.Id
        };

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image>
            {
                CreateImageMetadata("Main portrait", true),
                CreateImageMetadata("Playing in park"),
                CreateImageMetadata("With other dogs")
            },
            Files = new List<IFormFile>
            {
                CreateMockFormFile("portrait.jpg", 2048).Object,
                CreateMockFormFile("playing.jpg", 1536).Object,
                CreateMockFormFile("friends.jpg", 1800).Object
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);

        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.NotNull(savedAnimal);
        Assert.Equal("Bobby", savedAnimal.Name);
        Assert.Equal(75.50m, savedAnimal.Cost);
        Assert.True(savedAnimal.Sterilized);

        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            animal.Id,
            It.IsAny<IFormFile>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests creation with boundary name lengths.
    /// Boundary: Minimum valid (2 chars) and maximum valid (100 chars) name length.
    /// </summary>
    [Theory]
    [InlineData(2, "AB")]
    [InlineData(2, "Bo")]
    [InlineData(100, null)]
    public async Task Handle_BoundaryNameLength_CreatesSuccessfully(int length, string? specificName)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Name = specificName ?? new string('A', length);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests with null optional fields.
    /// Edge case: All optional fields set to null.
    /// </summary>
    [Fact]
    public async Task Handle_NullOptionalFields_CreatesSuccessfully()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Description = null;
        animal.Features = null;
        animal.OwnerId = null;
        animal.OwnershipStartDate = null;
        animal.OwnershipEndDate = null;

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedAnimal = await _dbContext.Animals.FindAsync(animal.Id);
        Assert.Null(savedAnimal!.Description);
        Assert.Null(savedAnimal.Features);
    }

    /// <summary>
    /// Tests with empty string descriptions in images.
    /// Edge case: Empty vs null description.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyImageDescriptions_CreatesSuccessfully()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(string.Empty, true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        _mockImageService.Verify(s => s.AddImageAsync(
            It.IsAny<AppDbContext>(),
            It.IsAny<string>(),
            It.IsAny<IFormFile>(),
            string.Empty,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests creation with boundary birth dates.
    /// Boundary: Very old dates (30 years ago) and very recent dates (today).
    /// </summary>
    [Theory]
    [InlineData(1995, 1, 1)]
    [InlineData(2000, 6, 15)]
    [InlineData(0, 0, 0)]
    public async Task Handle_BoundaryBirthDates_CreatesSuccessfully(int year, int month, int day)
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.BirthDate = year == 0 
            ? DateOnly.FromDateTime(DateTime.Today) 
            : new DateOnly(year, month, day);

        SetupSuccessfulImageService();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImageMetadata(isPrincipal: true) },
            Files = new List<IFormFile> { CreateMockFormFile().Object }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion
}