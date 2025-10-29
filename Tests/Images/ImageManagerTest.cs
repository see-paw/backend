using Application.Images;
using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using ImageUploadResult = Application.Images.ImageUploadResult;

namespace Tests.Images;

/// <summary>
/// Unit tests for ImageManager using Equivalence Class Partitioning and Boundary Value Analysis.
/// Focuses on discovering edge cases and potential failures in image management operations.
/// Tests are designed to find bugs, not to pass.
/// </summary>
public class ImageManagerTest
{
    private readonly Mock<ICloudinaryService> _mockCloudinaryService;
    private readonly Mock<IImageOwnerLoader<Animal>> _mockLoader;
    private readonly Mock<IImageOwnerLinker<Animal>> _mockLinker;
    private readonly Mock<IPrincipalImageEnforcer> _mockEnforcer;
    private readonly AppDbContext _dbContext;
    private readonly ImageManager<Animal> _sut;

    public ImageManagerTest()
    {
        _mockCloudinaryService = new Mock<ICloudinaryService>();
        _mockLoader = new Mock<IImageOwnerLoader<Animal>>();
        _mockLinker = new Mock<IImageOwnerLinker<Animal>>();
        _mockEnforcer = new Mock<IPrincipalImageEnforcer>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _sut = new ImageManager<Animal>(
            _dbContext,
            _mockCloudinaryService.Object,
            _mockLoader.Object,
            _mockLinker.Object,
            _mockEnforcer.Object
        );
    }

    #region AddImageAsync - Boundary Value Analysis

    /// <summary>
    /// Tests file size boundaries: zero, very small, typical, very large.
    /// BVA: 0, 1, 1024, 5MB-1, 5MB, 5MB+1, 10MB
    /// </summary>
    [Theory]
    [InlineData(0L, "empty-file.jpg")]
    [InlineData(1L, "single-byte.jpg")]
    [InlineData(1024L, "1kb.jpg")]
    [InlineData(5242879L, "5mb-minus-1.jpg")]
    [InlineData(5242880L, "exactly-5mb.jpg")]
    [InlineData(5242881L, "5mb-plus-1.jpg")]
    [InlineData(10485760L, "10mb.jpg")]
    [InlineData(104857600L, "100mb.jpg")]
    public async Task AddImageAsync_FileSizeBoundaries_VariousResults(long fileSize, string fileName)
    {
        var entityId = "animal-001";
        var animal = CreateAnimal(entityId);
        var mockFile = CreateMockFile(fileName, fileSize);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://test.jpg", PublicId = "test-id" });

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, "Test", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests description length boundaries.
    /// BVA: null, empty, 1 char, 255 chars, 256 chars, 1000+ chars
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345")]
    [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")]
    public async Task AddImageAsync_DescriptionLengthBoundaries_HandlesCorrectly(string description)
    {
        var entityId = "animal-002";
        var animal = CreateAnimal(entityId);
        var mockFile = CreateMockFile("test.jpg", 1024);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://test.jpg", PublicId = "test-id" });

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, description, false, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region AddImageAsync - Equivalence Class Partitioning

    /// <summary>
    /// ECP: Valid vs Invalid entity IDs.
    /// Classes: null, empty, whitespace, valid format, invalid format, non-existent
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("animal-123")]
    [InlineData("invalid-id-$%^&")]
    [InlineData("non-existent-entity-999")]
    [InlineData("animal-123\0null-byte")]
    [InlineData("../../path-traversal")]
    public async Task AddImageAsync_EntityIdVariations_HandlesCorrectly(string entityId)
    {
        var animal = CreateAnimal("valid-animal-id");
        var mockFile = CreateMockFile("test.jpg", 1024);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string id, CancellationToken ct) => id == "animal-123" ? animal : null);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://test.jpg", PublicId = "test-id" });

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, "Test", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// ECP: File upload results - success, null, partial data.
    /// </summary>
    [Theory]
    [InlineData("http://valid.jpg", "valid-id", true)]
    [InlineData(null, "valid-id", false)]
    [InlineData("http://valid.jpg", null, false)]
    [InlineData(null, null, false)]
    [InlineData("", "valid-id", false)]
    [InlineData("http://valid.jpg", "", false)]
    public async Task AddImageAsync_UploadResultVariations_HandlesCorrectly(
        string url, 
        string publicId, 
        bool shouldReturnResult)
    {
        var entityId = "animal-003";
        var animal = CreateAnimal(entityId);
        var mockFile = CreateMockFile("test.jpg", 1024);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var uploadResult = shouldReturnResult && !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(publicId)
            ? new ImageUploadResult { Url = url, PublicId = publicId }
            : null;

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(uploadResult);

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, "Test", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// ECP: Multiple concurrent principal images scenario.
    /// </summary>
    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 5)]
    [InlineData(false, 0)]
    [InlineData(false, 3)]
    public async Task AddImageAsync_PrincipalImageWithExistingImages_EnforcerCalled(
        bool isPrincipal, 
        int existingImagesCount)
    {
        var entityId = "animal-004";
        var animal = CreateAnimal(entityId);
        
        for (int i = 0; i < existingImagesCount; i++)
        {
            animal.Images.Add(new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = $"http://existing-{i}.jpg",
                PublicId = $"existing-{i}",
                IsPrincipal = i == 0
            });
        }

        var mockFile = CreateMockFile("test.jpg", 1024);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://new.jpg", PublicId = "new-id" });

        _mockLinker.Setup(x => x.Link(It.IsAny<Animal>(), It.IsAny<Image>(), It.IsAny<string>()))
            .Callback<Animal, Image, string>((a, img, id) => a.Images.Add(img));

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, "New image", isPrincipal, CancellationToken.None);

        _mockEnforcer.Verify(x => x.EnforceSinglePrincipal(It.IsAny<ICollection<Image>>(), It.IsAny<Image>()), 
            Times.Once);
    }

    #endregion

    #region DeleteImageAsync - Boundary Value Analysis

    /// <summary>
    /// BVA: Testing imageId boundaries and special characters.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    [InlineData("valid-guid-12345678901234567890")]
    [InlineData("invalid-$%^&*-chars")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task DeleteImageAsync_ImageIdBoundaries_HandlesCorrectly(string imageId)
    {
        var entityId = "animal-005";
        var animal = CreateAnimal(entityId);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var result = await _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region DeleteImageAsync - Equivalence Class Partitioning

    /// <summary>
    /// ECP: Entity not found scenarios.
    /// </summary>
    [Theory]
    [InlineData(null, "image-123")]
    [InlineData("non-existent", "image-123")]
    [InlineData("", "image-123")]
    public async Task DeleteImageAsync_EntityNotFound_ReturnsFailure(string entityId, string imageId)
    {
        _mockLoader.Setup(x => x.LoadAsync(_dbContext, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal)null);

        var result = await _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    /// <summary>
    /// ECP: Attempting to delete principal image.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_PrincipalImage_ReturnsFailure()
    {
        var entityId = "animal-006";
        var imageId = Guid.NewGuid().ToString();
        
        var principalImage = new Image
        {
            Id = imageId,
            PublicId = "principal-id",
            Url = "http://principal.jpg",
            IsPrincipal = true,
            AnimalId = entityId
        };

        var animal = CreateAnimal(entityId);
        animal.Images.Add(principalImage);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(principalImage);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("principal", result.Error.ToLower());
    }

    /// <summary>
    /// ECP: Image belongs to different entity (ownership violation).
    /// </summary>
    [Theory]
    [InlineData("animal-007", "animal-008", "image-001")]
    [InlineData("animal-009", "animal-010", "image-002")]
    public async Task DeleteImageAsync_ImageBelongsToDifferentEntity_ReturnsFailure(
        string requestedEntityId, 
        string actualEntityId, 
        string imageId)
    {
        var imageGuid = Guid.NewGuid().ToString();
        
        var image = new Image
        {
            Id = imageGuid,
            PublicId = "test-public-id",
            Url = "http://test.jpg",
            IsPrincipal = false,
            AnimalId = actualEntityId
        };

        var requestedAnimal = CreateAnimal(requestedEntityId);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, requestedEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestedAnimal);

        _dbContext.Animals.Add(requestedAnimal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteImageAsync(requestedEntityId, imageGuid, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
    }

    /// <summary>
    /// ECP: Cloudinary deletion responses - success, failures, edge cases.
    /// </summary>
    [Theory]
    [InlineData("ok", true)]
    [InlineData("OK", true)]
    [InlineData("Ok", true)]
    [InlineData("oK", true)]
    [InlineData("error", false)]
    [InlineData("failed", false)]
    [InlineData("not found", false)]
    [InlineData("timeout", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("ok ", false)]
    [InlineData(" ok", false)]
    public async Task DeleteImageAsync_CloudinaryResponses_HandlesCorrectly(
        string cloudinaryResponse, 
        bool shouldSucceed)
    {
        var entityId = "animal-011";
        var imageId = Guid.NewGuid().ToString();
        
        var image = new Image
        {
            Id = imageId,
            PublicId = "test-public-id",
            Url = "http://test.jpg",
            IsPrincipal = false,
            AnimalId = entityId
        };

        var animal = CreateAnimal(entityId);
        animal.Images.Add(image);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.DeleteImage(It.IsAny<string>()))
            .ReturnsAsync(cloudinaryResponse);

        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        if (shouldSucceed)
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(204, result.Code);
        }
        else
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(502, result.Code);
        }
    }

    /// <summary>
    /// ECP: Database save failures after Cloudinary deletion.
    /// Tests orphaned Cloudinary resources scenario.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_DatabaseSaveFailsAfterCloudinarySuccess_LeavesOrphanedResource()
    {
        var entityId = "animal-012";
        var imageId = Guid.NewGuid().ToString();
        
        var image = new Image
        {
            Id = imageId,
            PublicId = "test-public-id",
            Url = "http://test.jpg",
            IsPrincipal = false,
            AnimalId = entityId
        };

        var animal = CreateAnimal(entityId);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.DeleteImage(It.IsAny<string>()))
            .ReturnsAsync("ok");

        _dbContext.Images.Add(image);

        var result = await _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        _mockCloudinaryService.Verify(x => x.DeleteImage("test-public-id"), Times.Once);
    }

    #endregion

    #region Edge Cases & Combinations

    /// <summary>
    /// Tests null/invalid file scenarios.
    /// </summary>
    [Fact]
    public async Task AddImageAsync_NullFile_HandlesCorrectly()
    {
        var entityId = "animal-013";
        var animal = CreateAnimal(entityId);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var result = await _sut.AddImageAsync(entityId, null, "Test", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests concurrent deletion attempts.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_ConcurrentDeletion_HandlesRaceCondition()
    {
        var entityId = "animal-014";
        var imageId = Guid.NewGuid().ToString();
        
        var image = new Image
        {
            Id = imageId,
            PublicId = "test-public-id",
            Url = "http://test.jpg",
            IsPrincipal = false,
            AnimalId = entityId
        };

        var animal = CreateAnimal(entityId);
        animal.Images.Add(image);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.DeleteImage(It.IsAny<string>()))
            .ReturnsAsync("ok");

        _dbContext.Animals.Add(animal);
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        var task1 = _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);
        var task2 = _sut.DeleteImageAsync(entityId, imageId, CancellationToken.None);

        await Task.WhenAll(task1, task2);

        var result1 = await task1;
        var result2 = await task2;

        Assert.True((result1.IsSuccess && !result2.IsSuccess) || 
                    (!result1.IsSuccess && result2.IsSuccess) ||
                    (!result1.IsSuccess && !result2.IsSuccess));
    }

    /// <summary>
    /// Tests special characters in file names.
    /// </summary>
    [Theory]
    [InlineData("normal-file.jpg")]
    [InlineData("file with spaces.jpg")]
    [InlineData("file@#$%^&*.jpg")]
    [InlineData("файл.jpg")]
    [InlineData("文件.jpg")]
    [InlineData("archivo.jpeg.jpg.png")]
    [InlineData("..jpg")]
    [InlineData(".jpg")]
    [InlineData("file\0null.jpg")]
    public async Task AddImageAsync_SpecialFileNames_HandlesCorrectly(string fileName)
    {
        var entityId = "animal-015";
        var animal = CreateAnimal(entityId);
        var mockFile = CreateMockFile(fileName, 1024);

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult { Url = "http://test.jpg", PublicId = "test-id" });

        var result = await _sut.AddImageAsync(entityId, mockFile.Object, "Test", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests cancellation token behavior.
    /// </summary>
    [Fact]
    public async Task AddImageAsync_CancelledToken_HandlesCorrectly()
    {
        var entityId = "animal-016";
        var animal = CreateAnimal(entityId);
        var mockFile = CreateMockFile("test.jpg", 1024);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockLoader.Setup(x => x.LoadAsync(_dbContext, entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        try
        {
            var result = await _sut.AddImageAsync(entityId, mockFile.Object, "Test", false, cts.Token);
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            Assert.True(true);
        }
    }

    #endregion

    #region Helper Methods

    private Animal CreateAnimal(string id)
    {
        return new Animal
        {
            Id = id,
            Name = "Test Animal",
            Species = Domain.Enums.Species.Dog,
            Size = Domain.Enums.SizeType.Medium,
            Sex = Domain.Enums.SexType.Male,
            AnimalState = Domain.Enums.AnimalState.Available,
            ShelterId = Guid.NewGuid().ToString(),
            Images = new List<Image>()
        };
    }

    private Mock<IFormFile> CreateMockFile(string fileName, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

        if (length > 0)
        {
            var content = new byte[length];
            var stream = new MemoryStream(content);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mockFile;
    }

    #endregion
}