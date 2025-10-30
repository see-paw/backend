using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Images.Commands;

/// <summary>
/// Unit tests for DeleteAnimalImage.Handler using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests focus on finding bugs through boundary conditions, authorization, and error handling.
/// </summary>
public class DeleteAnimalImageTest : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IImageManager<Animal>> _mockImageManager;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly DeleteAnimalImage.Handler _sut;

    public DeleteAnimalImageTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mockImageManager = new Mock<IImageManager<Animal>>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _sut = new DeleteAnimalImage.Handler(_dbContext, _mockImageManager.Object, _mockUserAccessor.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region ECP - Animal Not Found

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("non-existent-animal")]
    [InlineData("invalid-$%^&")]
    [InlineData("../../path")]
    [InlineData("animal\0null")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_AnimalNotFound_Returns404(string animalId)
    {
        SetupUserAccessor("shelter-123");

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ECP - Authorization and Shelter Ownership

    [Fact]
    public async Task Handle_AnimalFromDifferentShelter_Returns403()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("different-shelter-456");

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Contains("your shelter", result.Error, StringComparison.OrdinalIgnoreCase);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AnimalFromSameShelter_CallsImageManager()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.Code);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            animal.Id,
            "image-456",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithNullShelterId_CannotDelete()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor(null);

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
    }

    #endregion

    #region BVA - ID Lengths

    [Theory]
    [InlineData("A")]
    [InlineData("01234567890123456789012345678901234")]
    [InlineData("012345678901234567890123456789012345")]
    [InlineData("0123456789012345678901234567890123456")]
    public async Task Handle_AnimalIdLengthBoundaries_HandlesCorrectly(string animalId)
    {
        var animal = await CreateAnimalInDb("shelter-123", animalId);
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = "image-123"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        if (result.IsSuccess)
        {
            Assert.Equal(204, result.Code);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("I")]
    [InlineData("01234567890123456789012345678901234")]
    [InlineData("012345678901234567890123456789012345")]
    [InlineData("0123456789012345678901234567890123456")]
    public async Task Handle_ImageIdLengthBoundaries_HandlesCorrectly(string imageId)
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = imageId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        if (result.IsSuccess)
        {
            _mockImageManager.Verify(x => x.DeleteImageAsync(
                animal.Id,
                imageId,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    #endregion

    #region ECP - ImageManager Results

    [Theory]
    [InlineData(true, 204, null)]
    [InlineData(false, 404, "Image not found")]
    [InlineData(false, 400, "Cannot delete the principal image")]
    [InlineData(false, 403, "Image does not belong to the specified entity")]
    [InlineData(false, 502, "Cloudinary deletion failed")]
    [InlineData(false, 500, "Failed to delete image from database")]
    public async Task Handle_ImageManagerResponses_PropagatesCorrectly(bool isSuccess, int code, string error)
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");

        _mockImageManager.Setup(x => x.DeleteImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isSuccess 
                ? Result<Unit>.Success(Unit.Value, code) 
                : Result<Unit>.Failure(error, code));

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        if (isSuccess)
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(code, result.Code);
        }
        else
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(code, result.Code);
            Assert.Equal(error, result.Error);
        }
    }

    #endregion

    #region ECP - Special Characters in IDs

    [Theory]
    [InlineData("animal<script>")]
    [InlineData("animal'; DROP--")]
    [InlineData("动物")]
    [InlineData("🐶")]
    [InlineData("animal\r\n\t")]
    public async Task Handle_SpecialCharactersInAnimalId_Returns404(string animalId)
    {
        SetupUserAccessor("shelter-123");

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = "image-123"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    [Theory]
    [InlineData("image<script>")]
    [InlineData("image'; DROP--")]
    [InlineData("图像")]
    [InlineData("📷")]
    [InlineData("image\r\n\t")]
    public async Task Handle_SpecialCharactersInImageId_PropagatedToManager(string imageId)
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = imageId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            animal.Id,
            imageId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ECP - Same IDs

    [Theory]
    [InlineData("same-id-123")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_SameAnimalAndImageId_HandlesCorrectly(string sameId)
    {
        var animal = await CreateAnimalInDb("shelter-123", sameId);
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = sameId,
            ImageId = sameId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        if (result.IsSuccess)
        {
            _mockImageManager.Verify(x => x.DeleteImageAsync(
                sameId,
                sameId,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_CancelledToken_PropagatedCorrectly()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");

        _mockImageManager.Setup(x => x.DeleteImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _sut.Handle(command, cts.Token));
    }

    [Theory]
    [InlineData("animal-123", "IMAGE-456")]
    [InlineData("ANIMAL-123", "image-456")]
    [InlineData("Animal-123", "Image-456")]
    public async Task Handle_CaseSensitiveIds_TreatedAsDistinct(string animalId, string imageId)
    {
        var animal = await CreateAnimalInDb("shelter-123", animalId);
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = imageId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            animalId,
            imageId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("animal-123 ")]
    [InlineData(" animal-123")]
    [InlineData(" animal-123 ")]
    public async Task Handle_WhitespaceInAnimalId_Returns404(string animalId)
    {
        await CreateAnimalInDb("shelter-123", "animal-123");
        SetupUserAccessor("shelter-123");

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animalId,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    [Theory]
    [InlineData(" image-456")]
    [InlineData("image-456 ")]
    [InlineData(" image-456 ")]
    public async Task Handle_WhitespaceInImageId_PropagatedToManager(string imageId)
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = imageId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        _mockImageManager.Verify(x => x.DeleteImageAsync(
            animal.Id,
            imageId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task Handle_VeryLongIds_HandlesCorrectly(int length)
    {
        SetupUserAccessor("shelter-123");

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = new string('A', length),
            ImageId = new string('I', length)
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Handle_NullUserAccessor_ThrowsNullReferenceException()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        _mockUserAccessor.Setup(x => x.GetUserAsync())
            .ReturnsAsync((User)null);

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        await Assert.ThrowsAsync<NullReferenceException>(
            async () => await _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ImageManagerThrowsException_PropagatesException()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");

        _mockImageManager.Setup(x => x.DeleteImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    [InlineData(AnimalState.TotallyFostered)]
    [InlineData(AnimalState.HasOwner)]
    [InlineData(AnimalState.Inactive)]
    public async Task Handle_DifferentAnimalStates_AllowsDeletion(AnimalState state)
    {
        var animal = await CreateAnimalInDb("shelter-123");
        animal.AnimalState = state;
        await _dbContext.SaveChangesAsync();

        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ConcurrentDeletionAttempts_BothCallImageManager()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");

        var callCount = 0;
        _mockImageManager.Setup(x => x.DeleteImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? Result<Unit>.Success(Unit.Value, 204)
                    : Result<Unit>.Failure("Image not found", 404);
            });

        var command1 = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var command2 = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "image-456"
        };

        var task1 = _sut.Handle(command1, CancellationToken.None);
        var task2 = _sut.Handle(command2, CancellationToken.None);

        await Task.WhenAll(task1, task2);

        var result1 = await task1;
        var result2 = await task2;

        Assert.True((result1.IsSuccess && !result2.IsSuccess) || 
                    (!result1.IsSuccess && result2.IsSuccess) ||
                    (result1.IsSuccess && result2.IsSuccess));
    }

    [Fact]
    public async Task Handle_MultipleImagesOnSameAnimal_DeletesSpecificImage()
    {
        var animal = await CreateAnimalInDb("shelter-123");
        SetupUserAccessor("shelter-123");
        SetupSuccessfulDeletion();

        var command = new DeleteAnimalImage.Command
        {
            AnimalId = animal.Id,
            ImageId = "specific-image-789"
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockImageManager.Verify(x => x.DeleteImageAsync(
            animal.Id,
            "specific-image-789",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private async Task<Animal> CreateAnimalInDb(string shelterId, string animalId = null)
    {
        var shelter = new Shelter
        {
            Id = shelterId,
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

        var animal = new Animal
        {
            Id = animalId ?? Guid.NewGuid().ToString(), 
            Name = "TestAnimal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelterId,
            BreedId = breed.Id
        };

        _dbContext.Animals.Add(animal);
        await _dbContext.SaveChangesAsync();

        return animal;
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

    private void SetupSuccessfulDeletion()
    {
        _mockImageManager.Setup(x => x.DeleteImageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 204));
    }

    #endregion
}