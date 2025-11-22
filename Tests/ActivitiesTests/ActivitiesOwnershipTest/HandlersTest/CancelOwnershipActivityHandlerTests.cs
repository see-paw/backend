using Application.Activities.Commands;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ActivitiesTests.ActivitiesOwnershipTest.HandlersTest;

public class CancelOwnershipActivityHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public CancelOwnershipActivityHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<(Activity activity, Animal animal, User user, Shelter shelter)> SeedActivityAsync(
        ActivityStatus status,
        ActivityType type = ActivityType.Ownership,
        string? userId = null)
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
            AnimalState = AnimalState.HasOwner,
            OwnerId = userId ?? Guid.NewGuid().ToString()
        };

        var user = new User
        {
            Id = userId ?? Guid.NewGuid().ToString(),
            Name = "Test User",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "User Street",
            City = "User City",
            PostalCode = "1234-567"
        };

        var activity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animal.Id,
            UserId = user.Id,
            Type = type,
            Status = status,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(3)
        };

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Animals.Add(animal);
        _context.Users.Add(user);
        _context.Activities.Add(activity);

        await _context.SaveChangesAsync();
        return (activity, animal, user, shelter);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        var userId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = Guid.NewGuid().ToString()
        }, default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnForbidden_WhenUserIsNotOwnerOfActivity()
    {
        var (activity, animal, user, shelter) = await SeedActivityAsync(ActivityStatus.Active);

        var differentUserId = Guid.NewGuid().ToString();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(differentUserId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsNotOwnershipType()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Fostering,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsAlreadyCancelled()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Cancelled,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsCompleted()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Completed,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(400, result.Code);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnSuccess_WhenAllConditionsAreMet()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldUpdateStatusToCancelled()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(ActivityStatus.Cancelled, result.Value!.Status);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldPersistChangesInDatabase()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        var updatedActivity = await _context.Activities.FindAsync(activity.Id);
        Assert.Equal(ActivityStatus.Cancelled, updatedActivity!.Status);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnActivityWithAnimalIncluded()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.NotNull(result.Value!.Animal);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnActivityWithUserIncluded()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.NotNull(result.Value!.User);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnCorrectActivityId()
    {
        var userId = Guid.NewGuid().ToString();
        var (activity, animal, user, shelter) = await SeedActivityAsync(
            ActivityStatus.Active,
            ActivityType.Ownership,
            userId);

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(userId);

        var handler = new CancelOwnershipActivity.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new CancelOwnershipActivity.Command
        {
            ActivityId = activity.Id
        }, default);

        Assert.Equal(activity.Id, result.Value!.Id);
    }
}