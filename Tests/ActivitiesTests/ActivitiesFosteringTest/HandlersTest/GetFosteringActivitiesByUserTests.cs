using Application.Activities.Queries;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.ActivitiesTests.ActivitiesFosteringTest.HandlersTest;

/// <summary>
/// Unit tests for the <see cref="GetFosteringActivitiesByUser.Handler"/> class.
///
/// These tests verify the business logic that retrieves a paginated list of
/// fostering activities for the currently authenticated user.
/// The handler is responsible for:
/// - Validating pagination parameters (page number and page size).
/// - Filtering activities by the logged-in user.
/// - Returning only valid fostering activities (not cancelled, correct type, active status).
/// - Excluding activities with missing data (e.g., no slot, slot not reserved, no principal image).
/// - Ordering the returned results by slot start date in ascending order.
///
/// The test suite uses an in-memory EF Core database and mock <see cref="IUserAccessor"/>
/// for isolation and deterministic test outcomes.
/// </summary>
public class GetFosteringActivitiesByUserTests
{
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly AppDbContext _context;
    private readonly GetFosteringActivitiesByUser.Handler _handler;

    /// <summary>
    /// Initializes the test suite by creating an in-memory database and
    /// configuring a mock <see cref="IUserAccessor"/> dependency.
    /// </summary>
    public GetFosteringActivitiesByUserTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        // Setup mock user accessor
        _mockUserAccessor = new Mock<IUserAccessor>();

        // Create _handler
        _handler = new GetFosteringActivitiesByUser.Handler(_context, _mockUserAccessor.Object);
    }

    /// <summary>
    /// Verifies that when valid data exists, the handler returns
    /// a paginated list containing one fostering activity.
    /// </summary>
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsPagedListOfFosteringVisits()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal(activity.Id, result.Value.Items.First().Id);
    }

    /// <summary>
    /// Ensures that when the page number is less than one,
    /// the handler returns a failure result with a 400 code.
    /// </summary>
    [Fact]
    public async Task Handle_WithPageNumberLessThanOne_ReturnsFailure()
    {
        // Arrange
        var user = CreateUser("user1");
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 0,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Page number must be greater than 0", result.Error);
    }

    /// <summary>
    /// Ensures that when the page size is less than one,
    /// the handler returns a validation failure (400).
    /// </summary>
    [Fact]
    public async Task Handle_WithPageSizeLessThanOne_ReturnsFailure()
    {
        // Arrange
        var user = CreateUser("user1");
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 0
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Page size must be between 1 and 50", result.Error);
    }

    /// <summary>
    /// Ensures that when the page size exceeds the maximum (50),
    /// the handler returns a validation failure (400).
    /// </summary>
    [Fact]
    public async Task Handle_WithPageSizeGreaterThan50_ReturnsFailure()
    {
        // Arrange
        var user = CreateUser("user1");
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 51
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Page size must be between 1 and 50", result.Error);
    }

    /// <summary>
    /// Ensures that when there are no future fostering activities,
    /// the handler returns an empty paginated list.
    /// </summary>
    [Fact]
    public async Task Handle_WithNoFutureVisits_ReturnsEmptyList()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var pastDate = DateTime.UtcNow.AddDays(-5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, pastDate);
        var slot = CreateActivitySlot(activity, pastDate, pastDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    /// <summary>
    /// Ensures that cancelled activities are excluded from the results.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancelledActivity_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Cancelled, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that ownership-type activities are excluded,
    /// as only fostering activities are expected.
    /// </summary>
    [Fact]
    public async Task Handle_WithOwnershipActivity_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Ownership, ActivityStatus.Active, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that activities associated with inactive (cancelled)
    /// fosterings are excluded from the returned results.
    /// </summary>
    [Fact]
    public async Task Handle_WithInactiveFostering_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Cancelled);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that activities without an associated slot
    /// are not returned in the result list.
    /// </summary>
    [Fact]
    public async Task Handle_WithActivityWithoutSlot_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate);
        // No slot created

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that activities linked to non-reserved slots
    /// are filtered out.
    /// </summary>
    [Fact]
    public async Task Handle_WithSlotNotReserved_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1), SlotStatus.Available);

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that animals missing a principal image
    /// cause their activities to be excluded.
    /// </summary>
    [Fact]
    public async Task Handle_WithAnimalWithoutPrincipalImage_DoesNotReturnActivity()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed, hasPrincipalImage: false);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);
        var futureDate = DateTime.UtcNow.AddDays(5);
        var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate);
        var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddAsync(activity);
        await _context.ActivitySlots.AddAsync(slot);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }

    /// <summary>
    /// Ensures that multiple fostering visits are ordered
    /// chronologically by slot start date (ascending).
    /// </summary>
    [Fact]
    public async Task Handle_WithMultipleFutureVisits_ReturnsOrderedByDateAscending()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);

        var date1 = DateTime.UtcNow.AddDays(10);
        var date2 = DateTime.UtcNow.AddDays(5);
        var date3 = DateTime.UtcNow.AddDays(15);

        var activity1 = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, date1, "act1");
        var activity2 = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, date2, "act2");
        var activity3 = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, date3, "act3");

        var slot1 = CreateActivitySlot(activity1, date1, date1.AddHours(1));
        var slot2 = CreateActivitySlot(activity2, date2, date2.AddHours(1));
        var slot3 = CreateActivitySlot(activity3, date3, date3.AddHours(1));

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddRangeAsync(activity1, activity2, activity3);
        await _context.ActivitySlots.AddRangeAsync(slot1, slot2, slot3);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal("act2", result.Value.Items.ElementAt(0).Id); // Closest date first
        Assert.Equal("act1", result.Value.Items.ElementAt(1).Id);
        Assert.Equal("act3", result.Value.Items.ElementAt(2).Id); // Furthest date last
    }

    /// <summary>
    /// Ensures that the handler filters results by the currently
    /// authenticated user, excluding activities from other users.
    /// </summary>
    [Fact]
    public async Task Handle_WithDifferentUsers_OnlyReturnsCurrentUserActivities()
    {
        // Arrange
        var user1 = CreateUser("user1");
        var user2 = CreateUser("user2");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering1 = CreateFostering(user1, animal, FosteringStatus.Active);
        var fostering2 = CreateFostering(user2, animal, FosteringStatus.Active);

        var futureDate1 = DateTime.UtcNow.AddDays(5);
        var futureDate2 = DateTime.UtcNow.AddDays(6);

        var activity1 = CreateActivity(user1, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate1, "act1");
        var activity2 = CreateActivity(user2, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate2, "act2");

        var slot1 = CreateActivitySlot(activity1, futureDate1, futureDate1.AddHours(1));
        var slot2 = CreateActivitySlot(activity2, futureDate2, futureDate2.AddHours(1));

        await _context.Users.AddRangeAsync(user1, user2);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddRangeAsync(fostering1, fostering2);
        await _context.Activities.AddRangeAsync(activity1, activity2);
        await _context.ActivitySlots.AddRangeAsync(slot1, slot2);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user1);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal("act1", result.Value!.Items.First().Id);
    }

    /// <summary>
    /// Verifies that pagination is correctly applied to results —
    /// only the requested page is returned with proper ordering.
    /// </summary>
    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var user = CreateUser("user1");
        var shelter = CreateShelter();
        var breed = CreateBreed();
        var animal = CreateAnimal(shelter, breed);
        var fostering = CreateFostering(user, animal, FosteringStatus.Active);

        // Create 5 future activities
        var activities = new List<Activity>();
        var slots = new List<ActivitySlot>();
        for (int i = 1; i <= 5; i++)
        {
            var futureDate = DateTime.UtcNow.AddDays(i);
            var activity = CreateActivity(user, animal, ActivityType.Fostering, ActivityStatus.Active, futureDate, $"act{i}");
            var slot = CreateActivitySlot(activity, futureDate, futureDate.AddHours(1));
            activities.Add(activity);
            slots.Add(slot);
        }

        await _context.Users.AddAsync(user);
        await _context.Shelters.AddAsync(shelter);
        await _context.Breeds.AddAsync(breed);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(fostering);
        await _context.Activities.AddRangeAsync(activities);
        await _context.ActivitySlots.AddRangeAsync(slots);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var query = new GetFosteringActivitiesByUser.Query
        {
            PageNumber = 2,
            PageSize = 2
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.CurrentPage);
        Assert.Equal("act3", result.Value.Items.ElementAt(0).Id);
        Assert.Equal("act4", result.Value.Items.ElementAt(1).Id);
    }

    // Helper methods for creating test entities

    /// <summary>
    /// Creates a new <see cref="User"/> entity with sample data.
    /// </summary>
    /// <param name="id">Unique user identifier.</param>
    private User CreateUser(string id)
    {
        return new User
        {
            Id = id,
            UserName = $"{id}@test.com",
            Email = $"{id}@test.com",
            Name = $"Test User {id}",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Test Street",
            City = "Test City",
            PostalCode = "1234-567",
            PhoneNumber = "912345678"
        };
    }

    /// <summary>
    /// Creates a mock <see cref="Shelter"/> entity with default values.
    /// </summary>
    private Shelter CreateShelter()
    {
        return new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Shelter Street",
            City = "Shelter City",
            PostalCode = "1234-567",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };
    }

    /// <summary>
    /// Creates a mock <see cref="Breed"/> entity with a generic name.
    /// </summary>
    private Breed CreateBreed()
    {
        return new Breed
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Breed"
        };
    }

    /// <summary>
    /// Creates an <see cref="Animal"/> entity with optional principal image.
    /// </summary>
    /// <param name="shelter">The shelter the animal belongs to.</param>
    /// <param name="breed">The breed associated with the animal.</param>
    /// <param name="hasPrincipalImage">Whether to include a principal image.</param>
    private Animal CreateAnimal(Shelter shelter, Breed breed, bool hasPrincipalImage = true)
    {
        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            Sterilized = true,
            Cost = 50,
            ShelterId = shelter.Id,
            Shelter = shelter,
            BreedId = breed.Id,
            Breed = breed
        };

        if (hasPrincipalImage)
        {
            animal.Images.Add(new Image
            {
                Id = Guid.NewGuid().ToString(),
                PublicId = "test-public-id",
                Url = "https://test.com/image.jpg",
                IsPrincipal = true,
                AnimalId = animal.Id,
                Description = "An Image"
            });
        }

        return animal;
    }

    /// <summary>
    /// Creates a <see cref="Fostering"/> entity linking a user and an animal.
    /// </summary>
    /// <param name="user">The fostering user.</param>
    /// <param name="animal">The fostered animal.</param>
    /// <param name="status">Fostering status (active, cancelled, etc.).</param>
    private Fostering CreateFostering(User user, Animal animal, FosteringStatus status)
    {
        return new Fostering
        {
            Id = Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            AnimalId = animal.Id,
            Animal = animal,
            Amount = 25,
            Status = status,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
    }

    /// <summary>
    /// Creates a new <see cref="Activity"/> entity with the specified type and status.
    /// </summary>
    /// <param name="user">Associated user.</param>
    /// <param name="animal">Associated animal.</param>
    /// <param name="type">Activity type (Fostering, Ownership, etc.).</param>
    /// <param name="status">Activity status (Active, Cancelled, etc.).</param>
    /// <param name="startDate">Activity start date.</param>
    /// <param name="id">Optional custom activity ID.</param>
    private Activity CreateActivity(User user, Animal animal, ActivityType type, ActivityStatus status, DateTime startDate, string? id = null)
    {
        return new Activity
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            AnimalId = animal.Id,
            Animal = animal,
            Type = type,
            Status = status,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1)
        };
    }

    /// <summary>
    /// Creates a <see cref="ActivitySlot"/> entity associated with an activity.
    /// </summary>
    /// <param name="activity">Linked activity.</param>
    /// <param name="start">Slot start date/time.</param>
    /// <param name="end">Slot end date/time.</param>
    /// <param name="status">Slot status (Reserved, Available, etc.).</param>
    private ActivitySlot CreateActivitySlot(Activity activity, DateTime start, DateTime end, SlotStatus status = SlotStatus.Reserved)
    {
        var slot = new ActivitySlot
        {
            Id = Guid.NewGuid().ToString(),
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = start,
            EndDateTime = end,
            Status = status,
            Type = SlotType.Activity
        };

        activity.Slot = slot;
        return slot;
    }
}
