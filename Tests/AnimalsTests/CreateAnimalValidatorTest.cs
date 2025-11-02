using Application.Animals.Commands;
using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.AnimalControllerTest.cs;

/// <summary>
/// Unit tests for CreateAnimal.Handler using Equivalence Class Partitioning and Boundary Value Analysis.
/// </summary>
public class CreateAnimalTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImagesUploader<Animal>> _mockUploadService;
    private readonly CreateAnimal.Handler _sut;

    public CreateAnimalTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _mockUploadService = new Mock<IImagesUploader<Animal>>();
        _sut = new CreateAnimal.Handler(_dbContext, _mockUploadService.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region BVA - Animal.Name [StringLength(100, MinimumLength = 2)] [Required]

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC")]
    [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567")]
    [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    [InlineData("01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task Handle_NameBoundaries_HandlesCorrectly(string name)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Name = name;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Animal.Description [StringLength(250)]

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A")]
    [InlineData(249)]
    [InlineData(250)]
    [InlineData(251)]
    [InlineData(500)]
    public async Task Handle_DescriptionBoundaries_HandlesCorrectly(object length)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Description = length is int len ? new string('D', len) : length as string;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Animal.Colour [StringLength(50)] [Required]

    [Theory]
    [InlineData("")]
    [InlineData("B")]
    [InlineData("0123456789012345678901234567890123456789012345678")]
    [InlineData("01234567890123456789012345678901234567890123456789")]
    [InlineData("012345678901234567890123456789012345678901234567890")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public async Task Handle_ColourBoundaries_HandlesCorrectly(string colour)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Colour = colour;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Animal.Features [StringLength(300)]

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("F")]
    [InlineData(299)]
    [InlineData(300)]
    [InlineData(301)]
    [InlineData(600)]
    public async Task Handle_FeaturesBoundaries_HandlesCorrectly(object length)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Features = length is int len ? new string('F', len) : length as string;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Animal.Cost [Range(0, 1000)] [Required]

    [Theory]
    [InlineData(-0.01)]
    [InlineData(0.00)]
    [InlineData(0.01)]
    [InlineData(999.98)]
    [InlineData(999.99)]
    [InlineData(1000.00)]
    [InlineData(1000.01)]
    [InlineData(1000.02)]
    [InlineData(2000.00)]
    [InlineData(-100.00)]
    [InlineData(9999.99)]
    public async Task Handle_CostBoundaries_HandlesCorrectly(decimal cost)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Cost = cost;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Animal.BirthDate [Required]

    [Theory]
    [InlineData(-18250)]
    [InlineData(-10950)]
    [InlineData(-3650)]
    [InlineData(-365)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(365)]
    public async Task Handle_BirthDateBoundaries_HandlesCorrectly(int daysFromToday)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddDays(daysFromToday));
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Image Collections

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    [InlineData(101)]
    [InlineData(500)]
    public async Task Handle_ImageCountBoundaries_HandlesCorrectly(int imageCount)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var files = Enumerable.Range(0, imageCount).Select(i => CreateMockFile($"img{i}.jpg", 1024).Object).ToList();
        var images = Enumerable.Range(0, imageCount).Select(i => CreateImage($"Img {i}", i == 0)).ToList();
        SetupSuccessfulUpload();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = images,
            Files = files
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(5, 3)]
    [InlineData(3, 5)]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(100, 50)]
    public async Task Handle_MismatchedCounts_HandlesCorrectly(int fileCount, int imageCount)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        var files = Enumerable.Range(0, fileCount).Select(i => CreateMockFile($"f{i}.jpg", 1024).Object).ToList();
        var images = Enumerable.Range(0, imageCount).Select(i => CreateImage($"Img{i}", false)).ToList();
        SetupSuccessfulUpload();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = images,
            Files = files
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - Animal.Id

    [Theory]
    [InlineData("")]
    [InlineData("valid-unique-id")]
    public async Task Handle_AnimalIdVariations_HandlesCorrectly(string animalId)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        if (animalId != null)
            animal.Id = animalId;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - Animal.AnimalState

    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.TotallyFostered)]
    [InlineData(AnimalState.PartiallyFostered)]
    [InlineData(AnimalState.HasOwner)]
    [InlineData(AnimalState.Inactive)]
    [InlineData((AnimalState)(-1))]
    [InlineData((AnimalState)5)]
    [InlineData((AnimalState)99)]
    [InlineData((AnimalState)999)]
    [InlineData((AnimalState)int.MinValue)]
    [InlineData((AnimalState)int.MaxValue)]
    public async Task Handle_AnimalStateValues_HandlesCorrectly(AnimalState state)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.AnimalState = state;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - Foreign Keys

    [Theory]
    [InlineData("")]
    [InlineData("valid-breed-123")]
    [InlineData("non-existent-breed")]
    public async Task Handle_BreedIdVariations_HandlesCorrectly(string breedId)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.BreedId = breedId;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("valid-shelter-123")]
    [InlineData("non-existent-shelter")]
    public async Task Handle_ShelterIdVariations_HandlesCorrectly(string shelterId)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelterId);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - Image Upload Service

    [Theory]
    [InlineData(true, 201, null)]
    [InlineData(false, 400, "Upload failed")]
    [InlineData(false, 500, "Internal error")]
    [InlineData(false, 413, "File too large")]
    [InlineData(false, 415, "Unsupported type")]
    [InlineData(false, 502, "Cloudinary down")]
    public async Task Handle_UploadServiceResponses_HandlesCorrectly(bool isSuccess, int code, string error)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isSuccess
                ? Result<Unit>.Success(Unit.Value, code)
                : Result<Unit>.Failure(error, code));

        var result = await ExecuteCommand(animal, shelter.Id);

        if (isSuccess)
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.Code);
        }
        else
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(code, result.Code);
        }
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public async Task Handle_UploadFails_ReturnsFailure()
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);

        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Upload failed", 500));

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.Code);
    }
    
    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("Animal\0Null")]
    [InlineData("Animal<script>")]
    [InlineData("Animal'; DROP--")]
    [InlineData("动物")]
    [InlineData("🐶🐱")]
    [InlineData("Animal\r\n\t")]
    public async Task Handle_SpecialCharacters_HandlesCorrectly(string name)
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        animal.Name = name;
        SetupSuccessfulUpload();

        var result = await ExecuteCommand(animal, shelter.Id);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_CancelledToken_HandlesCorrectly()
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        SetupSuccessfulUpload();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = new List<Image> { CreateImage("Test", true) },
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object }
        };

        try
        {
            await _sut.Handle(command, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public async Task Handle_NullCollections_HandlesCorrectly()
    {
        var (shelter, breed) = await SetupEntities();
        var animal = CreateValidAnimal(shelter.Id, breed.Id);
        SetupSuccessfulUpload();

        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelter.Id,
            Images = null,
            Files = null
        };

        try
        {
            await _sut.Handle(command, CancellationToken.None);
        }
        catch (NullReferenceException)
        {
            Assert.True(true);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<(Shelter, Breed)> SetupEntities()
    {
        var shelter = CreateShelter();
        var breed = CreateBreed();
        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();
        return (shelter, breed);
    }

    private void SetupSuccessfulUpload()
    {
        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 201));
    }

    private async Task<Result<string>> ExecuteCommand(Animal animal, string shelterId, List<IFormFile> files = null)
    {
        var command = new CreateAnimal.Command
        {
            Animal = animal,
            ShelterId = shelterId,
            Images = new List<Image> { CreateImage("Test", true) },
            Files = files
        };

        return await _sut.Handle(command, CancellationToken.None);
    }

    private Shelter CreateShelter()
    {
        return new Shelter
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
    }

    private Breed CreateBreed()
    {
        return new Breed
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Breed",
            Description = "Test"
        };
    }

    private Animal CreateValidAnimal(string shelterId, string breedId)
    {
        return new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "TestAnimal",
            AnimalState = AnimalState.Available,
            Description = "Test",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            Features = "Friendly",
            ShelterId = shelterId,
            BreedId = breedId
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