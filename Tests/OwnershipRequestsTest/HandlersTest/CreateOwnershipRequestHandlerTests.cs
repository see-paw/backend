using Application.Interfaces;
using Application.OwnershipRequests.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.OwnershipRequests;

public class CreateOwnershipRequestHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<INotificationService> _mockNotificationService;

    public CreateOwnershipRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private async Task<(Animal animal, User user, Shelter shelter)> SeedAnimalAsync(
        AnimalState animalState,
        bool withExistingRequest = false,
        string? existingRequestUserId = null)
    {
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Test City",
            PostalCode = "1234-567",
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

        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Max",
            Colour = "Brown",
            Cost = 150,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            BreedId = breed.Id,
            ShelterId = shelter.Id,
            AnimalState = animalState
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "User Street",
            City = "User City",
            PostalCode = "1234-567"
        };

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Animals.Add(animal);
        _context.Users.Add(user);

        if (withExistingRequest && existingRequestUserId != null)
        {
            var existingRequest = new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = existingRequestUserId,
                Amount = 150,
                Status = OwnershipStatus.Pending
            };
            _context.OwnershipRequests.Add(existingRequest);
        }

        await _context.SaveChangesAsync();
        return (animal, user, shelter);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = Guid.NewGuid().ToString()
        }, default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenAnimalHasOwner()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.HasOwner);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenAnimalIsInactive()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Inactive);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenUserAlreadyHasPendingRequest()
    {
        var userId = Guid.NewGuid().ToString();
        var (animal, user, shelter) = await SeedAnimalAsync(
            AnimalState.Available,
            withExistingRequest: true,
            existingRequestUserId: userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnSuccess_WhenAllConditionsAreMet()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCreateRequestWithCorrectAnimalId()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(animal.Id, result.Value!.AnimalId);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCreateRequestWithCorrectUserId()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(user.Id, result.Value!.UserId);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCreateRequestWithAmountEqualToAnimalCost()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(animal.Cost, result.Value!.Amount);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCreateRequestWithPendingStatus()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.Equal(OwnershipStatus.Pending, result.Value!.Status);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldPersistRequestInDatabase()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        var savedRequest = await _context.OwnershipRequests.FindAsync(result.Value!.Id);
        Assert.NotNull(savedRequest);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnRequestWithAnimalIncluded()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.NotNull(result.Value!.Animal);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnRequestWithUserIncluded()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        Assert.NotNull(result.Value!.User);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldNotifyAdminCAA_WhenRequestIsCreated()
    {
        var (animal, user, shelter) = await SeedAnimalAsync(AnimalState.Available);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelter.Id,
            Name = "Admin User",
            Email = "admin@test.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Admin Street",
            City = "Admin City",
            PostalCode = "1234-567"
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new CreateOwnershipRequest.Handler(
            _context,
            _mockUserAccessor.Object,
            _mockNotificationService.Object);

        await handler.Handle(new CreateOwnershipRequest.Command
        {
            AnimalID = animal.Id
        }, default);

        _mockNotificationService.Verify(
            x => x.CreateAndSendToUserAsync(
                adminUser.Id,
                NotificationType.NEW_OWNERSHIP_REQUEST,
                It.IsAny<string>(),
                animal.Id,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}