using Application.Interfaces;
using Application.OwnershipRequests.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.OwnershipRequests;

public class RejectOwnershipRequestHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<INotificationService> _mockNotificationService;
    public RejectOwnershipRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private async Task<(Animal animal, OwnershipRequest request, User user, Shelter shelter)> SeedOwnershipRequestAsync(
        OwnershipStatus status,
        string shelterId)
    {
        var shelter = new Shelter
        {
            Id = shelterId,
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
            Cost = 100,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            BreedId = breed.Id,
            ShelterId = shelterId,
            AnimalState = AnimalState.Available
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

        var request = new OwnershipRequest
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animal.Id,
            UserId = user.Id,
            Amount = 100,
            Status = status
        };

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Animals.Add(animal);
        _context.Users.Add(user);
        _context.OwnershipRequests.Add(request);

        await _context.SaveChangesAsync();
        return (animal, request, user, shelter);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnForbidden_WhenUserIsNotShelterAdmin()
    {
        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            Guid.NewGuid().ToString());

        var nonAdminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = null // Not a shelter admin
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(nonAdminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var shelterId = Guid.NewGuid().ToString();
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = Guid.NewGuid().ToString()
        }, default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnForbidden_WhenAnimalNotInAdminShelter()
    {
        var shelterId = Guid.NewGuid().ToString();
        var differentShelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = differentShelterId // Different shelter
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnBadRequest_WhenStatusIsNotAnalysing()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Pending, // Not Analysing
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnSuccess_WhenAllConditionsAreMet()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldUpdateStatusToRejected()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.Equal(OwnershipStatus.Rejected, updatedRequest!.Status);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldSetUpdatedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.NotNull(updatedRequest!.UpdatedAt);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldSetRejectionReason_WhenProvided()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var rejectionReason = "Does not meet requirements";

        await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id,
            RejectionReason = rejectionReason
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.Equal(rejectionReason, updatedRequest!.RequestInfo);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldNotSetRejectionReason_WhenNotProvided()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id,
            RejectionReason = null
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.Null(updatedRequest!.RequestInfo);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnRequestWithAnimalIncluded()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.NotNull(result.Value!.Animal);
    }

    [Fact]
    public async Task RejectOwnershipRequest_ShouldReturnRequestWithUserIncluded()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new RejectOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new RejectOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.NotNull(result.Value!.User);
    }
}