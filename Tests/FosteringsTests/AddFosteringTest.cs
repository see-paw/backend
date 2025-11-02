using Application.Fosterings.Commands;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Enums;
using Domain.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.FosteringsTests;

/// <summary>
/// Test suite for AddFostering command handler using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests are designed to identify failures and edge cases rather than just pass validation.
/// Each test uses an isolated SQLite in-memory database to support real transactions.
/// </summary>
public class AddFosteringTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly FosteringDomainService _fosteringDomainService;
    private readonly IFosteringService _fosteringService;
    private readonly AddFostering.Handler _handler;
    private const decimal MinMonthlyValue = 10.00m;
    private const string ValidUserId = "user-123";
    private const string ValidAnimalId = "animal-456";
    
    public AddFosteringTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        
        _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        
        _userAccessorMock = new Mock<IUserAccessor>();
        _fosteringDomainService = new FosteringDomainService();
        _fosteringService = new FosteringService(_fosteringDomainService);
        _handler = new AddFostering.Handler(_context, _fosteringDomainService, _fosteringService, _userAccessorMock.Object);
        
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(new User { Id = ValidUserId, Name = "Test User", Email = "test@test.com" });
    }

    #region Animal Not Found Tests

    /// <summary>
    /// EC: AnimalId does not exist in database
    /// Expected: 404 failure
    /// </summary>
    [Fact]
    public async Task Handle_NonExistentAnimalId_ReturnsNotFound()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "non-existent-id",
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Animal not found", result.Error);
    }

    #endregion

    #region Animal State Tests

    /// <summary>
    /// EC: Invalid animal states for fostering
    /// Expected: 409 conflict for Inactive, TotallyFostered, HasOwner states
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Inactive, "Animal is inactive", 409)]
    [InlineData(AnimalState.TotallyFostered, "Animal is totally fostered", 409)]
    [InlineData(AnimalState.HasOwner, "Animal has an owner, not available for fostering", 409)]
    public async Task Handle_InvalidAnimalState_ReturnsConflict(AnimalState state, string expectedMessage, int expectedCode)
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: state);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.Code);
        Assert.Equal(expectedMessage, result.Error);
    }

    /// <summary>
    /// EC: Valid animal states for fostering
    /// BVA: Testing with minimum valid monthly value
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    public async Task Handle_ValidAnimalStates_CreatesSuccessfully(AnimalState state)
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: state);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = MinMonthlyValue
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
    }

    #endregion
    
    #region Animal Cost Boundary Tests

    /// <summary>
    /// BVA: MonthValue just below remaining cost
    /// Expected: Should succeed and animal remains PartiallyFostered
    /// </summary>
    [Fact]
    public async Task Handle_MonthValueBelowRemainingCost_CreatesPartialFostering()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var existingFostering = CreateFostering("fostering-1", animal.Id, "other-user", 50m);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 49.99m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.PartiallyFostered, updatedAnimal.AnimalState);
    }

    /// <summary>
    /// BVA: MonthValue exactly matches remaining cost
    /// Expected: Should succeed and animal becomes TotallyFostered
    /// </summary>
    [Fact]
    public async Task Handle_MonthValueExactlyRemainingCost_UpdatesToTotallyFostered()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var existingFostering = CreateFostering("fostering-1", animal.Id, "other-user", 50m);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 50m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.TotallyFostered, updatedAnimal.AnimalState);
    }

    /// <summary>
    /// BVA: MonthValue exceeds remaining cost by minimal amount
    /// Expected: Should fail with 422
    /// </summary>
    [Fact]
    public async Task Handle_MonthValueExceedsRemainingCost_ReturnsUnprocessableEntity()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var existingFostering = CreateFostering("fostering-1", animal.Id, "other-user", 50m);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 50.01m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(422, result.Code);
        Assert.Equal("Monthly value surpasses animal costs", result.Error);
    }

    #endregion

    #region User Already Fostering Tests

    /// <summary>
    /// EC: User already has active fostering for this animal
    /// Expected: 409 conflict
    /// </summary>
    [Fact]
    public async Task Handle_UserAlreadyHasActiveFostering_ReturnsConflict()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var existingFostering = CreateFostering("fostering-1", animal.Id, ValidUserId, 20m, FosteringStatus.Active);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.Code);
        Assert.Equal("You already foster this animal", result.Error);
    }

    /// <summary>
    /// EC: User has cancelled fostering for this animal (should be allowed to foster again)
    /// Expected: Should succeed
    /// </summary>
    [Fact]
    public async Task Handle_UserHasCancelledFostering_AllowsNewFostering()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var cancelledFostering = CreateFostering("fostering-1", animal.Id, ValidUserId, 20m, FosteringStatus.Cancelled);
        animal.Fosterings.Add(cancelledFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(cancelledFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.Code);
    }

    #endregion

    #region Multiple Active Fosterings Calculation Tests

    /// <summary>
    /// EC: Multiple active fosterings from different users
    /// BVA: Testing that cancelled fosterings are not included in total calculation
    /// </summary>
    [Fact]
    public async Task Handle_MultipleActiveFosterings_CalculatesTotalCorrectly()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var fostering1 = CreateFostering("fostering-1", animal.Id, "user-1", 20m, FosteringStatus.Active);
        var fostering2 = CreateFostering("fostering-2", animal.Id, "user-2", 30m, FosteringStatus.Active);
        var fostering3 = CreateFostering("fostering-3", animal.Id, "user-3", 15m, FosteringStatus.Cancelled);
        
        animal.Fosterings.Add(fostering1);
        animal.Fosterings.Add(fostering2);
        animal.Fosterings.Add(fostering3);
        
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddRangeAsync(fostering1, fostering2, fostering3);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 50m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.TotallyFostered, updatedAnimal.AnimalState);
    }

    /// <summary>
    /// EC: All fosterings cancelled, new user tries to foster
    /// Expected: Should treat as if no active fosterings exist
    /// </summary>
    [Fact]
    public async Task Handle_AllFosteringsCancelled_AllowsFullCostFostering()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var fostering1 = CreateFostering("fostering-1", animal.Id, "user-1", 50m, FosteringStatus.Cancelled);
        var fostering2 = CreateFostering("fostering-2", animal.Id, "user-2", 50m, FosteringStatus.Cancelled);
        
        animal.Fosterings.Add(fostering1);
        animal.Fosterings.Add(fostering2);
        
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddRangeAsync(fostering1, fostering2);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 100m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.TotallyFostered, updatedAnimal.AnimalState);
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// EC: Animal cost is zero
    /// Expected: Any positive value should exceed cost
    /// </summary>
    [Fact]
    public async Task Handle_AnimalCostZero_RejectsAnyPositiveValue()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 0m);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = MinMonthlyValue
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(422, result.Code);
    }

    /// <summary>
    /// EC: MonthValue with many decimal places
    /// BVA: Testing precision handling
    /// </summary>
    [Fact]
    public async Task Handle_MonthValueWithPrecision_HandlesCorrectly()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100.00m);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 10.123456789m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Animal State Update Tests

    /// <summary>
    /// EC: First fostering for animal
    /// Expected: State changes from Available to PartiallyFostered
    /// </summary>
    [Fact]
    public async Task Handle_FirstFostering_UpdatesStateToPartiallyFostered()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.Available);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 30m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.PartiallyFostered, updatedAnimal.AnimalState);
        Assert.Single(updatedAnimal.Fosterings);
    }

    /// <summary>
    /// EC: Adding fostering that completes the total
    /// Expected: State changes from PartiallyFostered to TotallyFostered
    /// </summary>
    [Fact]
    public async Task Handle_CompletingFostering_UpdatesStateToTotallyFostered()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.PartiallyFostered);
        var existingFostering = CreateFostering("fostering-1", animal.Id, "user-1", 70m);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 30m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.TotallyFostered, updatedAnimal.AnimalState);
        Assert.Equal(2, updatedAnimal.Fosterings.Count);
    }

    /// <summary>
    /// BVA: Testing state with minimum fostering value
    /// Expected: State should be PartiallyFostered
    /// </summary>
    [Fact]
    public async Task Handle_MinimumFosteringValue_UpdatesStateCorrectly()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.Available);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = MinMonthlyValue
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.PartiallyFostered, updatedAnimal.AnimalState);
    }

    /// <summary>
    /// EC: Animal with only cancelled fosterings
    /// Expected: Should be treated as Available and accept new fostering
    /// </summary>
    [Fact]
    public async Task Handle_OnlyCancelledFosterings_UpdatesStateCorrectly()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.Available);
        var cancelledFostering = CreateFostering("fostering-1", animal.Id, "user-1", 50m, FosteringStatus.Cancelled);
        animal.Fosterings.Add(cancelledFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(cancelledFostering);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        
        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 30m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.PartiallyFostered, updatedAnimal.AnimalState);
        Assert.Single(updatedAnimal.Fosterings);
    }

    /// <summary>
    /// BVA: Fostering exactly matches animal cost
    /// Expected: State becomes TotallyFostered with single fostering
    /// </summary>
    [Fact]
    public async Task Handle_SingleFosteringMatchesCost_UpdatesToTotallyFostered()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.Available);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 100m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var updatedAnimal = await _context.Animals
            .Include(a => a.Fosterings.Where(f => f.Status == FosteringStatus.Active))
            .FirstOrDefaultAsync(a => a.Id == ValidAnimalId);
        Assert.Equal(AnimalState.TotallyFostered, updatedAnimal.AnimalState);
        Assert.Single(updatedAnimal.Fosterings);
        Assert.Equal(100m, updatedAnimal.Fosterings.First().Amount);
    }

    #endregion

    #region Transaction Rollback Tests

    /// <summary>
    /// EC: Transaction rollback on animal not found
    /// Expected: No fostering created, database remains unchanged
    /// </summary>
    [Fact]
    public async Task Handle_AnimalNotFound_RollsBackTransaction()
    {
        var command = new AddFostering.Command
        {
            AnimalId = "non-existent-id",
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var fosteringCount = await _context.Fosterings.CountAsync();
        Assert.Equal(0, fosteringCount);
    }

    /// <summary>
    /// EC: Transaction rollback when user already fosters animal
    /// Expected: No new fostering created
    /// </summary>
    [Fact]
    public async Task Handle_UserAlreadyFosters_RollsBackTransaction()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m);
        var existingFostering = CreateFostering("fostering-1", animal.Id, ValidUserId, 20m, FosteringStatus.Active);
        animal.Fosterings.Add(existingFostering);
        await _context.Animals.AddAsync(animal);
        await _context.Fosterings.AddAsync(existingFostering);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 15.00m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var fosteringCount = await _context.Fosterings.CountAsync();
        Assert.Equal(1, fosteringCount);
    }

    /// <summary>
    /// EC: Transaction rollback when value exceeds cost
    /// Expected: No fostering created, animal state unchanged
    /// </summary>
    [Fact]
    public async Task Handle_ValueExceedsCost_RollsBackWithoutStateChange()
    {
        var animal = CreateAnimal(ValidAnimalId, cost: 100m, state: AnimalState.Available);
        await _context.Animals.AddAsync(animal);
        await _context.SaveChangesAsync();

        var command = new AddFostering.Command
        {
            AnimalId = ValidAnimalId,
            MonthValue = 150m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var updatedAnimal = await _context.Animals.FindAsync(ValidAnimalId);
        Assert.Equal(AnimalState.Available, updatedAnimal.AnimalState);
        var fosteringCount = await _context.Fosterings.CountAsync();
        Assert.Equal(0, fosteringCount);
    }

    #endregion

    #region Helper Methods

    private static string GenerateUniqueId(string prefix = "")
    {
        return $"{prefix}{Guid.NewGuid().ToString()}";
    }

    private Animal CreateAnimal(string id, decimal cost, AnimalState state = AnimalState.Available)
    {
        var breedId = GenerateUniqueId("breed-");
        var shelterId = GenerateUniqueId("shelter-");
        
        return new Animal
        {
            Id = id,
            Name = "Test Animal",
            Species = Species.Dog,
            Breed = new Breed { Id = breedId, Name = "Test Breed" },
            BreedId = breedId,
            Sex = SexType.Male,
            Size = SizeType.Medium,
            BirthDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-2),
            Description = "Test Description",
            Cost = cost,
            AnimalState = state,
            ShelterId = shelterId,
            Fosterings = new List<Fostering>(),
            Images = new List<Image>()
        };
    }

    private Fostering CreateFostering(string id, string animalId, string userId, decimal amount, FosteringStatus status = FosteringStatus.Active)
    {
        return new Fostering
        {
            Id = id,
            AnimalId = animalId,
            UserId = userId,
            Amount = amount,
            Status = status,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}