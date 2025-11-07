using Application.Interfaces;
using Application.OwnershipRequests.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.OwnershipRequestsTest.HandlersTest;

public class GetOwnershipRequestsByShelterHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly Mock<INotificationService> _mockNotificationService;

    public GetOwnershipRequestsByShelterHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private async Task<(Shelter shelter, List<OwnershipRequest> requests)> SeedMultipleOwnershipRequestsAsync(
        string shelterId,
        int count = 3)
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

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);

        var requests = new List<OwnershipRequest>();

        for (int i = 0; i < count; i++)
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Animal {i}",
                Colour = "Brown",
                Cost = 100,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = breed.Id,
                ShelterId = shelterId,
                AnimalState = AnimalState.Available
            };

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"User {i}",
                Email = $"user{i}@example.com",
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
                Status = OwnershipStatus.Pending
            };

            _context.Animals.Add(animal);
            _context.Users.Add(user);
            _context.OwnershipRequests.Add(request);
            requests.Add(request);

            // Add delay to ensure different RequestedAt timestamps for ordering tests
            await Task.Delay(10);
        }

        await _context.SaveChangesAsync();
        return (shelter, requests);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnForbidden_WhenUserIsNotShelterAdmin()
    {
        var nonAdminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = null // Not a shelter admin
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(nonAdminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(403, result.Code);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnNotFound_WhenShelterDoesNotExist()
    {
        var shelterId = Guid.NewGuid().ToString();
        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId // Shelter doesn't exist in database
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnSuccess_WhenShelterExists()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 3);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnEmptyList_WhenNoRequestsExist()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 0); // No requests

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(0, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnOnlyRequestsFromAdminShelter()
    {
        var shelterId1 = Guid.NewGuid().ToString();
        var shelterId2 = Guid.NewGuid().ToString();

        await SeedMultipleOwnershipRequestsAsync(shelterId1, 3);
        await SeedMultipleOwnershipRequestsAsync(shelterId2, 2);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId1
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(3, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldIncludeAnimalInRequests()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 1);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.NotNull(result.Value!.Items.First().Animal);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldIncludeUserInRequests()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 1);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.NotNull(result.Value!.Items.First().User);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldOrderByRequestedAtDescending()
    {
        var shelterId = Guid.NewGuid().ToString();
        var (shelter, requests) = await SeedMultipleOwnershipRequestsAsync(shelterId, 3);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query
        {
            PageSize = 10
        }, default);

        var returnedRequests = result.Value!.Items.ToList();
        Assert.True(returnedRequests[0].RequestedAt >= returnedRequests[1].RequestedAt);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldRespectPageSize()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 5);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query
        {
            PageSize = 2
        }, default);

        Assert.Equal(2, result.Value!.Items.Count);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnCorrectPageNumber()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 5);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query
        {
            PageNumber = 2,
            PageSize = 2
        }, default);

        Assert.Equal(2, result.Value!.CurrentPage);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnCorrectTotalPages()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 5);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query
        {
            PageSize = 2 // 5 items / 2 per page = 3 pages
        }, default);

        Assert.Equal(3, result.Value!.TotalPages);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldReturnCorrectTotalCount()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 7);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query
        {
            PageSize = 3
        }, default);

        Assert.Equal(7, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldUseDefaultPageNumber_WhenNotProvided()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 3);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(1, result.Value!.CurrentPage);
    }

    [Fact]
    public async Task GetOwnershipRequestsByShelter_ShouldUseDefaultPageSize_WhenNotProvided()
    {
        var shelterId = Guid.NewGuid().ToString();
        await SeedMultipleOwnershipRequestsAsync(shelterId, 25);

        var adminUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            ShelterId = shelterId
        };

        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(adminUser);

        var handler = new GetOwnershipRequestsByShelter.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetOwnershipRequestsByShelter.Query(), default);

        Assert.Equal(20, result.Value!.Items.Count); // Default PageSize is 20
    }
}