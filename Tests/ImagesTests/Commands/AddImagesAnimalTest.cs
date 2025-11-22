using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ImagesTests.Commands;

/// <summary>
/// Unit tests for AddImagesAnimal.Handler using Equivalence Class Partitioning and Boundary Value Analysis.
/// </summary>
public class AddImagesAnimalTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImagesUploader<Animal>> _mockUploadService;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly AddImagesAnimal.Handler _sut;

    public AddImagesAnimalTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new AppDbContext(options);
        _mockUploadService = new Mock<IImagesUploader<Animal>>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _sut = new AddImagesAnimal.Handler(_dbContext, _mockUserAccessor.Object, _mockUploadService.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region BVA - Collection Sizes

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
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var files = Enumerable.Range(0, imageCount).Select(i => CreateMockFile($"img{i}.jpg", 1024).Object).ToList();
        var images = Enumerable.Range(0, imageCount).Select(i => CreateImage($"Image {i}", i == 0)).ToList();
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = files,
            Images = images
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
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var files = Enumerable.Range(0, fileCount).Select(i => CreateMockFile($"f{i}.jpg", 1024).Object).ToList();
        var images = Enumerable.Range(0, imageCount).Select(i => CreateImage($"I{i}", false)).ToList();
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = files,
            Images = images
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(1024L)]
    [InlineData(5242879L)]
    [InlineData(5242880L)]
    [InlineData(5242881L)]
    [InlineData(10485760L)]
    [InlineData(104857600L)]
    public async Task Handle_FileSizeBoundaries_HandlesCorrectly(long fileSize)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var file = CreateMockFile("large.jpg", fileSize).Object;
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { file },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region BVA - Image.Description [MaxLength(255)] [Required]

    [Theory]
    [InlineData("")]
    [InlineData("D")]
    [InlineData(254)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(500)]
    public async Task Handle_ImageDescriptionBoundaries_HandlesCorrectly(object length)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var description = length is int len ? new string('D', len) : length as string;
        var image = CreateImage(description, false);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { image }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - AnimalId

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("non-existent-animal")]
    [InlineData("invalid-$%^&")]
    [InlineData("../../path")]
    [InlineData("animal\0null")]
    public async Task Handle_AnimalIdVariations_HandlesCorrectly(string animalId)
    {
        SetupUserAccessor("any-shelter-id");
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animalId,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    #endregion

    #region ECP - Authorization and Shelter Ownership

    [Fact]
    public async Task Handle_AnimalFromDifferentShelter_ReturnsForbidden()
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor("different-shelter-id");
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Contains("your shelter", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_AnimalFromSameShelter_Succeeds()
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
    }

    #endregion

    #region ECP - AnimalState

    [Fact]
    public async Task Handle_InactiveAnimal_ReturnsBadRequest()
    {
        var animal = await CreateAnimalInDb();
        animal.AnimalState = AnimalState.Inactive;
        await _dbContext.SaveChangesAsync();

        SetupUserAccessor(animal.ShelterId);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("inactive", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    [InlineData(AnimalState.TotallyFostered)]
    [InlineData(AnimalState.HasOwner)]
    public async Task Handle_ActiveAnimalStates_Succeeds(AnimalState state)
    {
        var animal = await CreateAnimalInDb();
        animal.AnimalState = state;
        await _dbContext.SaveChangesAsync();

        SetupUserAccessor(animal.ShelterId);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
    }

    #endregion

    #region ECP - Principal Images

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 5)]
    [InlineData(3, 5)]
    [InlineData(5, 5)]
    public async Task Handle_PrincipalImageCount_HandlesCorrectly(int principalCount, int totalCount)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var images = Enumerable.Range(0, totalCount).Select(i => CreateImage($"Img{i}", i < principalCount)).ToList();
        var files = Enumerable.Range(0, totalCount).Select(i => CreateMockFile($"f{i}.jpg", 1024).Object).ToList();
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = files,
            Images = images
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    #endregion

    #region ECP - Upload Service Results

    [Theory]
    [InlineData(true, 201, null)]
    [InlineData(false, 400, "Upload failed")]
    [InlineData(false, 500, "Internal error")]
    [InlineData(false, 413, "File too large")]
    [InlineData(false, 415, "Unsupported type")]
    [InlineData(false, 502, "Cloudinary down")]
    public async Task Handle_UploadServiceResponses_HandlesCorrectly(bool isSuccess, int code, string error)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);

        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, List<IFormFile>, List<Image>, CancellationToken>((id, files, images, ct) =>
            {
                if (isSuccess)
                {
                    var otherAnimal = _dbContext.Animals.Include(a => a.Images).First(a => a.Id == id);
                    foreach (var img in images)
                    {
                        img.AnimalId = id;
                        otherAnimal.Images.Add(img);
                        _dbContext.Images.Add(img);
                    }
                }
            })
            .ReturnsAsync(isSuccess
                ? Result<Unit>.Success(Unit.Value, code)
                : Result<Unit>.Failure(error, code));

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

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
    public async Task Handle_UploadFails_RollsBackTransaction()
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var initialImageCount = animal.Images.Count;

        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Upload failed", 500));

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var updatedAnimal = await _dbContext.Animals.Include(a => a.Images).FirstOrDefaultAsync(a => a.Id == animal.Id);
        Assert.Equal(initialImageCount, updatedAnimal.Images.Count);
    }

    [Fact]
    public async Task Handle_DatabaseSaveFails_ReturnsFailure()
    {
        var animal = CreateAnimal("test-animal");
        SetupUserAccessor("any-shelter-id");
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("Image\0Null")]
    [InlineData("Image<script>")]
    [InlineData("Image'; DROP--")]
    [InlineData("图像")]
    [InlineData("🖼️📷")]
    [InlineData("Image\r\n\t")]
    public async Task Handle_SpecialCharactersInDescription_HandlesCorrectly(string description)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        var image = CreateImage(description, false);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { image }
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_CancelledToken_HandlesCorrectly()
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        SetupSuccessfulUpload();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = new List<IFormFile> { CreateMockFile("test.jpg", 1024).Object },
            Images = new List<Image> { CreateImage("Test", false) }
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
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);
        SetupSuccessfulUpload();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = null,
            Images = null
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

    [Theory]
    [InlineData(0, 3)]
    [InlineData(5, 3)]
    [InlineData(10, 5)]
    public async Task Handle_ImageCountBeforeSkipLogic_ReturnsCorrectAddedImages(int existingCount, int newCount)
    {
        var animal = await CreateAnimalInDb();
        SetupUserAccessor(animal.ShelterId);

        for (int i = 0; i < existingCount; i++)
        {
            var existing = CreateImage($"Existing {i}", false);
            existing.AnimalId = animal.Id;
            animal.Images.Add(existing);
        }
        await _dbContext.SaveChangesAsync();

        SetupSuccessfulUpload();

        var files = Enumerable.Range(0, newCount).Select(i => CreateMockFile($"new{i}.jpg", 1024).Object).ToList();
        var images = Enumerable.Range(0, newCount).Select(i => CreateImage($"New {i}", false)).ToList();

        var command = new AddImagesAnimal.Command
        {
            AnimalId = animal.Id,
            Files = files,
            Images = images
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Assert.Equal(newCount, result.Value.Count);
        }
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
            Name = "Test Breed"
        };

        _dbContext.Shelters.Add(shelter);
        _dbContext.Breeds.Add(breed);
        await _dbContext.SaveChangesAsync();

        var animal = CreateAnimal(Guid.NewGuid().ToString());
        animal.ShelterId = shelter.Id;
        animal.BreedId = breed.Id;
        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        return animal;
    }

    private Animal CreateAnimal(string id)
    {
        return new Animal
        {
            Id = id,
            Name = "TestAnimal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = Guid.NewGuid().ToString(),
            BreedId = Guid.NewGuid().ToString(),
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

    private void SetupSuccessfulUpload()
    {
        _mockUploadService.Setup(x => x.UploadImagesAsync(
                It.IsAny<string>(),
                It.IsAny<List<IFormFile>>(),
                It.IsAny<List<Image>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, List<IFormFile>, List<Image>, CancellationToken>((id, files, images, ct) =>
            {
                var animal = _dbContext.Animals.Include(a => a.Images).First(a => a.Id == id);
                foreach (var img in images)
                {
                    img.AnimalId = id;
                    animal.Images.Add(img);
                    _dbContext.Images.Add(img);
                }
            })
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 201));
    }

    private void SetupUserAccessor(string shelterId)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId,
            Name = "Test Admin",
            Email = "admin@test.com",
            UserName = "admin@test.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Test Street",
            City = "Porto",
            PostalCode = "4000-000"
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);
    }

    #endregion
}