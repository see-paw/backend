using Application.Activities.Commands;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;


namespace Tests.ActivitiesTest.ActivitiesFosteringTest.HandlersTest;

/// <summary>
/// Unit test suite for <see cref="CreateFosteringActivity.Handler"/>,
/// ensuring correct validation and creation flow for fostering activity scheduling.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Correct validation of date ranges and time restrictions.</description></item>
/// <item><description>Proper entity creation and relationship mapping.</description></item>
/// <item><description>Conflict, time, and authorization scenarios.</description></item>
/// <item><description>Accurate setting of <see cref="Activity"/>, <see cref="ActivitySlot"/>, and related entities.</description></item>
/// </list>
/// Uses EF Core’s <see cref="DbContextOptionsBuilder.UseInMemoryDatabase(string)"/> for deterministic isolation.
/// </remarks>
public class CreateFosteringActivityTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly AppDbContext _context;
    private readonly CreateFosteringActivity.Handler _handler;

    /// <summary>
    /// Initializes the in-memory database context, 
    /// mocked dependencies, and handler instance.
    /// </summary>
    public CreateFosteringActivityTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _userAccessorMock = new Mock<IUserAccessor>();
        _handler = new CreateFosteringActivity.Handler(_context, _userAccessorMock.Object);
    }

    /// <summary>
    /// Seeds the test database with baseline entities:
    /// one user, one shelter, one breed, one animal in fostering state,
    /// and an active fostering relationship.
    /// </summary>
    private async Task SeedBasicTestData()
    {
        var user = new User
        {
            Id = "user-001",
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test User"
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

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validates that a well-formed request creates a fostering activity successfully,
    /// returning <c>201 Created</c> and setting all required relationships.
    /// </summary>
    [Fact]
    public async Task Handle_WithValidRequest_CreatesActivitySuccessfully()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
        Assert.NotNull(result.Value.Activity);
        Assert.NotNull(result.Value.ActivitySlot);
        Assert.Equal("animal-001", result.Value.Activity.AnimalId);
        Assert.Equal(ActivityStatus.Active, result.Value.Activity.Status);
        Assert.Equal(SlotStatus.Reserved, result.Value.ActivitySlot.Status);

        var savedActivity = await _context.Activities.FirstOrDefaultAsync();
        Assert.NotNull(savedActivity);
        Assert.Equal(ActivityType.Fostering, savedActivity.Type);
    }

    /// <summary>
    /// Ensures that scheduling within less than 24 hours 
    /// results in a <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithStartTimeLessThan24HoursAhead_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddHours(12); // Less than 24 hours
        var endTime = DateTime.UtcNow.AddHours(14);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot schedule an activity before", result.Error);
    }

    /// <summary>
    /// Verifies that an end time before the start time triggers a <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithEndTimeBeforeStartTime_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(14);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12); // Before start time

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("cannot be before", result.Error);
    }

    /// <summary>
    /// Ensures that scheduling an activity in the past returns a <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithStartTimeInPast_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddHours(-2); // In the past
        var endTime = DateTime.UtcNow.AddHours(2);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot schedule an activity before", result.Error);
    }

    /// <summary>
    /// Ensures that an end time less than 24 hours ahead is rejected with a <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithEndTimeLessThan24HoursAhead_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2);
        var endTime = DateTime.UtcNow.AddHours(12); // Less than 24 hours

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot schedule an activity before", result.Error);
    }

    /// <summary>
    /// Verifies that scheduling for a non-existent animal returns a <c>404 Not Found</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonExistentAnimal_ReturnsNotFound()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "non-existent-id",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
    }

    /// <summary>
    /// Ensures that activities cannot be scheduled for inactive animals.
    /// </summary>
    [Fact]
    public async Task Handle_WithInactiveAnimal_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var animal = await _context.Animals.FindAsync("animal-001");
        animal!.AnimalState = AnimalState.Inactive;
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Animal cannot be visited", result.Error);
    }

    /// <summary>
    /// Ensures that animals available for adoption cannot be scheduled for fostering visits.
    /// </summary>
    [Fact]
    public async Task Handle_WithAvailableAnimal_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var animal = await _context.Animals.FindAsync("animal-001");
        animal!.AnimalState = AnimalState.Available;
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Animal cannot be visited", result.Error);
    }

    /// <summary>
    /// Ensures that animals already adopted cannot have fostering activities scheduled.
    /// </summary>
    [Fact]
    public async Task Handle_WithAnimalHavingOwner_ReturnsBadRequest()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var animal = await _context.Animals.FindAsync("animal-001");
        animal!.AnimalState = AnimalState.HasOwner;
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Animal cannot be visited", result.Error);
    }

    /// <summary>
    /// Ensures that users without an active fostering relationship 
    /// cannot schedule activities, returning <c>404</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithoutActiveFostering_ReturnsNotFound()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var fostering = await _context.Fosterings.FindAsync("fostering-001");
        fostering!.Status = FosteringStatus.Cancelled;
        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("You are not currently fostering this animal", result.Error);
    }

    /// <summary>
    /// Ensures scheduling before shelter opening hours (09:00) returns <c>422 Unprocessable Entity</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithStartBeforeShelterOpening_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var tomorrow = DateTime.UtcNow.Date.AddDays(2);
        var startTime = tomorrow.AddHours(8); // Before 9:00 opening
        var endTime = tomorrow.AddHours(10);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(422, result.Code);
        Assert.Contains("opening time", result.Error);
    }

    /// <summary>
    /// Ensures scheduling after shelter closing hours (18:00) returns <c>422 Unprocessable Entity</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithEndAfterShelterClosing_ReturnsUnprocessableEntity()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var tomorrow = DateTime.UtcNow.Date.AddDays(2);
        var startTime = tomorrow.AddHours(16);
        var endTime = tomorrow.AddHours(19); // After 18:00 closing

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(422, result.Code);
        Assert.Contains("closing time", result.Error);
    }

    /// <summary>
    /// Ensures that overlapping with shelter unavailability slots 
    /// returns <c>409 Conflict</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithShelterUnavailability_ReturnsConflict()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var tomorrow = DateTime.UtcNow.Date.AddDays(2);
        var unavailabilitySlot = new ShelterUnavailabilitySlot
        {
            Id = "unavail-001",
            ShelterId = "shelter-001",
            StartDateTime = tomorrow.AddHours(14),
            EndDateTime = tomorrow.AddHours(16),
            Status = SlotStatus.Reserved,
            Type = SlotType.ShelterUnavailable,
            Reason = "Maintenance"
        };
        await _context.ShelterUnavailabilitySlots.AddAsync(unavailabilitySlot);
        await _context.SaveChangesAsync();

        var startTime = tomorrow.AddHours(14).AddMinutes(30);
        var endTime = tomorrow.AddHours(15).AddMinutes(30);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
        Assert.Contains("Shelter is unavailable", result.Error);
    }

    /// <summary>
    /// Ensures that an overlap with another activity slot 
    /// returns <c>409 Conflict</c> with proper message.
    /// </summary>
    [Fact]
    public async Task Handle_WithOverlappingActivitySlot_ReturnsConflict()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var tomorrow = DateTime.UtcNow.Date.AddDays(2);

        var existingActivity = new Activity
        {
            Id = "activity-existing",
            AnimalId = "animal-001",
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = tomorrow.AddHours(10),
            EndDate = tomorrow.AddHours(12)
        };
        await _context.Activities.AddAsync(existingActivity);

        var existingSlot = new ActivitySlot
        {
            Id = "slot-existing",
            ActivityId = existingActivity.Id,
            StartDateTime = tomorrow.AddHours(10),
            EndDateTime = tomorrow.AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };
        await _context.ActivitySlots.AddAsync(existingSlot);
        await _context.SaveChangesAsync();

        var startTime = tomorrow.AddHours(11);
        var endTime = tomorrow.AddHours(13);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
        Assert.Contains("another visit scheduled", result.Error);
    }

    /// <summary>
    /// Ensures that overlap with another active fostering activity 
    /// returns <c>409 Conflict</c>.
    /// </summary>
    [Fact]
    public async Task Handle_WithOverlappingActivity_ReturnsConflict()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var tomorrow = DateTime.UtcNow.Date.AddDays(2);

        var existingActivity = new Activity
        {
            Id = "activity-existing",
            AnimalId = "animal-001",
            UserId = user.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = tomorrow.AddHours(10),
            EndDate = tomorrow.AddHours(14)
        };
        await _context.Activities.AddAsync(existingActivity);
        await _context.SaveChangesAsync();

        var startTime = tomorrow.AddHours(12);
        var endTime = tomorrow.AddHours(16);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
        Assert.Contains("another activity scheduled", result.Error);
    }

    /// <summary>
    /// Verifies that non-UTC datetimes are automatically converted to UTC before persistence.
    /// </summary>
    [Fact]
    public async Task Handle_ConvertsNonUtcToUtc()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var localStart = DateTime.Now.AddDays(2).AddHours(10);
        var localEnd = DateTime.Now.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = localStart,
            EndDateTime = localEnd
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeKind.Utc, result.Value.Activity.StartDate.Kind);
        Assert.Equal(DateTimeKind.Utc, result.Value.Activity.EndDate.Kind);
    }

    /// <summary>
    /// Ensures that UTC datetimes remain unchanged when already correctly set.
    /// </summary>
    [Fact]
    public async Task Handle_DoesNotConvertUtcDates()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var utcStart = DateTime.UtcNow.AddDays(2).AddHours(10);
        var utcEnd = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = utcStart,
            EndDateTime = utcEnd
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(utcStart, result.Value.Activity.StartDate);
        Assert.Equal(utcEnd, result.Value.Activity.EndDate);
    }

    /// <summary>
    /// Verifies that all related entities (Activity, Slot, Animal, Shelter)
    /// are properly included in the handler response.
    /// </summary>
    [Fact]
    public async Task Handle_ReturnsAllRequiredEntities()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Activity);
        Assert.NotNull(result.Value.ActivitySlot);
        Assert.NotNull(result.Value.Animal);
        Assert.NotNull(result.Value.Shelter);
        Assert.Equal("animal-001", result.Value.Animal.Id);
        Assert.Equal("shelter-001", result.Value.Shelter.Id);
    }

    /// <summary>
    /// Ensures that all <see cref="Activity"/> properties are set correctly after creation.
    /// </summary>
    [Fact]
    public async Task Handle_SetsCorrectActivityProperties()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ActivityType.Fostering, result.Value.Activity.Type);
        Assert.Equal(ActivityStatus.Active, result.Value.Activity.Status);
        Assert.Equal("user-001", result.Value.Activity.UserId);
        Assert.Equal("animal-001", result.Value.Activity.AnimalId);
    }

    /// <summary>
    /// Ensures that all <see cref="ActivitySlot"/> properties are correctly assigned and linked.
    /// </summary>
    [Fact]
    public async Task Handle_SetsCorrectSlotProperties()
    {
        // Arrange
        await SeedBasicTestData();

        var user = new User { Id = "user-001" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var command = new CreateFosteringActivity.Command
        {
            AnimalId = "animal-001",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(SlotStatus.Reserved, result.Value.ActivitySlot.Status);
        Assert.Equal(SlotType.Activity, result.Value.ActivitySlot.Type);
        Assert.Equal(result.Value.Activity.Id, result.Value.ActivitySlot.ActivityId);
    }
}