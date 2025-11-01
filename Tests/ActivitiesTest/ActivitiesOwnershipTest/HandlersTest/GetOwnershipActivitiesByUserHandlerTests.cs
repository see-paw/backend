using Application.Activities.Queries;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Activities;

/// <summary>
/// Unit tests for GetOwnershipActivitiesByUser handler.
/// Validates retrieval, filtering, ordering, and pagination of ownership activities.
/// </summary>
public class GetOwnershipActivitiesByUserHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public GetOwnershipActivitiesByUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<(User owner, User otherUser, Animal animal1, Animal animal2)> SeedDataAsync(
        int activeActivitiesCount = 1,
        int completedActivitiesCount = 1,
        int canceledActivitiesCount = 1,
        bool addFosteringActivities = false)
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

        var animal1 = new Animal
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
            AnimalState = AnimalState.HasOwner,
            OwnerId = owner.Id
        };

        var animal2 = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Bella",
            Colour = "White",
            Cost = 120,
            Species = Species.Cat,
            Size = SizeType.Small,
            Sex = SexType.Female,
            BirthDate = new DateOnly(2021, 6, 15),
            Sterilized = true,
            BreedId = breed.Id,
            ShelterId = shelter.Id,
            AnimalState = AnimalState.HasOwner,
            OwnerId = owner.Id
        };

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Users.Add(owner);
        _context.Users.Add(otherUser);
        _context.Animals.Add(animal1);
        _context.Animals.Add(animal2);

        var baseDate = DateTime.UtcNow.AddDays(2);

        // Add Active activities
        for (int i = 0; i < activeActivitiesCount; i++)
        {
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = i % 2 == 0 ? animal1.Id : animal2.Id,
                UserId = owner.Id,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(i),
                EndDate = baseDate.AddDays(i).AddHours(2)
            };
            _context.Activities.Add(activity);
        }

        // Add Completed activities
        for (int i = 0; i < completedActivitiesCount; i++)
        {
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = i % 2 == 0 ? animal1.Id : animal2.Id,
                UserId = owner.Id,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Completed,
                StartDate = baseDate.AddDays(-10 - i),
                EndDate = baseDate.AddDays(-10 - i).AddHours(2)
            };
            _context.Activities.Add(activity);
        }

        // Add Cancelled activities
        for (int i = 0; i < canceledActivitiesCount; i++)
        {
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = i % 2 == 0 ? animal1.Id : animal2.Id,
                UserId = owner.Id,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Cancelled,
                StartDate = baseDate.AddDays(-5 - i),
                EndDate = baseDate.AddDays(-5 - i).AddHours(2)
            };
            _context.Activities.Add(activity);
        }

        // Add Fostering activities (should NOT be returned)
        if (addFosteringActivities)
        {
            var fosteringActivity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal1.Id,
                UserId = owner.Id,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = baseDate.AddDays(10),
                EndDate = baseDate.AddDays(10).AddHours(2)
            };
            _context.Activities.Add(fosteringActivity);
        }

        await _context.SaveChangesAsync();
        return (owner, otherUser, animal1, animal2);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnSuccess_WhenUserHasActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnAllActivities_WhenNoStatusProvided()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.Equal(5, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnAllActivities_WhenStatusIsAll()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "All"
        }, default);

        Assert.Equal(5, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCount_WhenStatusIsActive()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 3,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Active"
        }, default);

        Assert.Equal(3, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOnlyActiveStatus_WhenStatusIsActive()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 3,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Active"
        }, default);

        Assert.All(result.Value!, a => Assert.Equal(ActivityStatus.Active, a.Status));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCount_WhenStatusIsCompleted()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 4,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Completed"
        }, default);

        Assert.Equal(4, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOnlyCompletedStatus_WhenStatusIsCompleted()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 4,
            canceledActivitiesCount: 1);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Completed"
        }, default);

        Assert.All(result.Value!, a => Assert.Equal(ActivityStatus.Completed, a.Status));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCount_WhenStatusIsCanceled()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 3);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Cancelled"
        }, default);

        Assert.Equal(3, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOnlyCanceledStatus_WhenStatusIsCanceled()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 2,
            canceledActivitiesCount: 3);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "Cancelled"
        }, default);

        Assert.All(result.Value!, a => Assert.Equal(ActivityStatus.Cancelled, a.Status));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "InvalidStatus"
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnActivitiesOrderedByStartDateAscending()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 5,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        var activities = result.Value!.ToList();
        for (int i = 0; i < activities.Count - 1; i++)
        {
            Assert.True(activities[i].StartDate <= activities[i + 1].StartDate);
        }
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCount_WhenExcludingFosteringActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 1,
            canceledActivitiesCount: 1,
            addFosteringActivities: true);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.Equal(4, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOnlyOwnershipType_WhenFosteringActivitiesExist()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 1,
            canceledActivitiesCount: 1,
            addFosteringActivities: true);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.All(result.Value!, a => Assert.Equal(ActivityType.Ownership, a.Type));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOnlyCurrentUserActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync();

        // Add activity for other user
        var otherUserActivity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animal1.Id,
            UserId = otherUser.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(5).AddHours(2)
        };
        _context.Activities.Add(otherUserActivity);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.All(result.Value!, a => Assert.Equal(owner.Id, a.UserId));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnSuccess_WhenUserHasNoActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 0,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnZeroCount_WhenUserHasNoActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 0,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.Equal(0, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnEmptyList_WhenUserHasNoActivities()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 0,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectTotalCount_WhenPaginating()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        }, default);

        Assert.Equal(25, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectPageSize_WhenPaginating()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        }, default);

        Assert.Equal(10, result.Value!.Count);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCurrentPage_WhenPaginating()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        }, default);

        Assert.Equal(1, result.Value!.CurrentPage);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectTotalPages_WhenPaginating()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        }, default);

        Assert.Equal(3, result.Value!.TotalPages);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectTotalCount_WhenOnPageTwo()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 2,
            PageSize = 10
        }, default);

        Assert.Equal(25, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectPageSize_WhenOnPageTwo()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 2,
            PageSize = 10
        }, default);

        Assert.Equal(10, result.Value!.Count);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCurrentPage_WhenOnPageTwo()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 25,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = 2,
            PageSize = 10
        }, default);

        Assert.Equal(2, result.Value!.CurrentPage);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldIncludeAnimalData()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.All(result.Value!, a => Assert.NotNull(a.Animal));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldIncludeUserData()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query(), default);

        Assert.All(result.Value!, a => Assert.NotNull(a.User));
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldHandleCaseInsensitiveStatus()
    {
        var (owner, otherUser, animal1, animal2) = await SeedDataAsync(
            activeActivitiesCount: 2,
            completedActivitiesCount: 0,
            canceledActivitiesCount: 0);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(owner.Id);

        var handler = new GetOwnershipActivitiesByUser.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipActivitiesByUser.Query
        {
            Status = "active"
        }, default);

        Assert.True(result.IsSuccess);
    }
}