namespace Tests.Scheduling.Queries;

using Application.Interfaces;
using Application.Scheduling;
using Application.Scheduling.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

/// <summary>
/// Tests for GetAnimalWeeklySchedule.Handler using equivalence class partitioning and boundary value analysis.
/// Focuses on finding defects rather than validating current implementation behavior.
/// </summary>
public class GetAnimalWeeklyScheduleHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<ITimeRangeCalculator> _mockTimeRangeCalculator;
    private readonly Mock<IScheduleAssembler> _mockScheduleAssembler;
    private readonly Mock<ISlotNormalizer> _mockSlotNormalizer;
    private readonly GetAnimalWeeklySchedule.Handler _handler;
    private readonly User _testUser;
    private readonly Shelter _testShelter;

    public GetAnimalWeeklyScheduleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockTimeRangeCalculator = new Mock<ITimeRangeCalculator>();
        _mockScheduleAssembler = new Mock<IScheduleAssembler>();
        _mockSlotNormalizer = new Mock<ISlotNormalizer>();

        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User",
            Email = "test@example.com",
            UserName = "testuser",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Test St",
            City = "Test City",
            PostalCode = "1234-567"
        };

        _testShelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Shelter St",
            City = "Shelter City",
            PostalCode = "1234-567",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        _context.Users.Add(_testUser);
        _context.Shelters.Add(_testShelter);
        _context.SaveChanges();

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(_testUser);

        _handler = new GetAnimalWeeklySchedule.Handler(
            _context,
            _mockUserAccessor.Object,
            _mockTimeRangeCalculator.Object,
            _mockScheduleAssembler.Object,
            _mockSlotNormalizer.Object
        );
    }

    #region Animal Existence - Equivalence Classes

    [Fact]
    public async Task Handle_AnimalDoesNotExist_ReturnsNotFound()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
    }

    [Fact]
    public async Task Handle_AnimalIdEmpty_ReturnsNotFound()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = string.Empty,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Handle_AnimalIdWhitespace_ReturnsNotFound()
    {
        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = "   ",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
    }

    #endregion

    #region Fostering Status - Equivalence Classes

    [Theory]
    [InlineData(FosteringStatus.Cancelled)]
    [InlineData(FosteringStatus.Terminated)]
    public async Task Handle_FosteringNotActive_ReturnsConflict(FosteringStatus status)
    {
        var animal = CreateTestAnimal();
        var fostering = new Fostering
        {
            AnimalId = animal.Id,
            UserId = _testUser.Id,
            Amount = 50m,
            Status = status,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
        Assert.Equal("Animal not fostered by user", result.Error);
    }

    [Fact]
    public async Task Handle_NoFosteringExists_ReturnsConflict()
    {
        var animal = CreateTestAnimal();
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
    }

    [Fact]
    public async Task Handle_FosteringByDifferentUser_ReturnsConflict()
    {
        var otherUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other User",
            Email = "other@example.com",
            UserName = "otheruser",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Other St",
            City = "Other City",
            PostalCode = "9876-543"
        };
        _context.Users.Add(otherUser);

        var animal = CreateTestAnimal();
        var fostering = new Fostering
        {
            AnimalId = animal.Id,
            UserId = otherUser.Id,
            Amount = 50m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
    }

    #endregion

    #region Shelter Hours - Boundary Value Analysis

    [Fact]
    public async Task Handle_OpeningEqualsClosing_ThrowsArgumentException()
    {
        var animal = CreateTestAnimal();
        animal.Shelter.OpeningTime = new TimeOnly(9, 0);
        animal.Shelter.ClosingTime = new TimeOnly(9, 0);

        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_OpeningAfterClosing_ThrowsArgumentException()
    {
        var animal = CreateTestAnimal();
        animal.Shelter.OpeningTime = new TimeOnly(18, 0);
        animal.Shelter.ClosingTime = new TimeOnly(9, 0);

        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 1)]
    [InlineData(0, 0, 23, 59, 59)]
    [InlineData(23, 59, 23, 59, 59)]
    public async Task Handle_ShelterHoursBoundaries_ProcessesCorrectly(
        int openHour, int openMin, int closeHour, int closeMin, int closeSec)
    {
        var animal = CreateTestAnimal();
        animal.Shelter.OpeningTime = new TimeOnly(openHour, openMin);
        animal.Shelter.ClosingTime = new TimeOnly(closeHour, closeMin, closeSec);

        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Date Range - Boundary Value Analysis

    [Fact]
    public async Task Handle_StartDateMinValue_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.MinValue
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_StartDateMaxValue_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.MaxValue.AddDays(-7)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_StartDateCausesOverflow_ThrowsOrHandlesGracefully()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.MaxValue
        };

        await Assert.ThrowsAnyAsync<Exception>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task Handle_VariousStartDateOffsets_ProcessesCorrectly(int daysOffset)
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysOffset))
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Slot Filtering - Equivalence Classes

    [Theory]
    [InlineData(ActivityStatus.Cancelled)]
    [InlineData(ActivityStatus.Completed)]
    public async Task Handle_NonActiveActivitySlots_AreNotIncluded(ActivityStatus status)
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var activity = new Activity
        {
            AnimalId = animal.Id,
            UserId = _testUser.Id,
            Type = ActivityType.Fostering,
            Status = status,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(2)
        };
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.OfType<ActivitySlot>().Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Theory]
    [InlineData(SlotStatus.Available)]
    [InlineData(SlotStatus.Reserved)]
    public async Task Handle_NonUnavailableShelterSlots_AreNotIncluded(SlotStatus status)
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var unavailabilitySlot = new ShelterUnavailabilitySlot
        {
            ShelterId = animal.ShelterId,
            Reason = "Test",
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = status,
            Type = SlotType.ShelterUnavailable
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Set<ShelterUnavailabilitySlot>().Add(unavailabilitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.OfType<ShelterUnavailabilitySlot>().Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region Slot Time Window - Boundary Value Analysis

    [Fact]
    public async Task Handle_SlotStartsExactlyAtWeekEnd_IsExcluded()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekEnd = startDate.AddDays(7).ToDateTime(TimeOnly.MinValue);

        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = weekEnd,
            EndDateTime = weekEnd.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = startDate
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SlotEndsExactlyAtWeekStart_IsExcluded()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = startDate.ToDateTime(TimeOnly.MinValue);

        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = weekStart.AddHours(-2),
            EndDateTime = weekStart,
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = startDate
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SlotOverlapsWeekStart_IsIncluded()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = startDate.ToDateTime(TimeOnly.MinValue);

        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = weekStart.AddHours(-1),
            EndDateTime = weekStart.AddHours(1),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = startDate
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => slots.Count() == 1),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SlotOverlapsWeekEnd_IsIncluded()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekEnd = startDate.AddDays(7).ToDateTime(TimeOnly.MinValue);

        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = weekEnd.AddHours(-1),
            EndDateTime = weekEnd.AddHours(1),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = startDate
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => slots.Count() == 1),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region Multiple Slots - Equivalence Classes

    [Fact]
    public async Task Handle_NoSlots_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OnlyActivitySlots_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(
                    slots => slots.Count() == 1 && slots.All(s => s is ActivitySlot)),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OnlyUnavailabilitySlots_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var unavailabilitySlot = new ShelterUnavailabilitySlot
        {
            ShelterId = animal.ShelterId,
            Reason = "Test",
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Unavailable,
            Type = SlotType.ShelterUnavailable
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Set<ShelterUnavailabilitySlot>().Add(unavailabilitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(
                    slots => slots.Count() == 1 && slots.All(s => s is ShelterUnavailabilitySlot)),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MixedSlotTypes_ProcessesCorrectly()
    {
        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var activity = CreateActivity(animal.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };
        var unavailabilitySlot = new ShelterUnavailabilitySlot
        {
            ShelterId = animal.ShelterId,
            Reason = "Test",
            StartDateTime = DateTime.UtcNow.AddHours(3),
            EndDateTime = DateTime.UtcNow.AddHours(4),
            Status = SlotStatus.Unavailable,
            Type = SlotType.ShelterUnavailable
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        _context.Set<ShelterUnavailabilitySlot>().Add(unavailabilitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => slots.Count() == 2),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region Different Animal Slots - Equivalence Classes

    [Fact]
    public async Task Handle_SlotsForDifferentAnimal_AreNotIncluded()
    {
        var animal1 = CreateTestAnimal();
        var animal2 = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal1.Id);
        
        var activity = CreateActivity(animal2.Id);
        var activitySlot = new ActivitySlot
        {
            ActivityId = activity.Id,
            Activity = activity,
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        _context.Animals.AddRange(animal1, animal2);
        _context.Fosterings.Add(fostering);
        _context.Activities.Add(activity);
        _context.Set<ActivitySlot>().Add(activitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal1.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UnavailabilitySlotsForDifferentShelter_AreNotIncluded()
    {
        var otherShelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Other Shelter",
            Street = "Other St",
            City = "Other City",
            PostalCode = "9876-543",
            Phone = "923456789",
            NIF = "987654321",
            OpeningTime = new TimeOnly(8, 0),
            ClosingTime = new TimeOnly(20, 0)
        };
        _context.Shelters.Add(otherShelter);

        var animal = CreateTestAnimal();
        var fostering = CreateActiveFostering(animal.Id);
        var unavailabilitySlot = new ShelterUnavailabilitySlot
        {
            ShelterId = otherShelter.Id,
            Reason = "Test",
            StartDateTime = DateTime.UtcNow.AddHours(1),
            EndDateTime = DateTime.UtcNow.AddHours(2),
            Status = SlotStatus.Unavailable,
            Type = SlotType.ShelterUnavailable
        };

        _context.Animals.Add(animal);
        _context.Fosterings.Add(fostering);
        _context.Set<ShelterUnavailabilitySlot>().Add(unavailabilitySlot);
        await _context.SaveChangesAsync();

        SetupMocksForSuccessfulCall();

        var query = new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animal.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        await _handler.Handle(query, CancellationToken.None);

        _mockSlotNormalizer.Verify(
            x => x.Normalize(
                It.Is<IEnumerable<Slot>>(slots => !slots.Any()),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private Animal CreateTestAnimal()
    {
        return new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            Sterilized = true,
            Cost = 50m,
            ShelterId = _testShelter.Id,
            Shelter = _testShelter,
            BreedId = Guid.NewGuid().ToString()
        };
    }

    private Fostering CreateActiveFostering(string animalId)
    {
        return new Fostering
        {
            AnimalId = animalId,
            UserId = _testUser.Id,
            Amount = 50m,
            Status = FosteringStatus.Active,
            StartDate = DateTime.UtcNow.AddMonths(-1)
        };
    }

    private Activity CreateActivity(string animalId)
    {
        return new Activity
        {
            AnimalId = animalId,
            UserId = _testUser.Id,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddHours(2)
        };
    }

    private void SetupMocksForSuccessfulCall()
    {
        _mockSlotNormalizer
            .Setup(x => x.Normalize(
                It.IsAny<IEnumerable<Slot>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()))
            .Returns(new NormalizedSlots(new List<Slot>()));

        _mockTimeRangeCalculator
            .Setup(x => x.CalculateWeeklyAvailableRanges(
                It.IsAny<IEnumerable<Slot>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<DateOnly>()))
            .Returns(new List<TimeBlock>());

        _mockScheduleAssembler
            .Setup(x => x.AssembleWeekSchedule(
                It.IsAny<IReadOnlyList<ActivitySlot>>(),
                It.IsAny<IReadOnlyList<ShelterUnavailabilitySlot>>(),
                It.IsAny<IReadOnlyList<TimeBlock>>(),
                It.IsAny<Animal>(),
                It.IsAny<DateOnly>()))
            .Returns(new AnimalWeeklySchedule
            {
                Animal = new Animal
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Test",
                    BirthDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    Sterilized = false,
                    Cost = 0,
                    ShelterId = _testShelter.Id,
                    BreedId = Guid.NewGuid().ToString()
                },
                Shelter = _testShelter,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
            });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #endregion
}