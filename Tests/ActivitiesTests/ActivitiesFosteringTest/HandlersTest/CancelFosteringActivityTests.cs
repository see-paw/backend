using Application.Activities.Commands;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ActivitiesTests.ActivitiesFosteringTest.HandlersTest;

/// <summary>
/// Unit test suite for <see cref="CancelFosteringActivity.Handler"/>, 
/// responsible for validating and executing the cancellation of fostering activity visits.
/// </summary>
/// <remarks>
/// These tests ensure that the handler:
/// <list type="bullet">
/// <item><description>Properly validates authorization and ownership rules.</description></item>
/// <item><description>Correctly updates the <see cref="Activity"/> and <see cref="ActivitySlot"/> statuses.</description></item>
/// <item><description>Handles all relevant error cases (404, 403, 400).</description></item>
/// <item><description>Maintains data integrity and does not affect unrelated records.</description></item>
/// </list>
/// Uses EF Core’s <see cref="DbContextOptionsBuilder.UseInMemoryDatabase(string)"/> for isolation and deterministic results.
/// </remarks>
public class CancelFosteringActivityTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly AppDbContext _context;
    private readonly CancelFosteringActivity.Handler _handler;

    /// <summary>
    /// Initializes a new test instance, using an in-memory EF Core database and a mocked user accessor.
    /// </summary>
    public CancelFosteringActivityTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _userAccessorMock = new Mock<IUserAccessor>();
        _handler = new CancelFosteringActivity.Handler(_context, _userAccessorMock.Object);
    }

    /// <summary>
    /// Seeds reusable test data, including:
    /// - A fostering user and another user
    /// - Shelter, breed, and animal
    /// - Active fostering record
    /// - Active fostering activity with reserved slot
    /// </summary>
    private async Task SeedTestData()
    {
        var user = new User
        {
            Id = "user-001",
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test User"
        };

        var otherUser = new User
        {
            Id = "user-002",
            UserName = "other@test.com",
            Email = "other@test.com",
            Name = "Other User"
        };

        var shelter = new Shelter
        {
            Id = "shelter-001",
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Porto",
            PostalCode = "4000-001",
            Phone = "223456789",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var breed = new Breed
        {
            Id = "breed-001",
            Name = "Test Breed"
        };

        var animal = new Animal
        {
            Id = "animal-001",
            Name = "Rex",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id
        };

        var fostering = new Fostering
        {
            Id = "fostering-001",
            AnimalId = animal.Id,
            UserId = user.Id,
            Amount = 50.00m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        var futureStartTime = DateTime.UtcNow.AddDays(2);
        var futureEndTime = futureStartTime.AddHours(2);

        var activity = new Activity
        {
            Id = "activity-001",
            AnimalId = animal.Id,
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = futureStartTime,
            EndDate = futureEndTime
        };

        var activitySlot = new ActivitySlot
        {
            Id = "slot-001",
            ActivityId = activity.Id,
            StartDateTime = futureStartTime,
            EndDateTime = futureEndTime,
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        await _context.Users.AddRangeAsync(user, otherUser);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(activitySlot);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Ensures that cancelling a valid fostering activity updates
    /// the activity and slot states successfully, returning a <c>200 OK</c> result.
    /// </summary>
    [Fact]
    public async Task Handle_WithValidRequest_CancelsActivitySuccessfully()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.Code);
        if (result.Value != null)
        {
            Assert.Equal("activity-001", result.Value.ActivityId);
            Assert.Equal("Visit cancelled successfully", result.Value.Message);
        }

        var activity = await _context.Activities.FindAsync("activity-001");
        Assert.Equal(ActivityStatus.Cancelled, activity!.Status);

        var slot = await _context.ActivitySlots.FindAsync("slot-001");
        Assert.Equal(SlotStatus.Available, slot!.Status);
        Assert.NotNull(slot.UpdatedAt);
    }

    /// <summary>
    /// Verifies that attempting to cancel a non-existent activity 
    /// returns a <c>404 Not Found</c> response with an appropriate error message.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonExistentActivity_ReturnsNotFound()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "non-existent-id"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Activity not found", result.Error);
    }

    /// <summary>
    /// Ensures that a user cannot cancel another user's activity,
    /// returning a <c>403 Forbidden</c> result.
    /// </summary>
    [Fact]
    public async Task Handle_WithActivityNotBelongingToUser_ReturnsForbidden()
    {
        // Arrange
        await SeedTestData();

        var otherUser = new User { Id = "user-002" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(otherUser);

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Equal("You are not authorized to cancel this activity", result.Error);
    }

    /// <summary>
    /// Verifies that only fostering-type activities can be cancelled through this handler,
    /// returning a <c>400 Bad Request</c> otherwise.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonFosteringActivity_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var futureStartTime = DateTime.UtcNow.AddDays(2);
        var ownershipActivity = new Activity
        {
            Id = "activity-ownership",
            AnimalId = "animal-001",
            UserId = user.Id,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = futureStartTime,
            EndDate = futureStartTime.AddMonths(1)
        };

        var ownershipSlot = new ActivitySlot
        {
            Id = "slot-ownership",
            ActivityId = "activity-ownership",
            StartDateTime = futureStartTime,
            EndDateTime = futureStartTime.AddMonths(1),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        await _context.Activities.AddAsync(ownershipActivity);
        await _context.ActivitySlots.AddAsync(ownershipSlot);
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-ownership"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Only fostering activities", result.Error);
    }

    /// <summary>
    /// Ensures that attempting to cancel an already cancelled activity 
    /// returns <c>400 Bad Request</c> with a descriptive message.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancelledActivity_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var activity = await _context.Activities.FindAsync("activity-001");
        activity!.Status = ActivityStatus.Cancelled;
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot cancel an activity with status 'Cancelled'", result.Error);
    }

    /// <summary>
    /// Ensures that completed activities cannot be cancelled,
    /// returning <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithCompletedActivity_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var activity = await _context.Activities.FindAsync("activity-001");
        activity!.Status = ActivityStatus.Completed;
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot cancel an activity with status 'Completed'", result.Error);
    }

    /// <summary>
    /// Verifies that cancellation fails if the user no longer has an active fostering relationship
    /// with the animal, returning <c>403 Forbidden</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithoutActiveFostering_ReturnsForbidden()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var fostering = await _context.Fosterings.FindAsync("fostering-001");
        fostering!.Status = FosteringStatus.Cancelled;
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.Code);
        Assert.Equal("You no longer have an active fostering relationship with this animal", result.Error);
    }

    /// <summary>
    /// Ensures that if the associated activity slot does not exist,
    /// the handler returns <c>404 Not Found</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithMissingSlot_ReturnsNotFound()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        // Remove the slot
        var slot = await _context.ActivitySlots.FindAsync("slot-001");
        _context.ActivitySlots.Remove(slot!);
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Activity slot not found", result.Error);
    }

    /// <summary>
    /// Ensures that activities linked to already available slots 
    /// cannot be cancelled, returning <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithAvailableSlot_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var slot = await _context.ActivitySlots.FindAsync("slot-001");
        slot!.Status = SlotStatus.Available;
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot cancel a slot with status 'Available'", result.Error);
    }

    /// <summary>
    /// Ensures that cancelling a slot marked as unavailable returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithUnavailableSlot_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var slot = await _context.ActivitySlots.FindAsync("slot-001");
        slot!.Status = SlotStatus.Unavailable;
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot cancel a slot with status 'Unavailable'", result.Error);
    }

    /// <summary>
    /// Verifies that attempting to cancel a past activity (already started or finished)
    /// returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithPastActivity_ReturnsBadRequest()
    {
        // Arrange
        var user = new User
        {
            Id = "user-past",
            UserName = "past@test.com",
            Email = "past@test.com",
            Name = "Past User"
        };

        var shelter = new Shelter
        {
            Id = "shelter-past",
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Porto",
            PostalCode = "4000-001",
            Phone = "223456789",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var breed = new Breed
        {
            Id = "breed-past",
            Name = "Test Breed"
        };

        var animal = new Animal
        {
            Id = "animal-past",
            Name = "Past Rex",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id
        };

        var fostering = new Fostering
        {
            Id = "fostering-past",
            AnimalId = animal.Id,
            UserId = user.Id,
            Amount = 50.00m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        var pastStartTime = DateTime.UtcNow.AddHours(-1);
        var pastActivity = new Activity
        {
            Id = "activity-past",
            AnimalId = animal.Id,
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = pastStartTime,
            EndDate = pastStartTime.AddHours(2)
        };

        var pastSlot = new ActivitySlot
        {
            Id = "slot-past",
            ActivityId = "activity-past",
            StartDateTime = pastStartTime,
            EndDateTime = pastStartTime.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(pastActivity);
        await _context.ActivitySlots.AddAsync(pastSlot);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-past"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Cannot cancel an activity that has already started or passed", result.Error);
    }

    /// <summary>
    /// Ensures that cancelling an activity that starts "now" also fails with <c>400 Bad Request</c>,
    /// maintaining consistent temporal validation.
    /// </summary>
    [Fact]
    public async Task Handle_WithActivityStartingNow_ReturnsBadRequest()
    {
        // Arrange
        var user = new User
        {
            Id = "user-now",
            UserName = "now@test.com",
            Email = "now@test.com",
            Name = "Now User"
        };

        var shelter = new Shelter
        {
            Id = "shelter-now",
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Porto",
            PostalCode = "4000-001",
            Phone = "223456789",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var breed = new Breed
        {
            Id = "breed-now",
            Name = "Test Breed"
        };

        var animal = new Animal
        {
            Id = "animal-now",
            Name = "Now Rex",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50.00m,
            ShelterId = shelter.Id,
            BreedId = breed.Id
        };

        var fostering = new Fostering
        {
            Id = "fostering-now",
            AnimalId = animal.Id,
            UserId = user.Id,
            Amount = 50.00m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        var nowStartTime = DateTime.UtcNow;
        var nowActivity = new Activity
        {
            Id = "activity-now",
            AnimalId = animal.Id,
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = nowStartTime,
            EndDate = nowStartTime.AddHours(2)
        };

        var nowSlot = new ActivitySlot
        {
            Id = "slot-now",
            ActivityId = "activity-now",
            StartDateTime = nowStartTime,
            EndDateTime = nowStartTime.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(nowActivity);
        await _context.ActivitySlots.AddAsync(nowSlot);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-now"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Cannot cancel an activity that has already started or passed", result.Error);
    }

    /// <summary>
    /// Verifies that cancelling an activity updates the <see cref="ActivitySlot.UpdatedAt"/> timestamp correctly.
    /// </summary>
    [Fact]
    public async Task Handle_UpdatesSlotUpdatedAt()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var slotBefore = await _context.ActivitySlots.FindAsync("slot-001");
        var updatedAtBefore = slotBefore!.UpdatedAt;

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var slotAfter = await _context.ActivitySlots.FindAsync("slot-001");
        Assert.NotNull(slotAfter!.UpdatedAt);
        Assert.NotEqual(updatedAtBefore, slotAfter.UpdatedAt);
    }

    /// <summary>
    /// Ensures that cancelling one activity does not affect 
    /// the status of other unrelated activities in the database.
    /// </summary>
    [Fact]
    public async Task Handle_DoesNotModifyOtherActivities()
    {
        // Arrange
        await SeedTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var futureStartTime = DateTime.UtcNow.AddDays(3);
        var otherActivity = new Activity
        {
            Id = "activity-002",
            AnimalId = "animal-001",
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = futureStartTime,
            EndDate = futureStartTime.AddHours(2)
        };

        var otherSlot = new ActivitySlot
        {
            Id = "slot-002",
            ActivityId = "activity-002",
            StartDateTime = futureStartTime,
            EndDateTime = futureStartTime.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        await _context.Activities.AddAsync(otherActivity);
        await _context.ActivitySlots.AddAsync(otherSlot);
        await _context.SaveChangesAsync();

        var command = new CancelFosteringActivity.Command
        {
            ActivityId = "activity-001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var otherActivityAfter = await _context.Activities.FindAsync("activity-002");
        Assert.Equal(ActivityStatus.Active, otherActivityAfter!.Status);
    }
}