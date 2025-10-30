using Application.Interfaces;
using Application.Ownerships.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.OwnershipRequestsTest;

public class GetOwnershipRequestsByUserTests
{
    
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly AppDbContext _context;
    private readonly GetOwnershipRequestsByUser.Handler _handler;

    public GetOwnershipRequestsByUserTests()
    {
        _userAccessorMock = new Mock<IUserAccessor>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _handler = new GetOwnershipRequestsByUser.Handler(_context, _userAccessorMock.Object);
    }

    private void SeedDatabase(User user, List<OwnershipRequest> requests)
    {
        _context.Users.Add(user);
        _context.OwnershipRequests.AddRange(requests);
        _context.SaveChanges();
    }

    private Animal CreateAnimal(string id, string name)
    {
        return new Animal
        {
            Id = id,
            Name = name,
            Breed = new Breed { Id = Guid.NewGuid().ToString(), Name = "Labrador" },
            Shelter = new Shelter { Id = Guid.NewGuid().ToString(), Name = "Test Shelter" },
            Images = new List<Image>
            {
                new Image 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    IsPrincipal = true, 
                    Url = "test.jpg",
                    Description = "Test image"
                }
            }
        };
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailureWithNotFound()
    {
        // Arrange
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync((User)null!);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoRequests_ReturnsEmptyList()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, new List<OwnershipRequest>());

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        Assert.Equal(200, result.Code);
    }

    [Fact]
    public async Task Handle_WhenUserHasPendingRequests_ReturnsOnlyPendingRequests()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-5),
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(OwnershipStatus.Pending, result.Value[0].Status);
    }

    [Fact]
    public async Task Handle_WhenUserHasAnalysingRequests_ReturnsAnalysingRequests()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Analysing,
                RequestedAt = DateTime.UtcNow.AddDays(-3),
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(OwnershipStatus.Analysing, result.Value[0].Status);
    }

    [Fact]
    public async Task Handle_WhenUserHasRecentlyRejectedRequests_ReturnsRejectedRequests()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5), // Rejected 5 days ago
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(OwnershipStatus.Rejected, result.Value[0].Status);
    }

    [Fact]
    public async Task Handle_WhenUserHasOldRejectedRequests_DoesNotReturnOldRejected()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2), // Rejected 2 months ago
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value); // Should not include requests older than 1 month
    }

    [Fact]
    public async Task Handle_OrdersPendingBeforeRejected()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal1 = CreateAnimal("animal1", "Max");
        var animal2 = CreateAnimal("animal2", "Bella");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal1.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Animal = animal1,
                User = user
            },
            new OwnershipRequest
            {
                Id = "req2",
                UserId = user.Id,
                AnimalId = animal2.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-5),
                Animal = animal2,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(OwnershipStatus.Pending, result.Value[0].Status); // Pending first
        Assert.Equal(OwnershipStatus.Rejected, result.Value[1].Status); // Rejected second
    }

    [Fact]
    public async Task Handle_WithinSameStatus_OrdersByRequestedAtDescending()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal1 = CreateAnimal("animal1", "Max");
        var animal2 = CreateAnimal("animal2", "Bella");
        var animal3 = CreateAnimal("animal3", "Charlie");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal1.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-10), // Oldest
                Animal = animal1,
                User = user
            },
            new OwnershipRequest
            {
                Id = "req2",
                UserId = user.Id,
                AnimalId = animal2.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-2), // Most recent
                Animal = animal2,
                User = user
            },
            new OwnershipRequest
            {
                Id = "req3",
                UserId = user.Id,
                AnimalId = animal3.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-5), // Middle
                Animal = animal3,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal("req2", result.Value[0].Id); // Most recent first
        Assert.Equal("req3", result.Value[1].Id); // Middle
        Assert.Equal("req1", result.Value[2].Id); // Oldest last
    }

    [Fact]
    public async Task Handle_ComplexScenario_OrdersCorrectly()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal1 = CreateAnimal("animal1", "Max");
        var animal2 = CreateAnimal("animal2", "Bella");
        var animal3 = CreateAnimal("animal3", "Charlie");
        var animal4 = CreateAnimal("animal4", "Luna");

        var requests = new List<OwnershipRequest>
        {
            // Recent rejected (should be last)
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal1.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                Animal = animal1,
                User = user
            },
            // Most recent pending (should be first)
            new OwnershipRequest
            {
                Id = "req2",
                UserId = user.Id,
                AnimalId = animal2.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-1),
                Animal = animal2,
                User = user
            },
            // Analysing (should be second)
            new OwnershipRequest
            {
                Id = "req3",
                UserId = user.Id,
                AnimalId = animal3.Id,
                Status = OwnershipStatus.Analysing,
                RequestedAt = DateTime.UtcNow.AddDays(-4),
                Animal = animal3,
                User = user
            },
            // Older pending (should be third)
            new OwnershipRequest
            {
                Id = "req4",
                UserId = user.Id,
                AnimalId = animal4.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow.AddDays(-7),
                Animal = animal4,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Count);
        
        // Most recent pending first
        Assert.Equal("req2", result.Value[0].Id);
        Assert.Equal(OwnershipStatus.Pending, result.Value[0].Status);
        
        // Older pending second
        Assert.Equal("req3", result.Value[1].Id);
        Assert.Equal(OwnershipStatus.Analysing, result.Value[1].Status);
        
        // Analysing third
        Assert.Equal("req4", result.Value[2].Id);
        Assert.Equal(OwnershipStatus.Pending, result.Value[2].Status);
        
        // Recent rejected last
        Assert.Equal("req1", result.Value[3].Id);
        Assert.Equal(OwnershipStatus.Rejected, result.Value[3].Status);
    }

    [Fact]
    public async Task Handle_IncludesAnimalWithBreed()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value[0].Animal);
        Assert.NotNull(result.Value[0].Animal.Breed);
        Assert.Equal("Labrador", result.Value[0].Animal.Breed.Name);
    }

    [Fact]
    public async Task Handle_IncludesAnimalWithShelter()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value[0].Animal);
        Assert.NotNull(result.Value[0].Animal.Shelter);
        Assert.Equal("Test Shelter", result.Value[0].Animal.Shelter.Name);
    }

    [Fact]
    public async Task Handle_IncludesAnimalWithImages()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal = CreateAnimal("animal1", "Max");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Animal = animal,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value[0].Animal);
        Assert.NotEmpty(result.Value[0].Animal.Images);
        Assert.True(result.Value[0].Animal.Images.First().IsPrincipal);
    }

    [Fact]
    public async Task Handle_OnlyReturnsRequestsForCurrentUser()
    {
        // Arrange
        var user1 = new User { Id = "user1", Name = "User One", Email = "user1@test.com" };
        var user2 = new User { Id = "user2", Name = "User Two", Email = "user2@test.com" };
        var animal1 = CreateAnimal("animal1", "Max");
        var animal2 = CreateAnimal("animal2", "Bella");

        var requests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user1.Id,
                AnimalId = animal1.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Animal = animal1,
                User = user1
            },
            new OwnershipRequest
            {
                Id = "req2",
                UserId = user2.Id,
                AnimalId = animal2.Id,
                Status = OwnershipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Animal = animal2,
                User = user2
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user1);

        _context.Users.Add(user1);
        _context.Users.Add(user2);
        SeedDatabase(user1, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("req1", result.Value[0].Id);
        Assert.Equal(user1.Id, result.Value[0].UserId);
    }

    [Fact]
    public async Task Handle_RespectsOneMonthBoundary_ForRejectedRequests()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        var animal1 = CreateAnimal("animal1", "Max");
        var animal2 = CreateAnimal("animal2", "Bella");

        var exactlyOneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var requests = new List<OwnershipRequest>
        {
            // Exactly at boundary (should be included)
            new OwnershipRequest
            {
                Id = "req1",
                UserId = user.Id,
                AnimalId = animal1.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = exactlyOneMonthAgo.AddMinutes(5),
                Animal = animal1,
                User = user
            },
            // Just past boundary (should NOT be included)
            new OwnershipRequest
            {
                Id = "req2",
                UserId = user.Id,
                AnimalId = animal2.Id,
                Status = OwnershipStatus.Rejected,
                RequestedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = exactlyOneMonthAgo.AddMinutes(-10),
                Animal = animal2,
                User = user
            }
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, requests);

        var query = new GetOwnershipRequestsByUser.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("req1", result.Value[0].Id);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CanBeCancelled()
    {
        // Arrange
        var user = new User { Id = "user1", Name = "Test User", Email = "test@test.com" };
        
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);

        SeedDatabase(user, new List<OwnershipRequest>());

        var query = new GetOwnershipRequestsByUser.Query();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _handler.Handle(query, cts.Token));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
