using Application.Interfaces;
using Application.OwnershipRequests.Commands;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.OwnershipRequests;

public class ApproveOwnershipRequestHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<INotificationService> _mockNotificationService;

    public ApproveOwnershipRequestHandlerTests()
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
        AnimalState animalState,
        string shelterId,
        bool withActiveFostering = false,
        bool withOtherRequests = false)
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

        if (withActiveFostering)
        {
            var fostering = new Fostering
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = user.Id,
                Status = FosteringStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-10),
                Amount = 50
            };
            _context.Fosterings.Add(fostering);
        }

        if (withOtherRequests)
        {
            var otherUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Other User",
                Email = "other@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Street = "Other Street",
                City = "Other City",
                PostalCode = "9876-543"
            };

            var pendingRequest = new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = otherUser.Id,
                Amount = 100,
                Status = OwnershipStatus.Pending
            };

            var analysingRequest = new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = otherUser.Id,
                Amount = 100,
                Status = OwnershipStatus.Analysing
            };

            _context.Users.Add(otherUser);
            _context.OwnershipRequests.Add(pendingRequest);
            _context.OwnershipRequests.Add(analysingRequest);
        }

        await _context.SaveChangesAsync();
        return (animal, request, user, shelter);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnForbidden_WhenUserIsNotShelterAdmin()
    {
        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            Guid.NewGuid().ToString());

        var nonAdminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = null // Not a shelter admin
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(nonAdminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var shelterId = Guid.NewGuid().ToString();
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = Guid.NewGuid().ToString()
        }, default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnForbidden_WhenAnimalNotInAdminShelter()
    {
        var shelterId = Guid.NewGuid().ToString();
        var differentShelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = differentShelterId // Different shelter
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnBadRequest_WhenAnimalIsInactive()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Inactive,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnBadRequest_WhenAnimalAlreadyHasOwner()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.HasOwner,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnBadRequest_WhenAnotherRequestAlreadyApproved()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        // Add another approved request for the same animal
        var otherUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other User",
            Email = "other@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Other Street",
            City = "Other City",
            PostalCode = "9876-543"
        };

        var approvedRequest = new OwnershipRequest
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animal.Id,
            UserId = otherUser.Id,
            Amount = 100,
            Status = OwnershipStatus.Approved
        };

        _context.Users.Add(otherUser);
        _context.OwnershipRequests.Add(approvedRequest);
        await _context.SaveChangesAsync();

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnBadRequest_WhenStatusIsNotAnalysing()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Pending, // Not Analysing
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldReturnSuccess_WhenAllConditionsAreMet()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldUpdateRequestStatusToApproved()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.Equal(OwnershipStatus.Approved, updatedRequest!.Status);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetApprovedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.NotNull(updatedRequest!.ApprovedAt);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetRequestUpdatedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedRequest = await _context.OwnershipRequests.FindAsync(request.Id);
        Assert.NotNull(updatedRequest!.UpdatedAt);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldTransferAnimalOwnershipToUser()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedAnimal = await _context.Animals.FindAsync(animal.Id);
        Assert.Equal(user.Id, updatedAnimal!.OwnerId);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldUpdateAnimalStateToHasOwner()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedAnimal = await _context.Animals.FindAsync(animal.Id);
        Assert.Equal(AnimalState.HasOwner, updatedAnimal!.AnimalState);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetAnimalOwnershipStartDate()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedAnimal = await _context.Animals.FindAsync(animal.Id);
        Assert.NotNull(updatedAnimal!.OwnershipStartDate);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetAnimalUpdatedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var updatedAnimal = await _context.Animals.FindAsync(animal.Id);
        Assert.NotNull(updatedAnimal!.UpdatedAt);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetFosteringStatusToCancelled()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withActiveFostering: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var fostering = await _context.Fosterings
            .FirstAsync(f => f.AnimalId == animal.Id);

        Assert.Equal(FosteringStatus.Cancelled, fostering.Status);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetFosteringEndDate()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withActiveFostering: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var fostering = await _context.Fosterings
            .FirstAsync(f => f.AnimalId == animal.Id);

        Assert.NotNull(fostering.EndDate);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetFosteringUpdatedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withActiveFostering: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var fostering = await _context.Fosterings
            .FirstAsync(f => f.AnimalId == animal.Id);

        Assert.NotNull(fostering.UpdatedAt);
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetOtherRequestsStatusToRejected()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withOtherRequests: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var otherRequests = await _context.OwnershipRequests
            .Where(or => or.AnimalId == animal.Id && or.Id != request.Id)
            .ToListAsync();

        Assert.All(otherRequests, or => Assert.Equal(OwnershipStatus.Rejected, or.Status));
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetOtherRequestsUpdatedAtTimestamp()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withOtherRequests: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var otherRequests = await _context.OwnershipRequests
            .Where(or => or.AnimalId == animal.Id && or.Id != request.Id)
            .ToListAsync();

        Assert.All(otherRequests, or => Assert.NotNull(or.UpdatedAt));
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldSetOtherRequestsRejectionMessage()
    {
        var shelterId = Guid.NewGuid().ToString();

        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId,
            withOtherRequests: true);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        var otherRequests = await _context.OwnershipRequests
            .Where(or => or.AnimalId == animal.Id && or.Id != request.Id)
            .ToListAsync();

        Assert.All(otherRequests, or => Assert.Contains("Automatically rejected", or.RequestInfo));
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldNotifyUser_WhenRequestIsApproved()
    {
        var shelterId = Guid.NewGuid().ToString();
        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
            OwnershipStatus.Analysing,
            AnimalState.Available,
            shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(
            _context,
            _mockUserAccessor.Object,
            _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        _mockNotificationService.Verify(
            x => x.CreateAndSendToUserAsync(
                request.UserId,
                NotificationType.OWNERSHIP_REQUEST_APPROVED,
                It.IsAny<string>(),
                request.AnimalId,
                request.Id,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ApproveOwnershipRequest_ShouldNotifyFosteringSponsors_WhenAnimalHasActiveFosterings()
    {
        var shelterId = Guid.NewGuid().ToString();
        var (animal, request, user, shelter) = await SeedOwnershipRequestAsync(
             OwnershipStatus.Analysing,
             AnimalState.Available,
             shelterId);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        var sponsor = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Sponsor User",
            Email = "sponsor@test.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Sponsor Street",
            City = "Sponsor City",
            PostalCode = "1234-567"
        };

        var fostering = new Fostering
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animal.Id,
            UserId = sponsor.Id,
            Amount = 50,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        _context.Users.Add(sponsor);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new ApproveOwnershipRequest.Handler(
            _context,
            _mockUserAccessor.Object,
            _mockNotificationService.Object);

        await handler.Handle(new ApproveOwnershipRequest.Command
        {
            OwnershipRequestId = request.Id
        }, default);

        _mockNotificationService.Verify(
            x => x.CreateAndSendToUserAsync(
                sponsor.Id,
                NotificationType.FOSTERED_ANIMAL_ADOPTED,
                It.IsAny<string>(),
                animal.Id,
                request.Id,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}