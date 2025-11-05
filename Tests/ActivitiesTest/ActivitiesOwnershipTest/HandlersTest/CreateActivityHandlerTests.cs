using Application.Activities.Commands;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.ActivitiesTest.ActivitiesOwnershipTest.HandlersTest;

/// <summary>
/// Unit tests for CreateOwnershipActivity handler.
/// Validates all business logic for creating ownership activities.
/// </summary>
public class CreateActivityHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<INotificationService> _mockNotificationService;

    public CreateActivityHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private async Task<(Animal animal, User owner, User otherUser, Shelter shelter)> SeedDataAsync(
    AnimalState animalState = AnimalState.HasOwner,
    bool withApprovedOwnershipRequest = true,
    bool withCompletedActivity = false,
    DateTime? completedActivityEndDate = null,
    bool setOwnerIdEvenIfAvailable = false)
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

        var owner = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Owner User",
            Email = "owner@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Owner Street",
            City = "Owner City",
            PostalCode = "1234-567"
        };

        var otherUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other User",
            Email = "other@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Other Street",
            City = "Other City",
            PostalCode = "1234-567"
        };

        string? ownerId = null;
        if (animalState == AnimalState.HasOwner)
        {
            ownerId = owner.Id;
        }
        else if (setOwnerIdEvenIfAvailable) // Edge case: Available but with OwnerId
        {
            ownerId = owner.Id;
        }

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
            AnimalState = animalState,
            OwnerId = ownerId
        };

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Users.Add(owner);
        _context.Users.Add(otherUser);
        _context.Animals.Add(animal);

        if (withApprovedOwnershipRequest)
        {
            var ownershipRequest = new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = owner.Id,
                Amount = 150,
                Status = OwnershipStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddDays(-5)
            };
            _context.OwnershipRequests.Add(ownershipRequest);
        }

        if (withCompletedActivity && completedActivityEndDate.HasValue)
        {
            var completedActivity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                UserId = owner.Id,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Completed,
                StartDate = completedActivityEndDate.Value.AddHours(-3),
                EndDate = completedActivityEndDate.Value
            };
            _context.Activities.Add(completedActivity);
        }

        await _context.SaveChangesAsync();
        return (animal, owner, otherUser, shelter);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        }, default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(otherUser.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object); 

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenAnimalDoesNotHaveOwnerStatus()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync(
            animalState: AnimalState.Available,
            withApprovedOwnershipRequest: true,
            setOwnerIdEvenIfAvailable: true); // Edge case: Available but with OwnerId

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnForbidden_WhenNoApprovedOwnershipRequestExists()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync(
            withApprovedOwnershipRequest: false);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenStartDateIsLessThan24HoursInAdvance()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = DateTime.UtcNow.AddHours(23),
            EndDate = DateTime.UtcNow.AddHours(25)
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(2)
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenEndDateIsOnDayBeforeStartDate()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = new DateTime(2025, 12, 5, 10, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 12, 4, 14, 0, 0, DateTimeKind.Utc);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenStartTimeIsBeforeShelterOpening()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        // Shelter opens at 09:00, trying to start at 08:00
        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(8);
        var endDate = startDate.AddHours(2);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenEndTimeIsAfterShelterClosing()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        // Shelter closes at 18:00, trying to end at 19:00
        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(17);
        var endDate = startDate.AddHours(2);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenConflictWithCompletedActivity()
    {
        var completedEndDate = DateTime.UtcNow.AddDays(1);
        var (animal, owner, otherUser, shelter) = await SeedDataAsync(
            withCompletedActivity: true,
            completedActivityEndDate: completedEndDate);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        // Trying to start before the completed activity ended
        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = completedEndDate.AddHours(-1),
            EndDate = completedEndDate.AddHours(1)
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnSuccess_WhenAllConditionsAreMet()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithCorrectAnimalId()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(animal.Id, result.Value!.AnimalId);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithCorrectUserId()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(owner.Id, result.Value!.UserId);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithOwnershipType()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(ActivityType.Ownership, result.Value!.Type);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithActiveStatus()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(ActivityStatus.Active, result.Value!.Status);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithCorrectStartDate()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(startDate, result.Value!.StartDate);
    }

    [Fact]
    public async Task CreateActivity_ShouldCreateActivityWithCorrectEndDate()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(endDate, result.Value!.EndDate);
    }

    [Fact]
    public async Task CreateActivity_ShouldPersistActivityInDatabase()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        var savedActivity = await _context.Activities.FindAsync(result.Value!.Id);
        Assert.NotNull(savedActivity);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnActivityWithAnimalIncluded()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.NotNull(result.Value!.Animal);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnActivityWithUserIncluded()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.NotNull(result.Value!.User);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnCode201_WhenActivityIsCreated()
    {
        var (animal, owner, otherUser, shelter) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        var startDate = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.Equal(201, result.Code);
    }

    [Fact]
    public async Task CreateActivity_ShouldAllowActivityAfterCompletedActivity()
    {
        var completedEndDate = DateTime.UtcNow.AddDays(1).Date.AddHours(12); // 12:00
        var (animal, owner, otherUser, shelter) = await SeedDataAsync(
            withCompletedActivity: true,
            completedActivityEndDate: completedEndDate);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new CreateOwnershipActivity.Handler(_context, _mockUserAccessor.Object, _mockNotificationService.Object);

        // Start AFTER the completed activity ended (14:00 > 12:00)
        var startDate = completedEndDate.AddDays(1).Date.AddHours(14);
        var endDate = startDate.AddHours(3);

        var result = await handler.Handle(new CreateOwnershipActivity.Command
        {
            AnimalId = animal.Id,
            StartDate = startDate,
            EndDate = endDate
        }, default);

        Assert.True(result.IsSuccess);
    }
}