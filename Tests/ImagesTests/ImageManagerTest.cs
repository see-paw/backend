using Application.Images;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Infrastructure.Images;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ImagesTests;

/// <summary>
/// Unit tests for ImageManager.
/// </summary>
public class ImageManagerTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ICloudinaryService> _mockCloudinaryService;
    private readonly Mock<IImageOwnerLoader<Animal>> _mockLoader;
    private readonly Mock<IImageOwnerLinker<Animal>> _mockLinker;
    private readonly ImageManager<Animal> _sut;

    public ImageManagerTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _mockCloudinaryService = new Mock<ICloudinaryService>();
        _mockLoader = new Mock<IImageOwnerLoader<Animal>>();
        _mockLinker = new Mock<IImageOwnerLinker<Animal>>();

        _sut = new ImageManager<Animal>(
            _dbContext,
            _mockCloudinaryService.Object,
            _mockLoader.Object,
            _mockLinker.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region AddImageAsync Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("animal-123")]
    [InlineData("animal-123\0null-byte")]
    [InlineData("../../path-traversal")]
    [InlineData("invalid-id-$%^&")]
    [InlineData("non-existent-entity-999")]
    public async Task AddImageAsync_EntityIdVariations_HandlesCorrectly(string entityId)
    {
        var animal = CreateAnimal();
        _mockLoader.Setup(x => x.LoadAsync(
                It.IsAny<AppDbContext>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.UploadImage(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://test.com/img.jpg",
                PublicId = "test-public-id"
            });

        var file = CreateMockFile("test.jpg", 1024).Object;
        var result = await _sut.AddImageAsync(entityId, file, "Test Description", false, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region DeleteImageAsync Tests

    [Theory]
    [InlineData("ok", true)]
    [InlineData("OK", true)]
    [InlineData("Ok", true)]
    [InlineData("oK", true)]
    [InlineData("ok ", false)]
    [InlineData(" ok", false)]
    [InlineData("error", false)]
    [InlineData("failed", false)]
    [InlineData("timeout", false)]
    [InlineData("not found", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task DeleteImageAsync_CloudinaryResponses_HandlesCorrectly(string cloudinaryResponse, bool shouldSucceed)
    {
        var animal = await CreateAnimalInDb();
        var image = CreateImage("Test Image", false);
        image.AnimalId = animal.Id;
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        _mockLoader.Setup(x => x.LoadAsync(
                It.IsAny<AppDbContext>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _mockCloudinaryService.Setup(x => x.DeleteImage(It.IsAny<string>()))
            .ReturnsAsync(cloudinaryResponse);

        var result = await _sut.DeleteImageAsync(animal.Id, image.Id, CancellationToken.None);

        if (shouldSucceed)
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(204, result.Code);
        }
        else
        {
            Assert.False(result.IsSuccess);
        }
    }

    [Fact]
    public async Task DeleteImageAsync_PrincipalImage_ReturnsFailure()
    {
        var animal = await CreateAnimalInDb();
        var image = CreateImage("Principal Image", true);
        image.AnimalId = animal.Id;
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        _mockLoader.Setup(x => x.LoadAsync(
                It.IsAny<AppDbContext>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var result = await _sut.DeleteImageAsync(animal.Id, image.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Cannot delete the principal image.", result.Error);
    }

    [Fact]
    public async Task DeleteImageAsync_ConcurrentDeletion_HandlesRaceCondition()
    {
        var animal = await CreateAnimalInDb();
        var image = CreateImage("Test Image", false);
        image.AnimalId = animal.Id;
        _dbContext.Images.Add(image);
        await _dbContext.SaveChangesAsync();

        _mockLoader.Setup(x => x.LoadAsync(
                It.IsAny<AppDbContext>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var callCount = 0;
        _mockCloudinaryService.Setup(x => x.DeleteImage(It.IsAny<string>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? "ok" : "not found";
            });

        var task1 = _sut.DeleteImageAsync(animal.Id, image.Id, CancellationToken.None);
        var task2 = _sut.DeleteImageAsync(animal.Id, image.Id, CancellationToken.None);

        await Task.WhenAll(task1, task2);

        Assert.True((task1.Result.IsSuccess && !task2.Result.IsSuccess) || 
                    (!task1.Result.IsSuccess && task2.Result.IsSuccess));
    }

    #endregion

    #region Helper Methods

    private async Task<Animal> CreateAnimalInDb()
    {
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Test St",
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
            Name = "Test Breed",
            Description = "Test"
        };

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateAnimal();
        animal.ShelterId = shelter.Id;
        animal.BreedId = breed.Id;
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        return animal;
    }

    private Animal CreateAnimal()
    {
        return new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "TestAnimal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            Images = new List<Image>()
        };
    }

    private Image CreateImage(string description, bool isPrincipal)
    {
        return new Image
        {
            Id = Guid.NewGuid().ToString(),
            PublicId = $"public-{Guid.NewGuid()}",
            Url = "https://test.com/img.jpg",
            Description = description,
            IsPrincipal = isPrincipal
        };
    }

    private Mock<IFormFile> CreateMockFile(string fileName, long length)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(length);
        mock.Setup(f => f.ContentType).Returns("image/jpeg");

        if (length > 0)
        {
            var stream = new MemoryStream(new byte[length]);
            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mock;
    }

    #endregion
}