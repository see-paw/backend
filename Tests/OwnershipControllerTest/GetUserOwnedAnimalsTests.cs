using Application.Interfaces;
using Application.Ownerships.Queries;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.OwnershipControllerTest;

public class GetUserOwnedAnimalsTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly AppDbContext _context;

    public GetUserOwnedAnimalsTests()
    {
        _userAccessorMock = new Mock<IUserAccessor>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
    }

    // ✅ Cenário 1: User não encontrado
    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync((User)null!);

        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);
        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(404, result.Code);
    }

    // ✅ Cenário 2: User sem animais
    [Fact]
    public async Task Handle_UserWithoutAnimals_ReturnsEmptyList()
    {
        var user = new User { Id = "user-1", UserName = "TestUser" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);
        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        Assert.Equal(200, result.Code);
    }

    // ✅ Cenário 3: User com animais
    [Fact]
    public async Task Handle_UserWithOwnedAnimals_ReturnsOnlyHisAnimals()
    {
        // Arrange
        var user = new User { Id = "user-1", UserName = "Owner" };
        var otherUser = new User { Id = "user-2", UserName = "Other" };

        var breed = new Breed { Name = "Labrador" };
        var shelter = CreateValidShelter();

        var animal1 = CreateValidAnimal(user.Id, breed, shelter, "Rex", DateTime.UtcNow.AddDays(-10));
        var animal2 = CreateValidAnimal(user.Id, breed, shelter, "Max", DateTime.UtcNow.AddDays(-5));
        var otherAnimal = CreateValidAnimal(otherUser.Id, breed, shelter, "Buddy", DateTime.UtcNow.AddDays(-1));

        await _context.AddRangeAsync(breed, shelter, animal1, animal2, otherAnimal);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);

        // Act
        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.All(result.Value!, a => Assert.Equal(user.Id, a.OwnerId));
        Assert.DoesNotContain(result.Value!, a => a.OwnerId == otherUser.Id);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(200, result.Code);
    }

    // ✅ Cenário 4: Ordenação decrescente por OwnershipStartDate
    [Fact]
    public async Task Handle_ReturnsAnimalsOrderedByOwnershipStartDateDescending()
    {
        var user = new User { Id = "user-1", UserName = "Owner" };
        var breed = new Breed { Name = "Golden Retriever" };
        var shelter = CreateValidShelter();

        var oldest = CreateValidAnimal(user.Id, breed, shelter, "Oldest", DateTime.UtcNow.AddDays(-30));
        var middle = CreateValidAnimal(user.Id, breed, shelter, "Middle", DateTime.UtcNow.AddDays(-10));
        var newest = CreateValidAnimal(user.Id, breed, shelter, "Newest", DateTime.UtcNow.AddDays(-1));

        await _context.AddRangeAsync(breed, shelter, oldest, middle, newest);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);
        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);

        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "Newest", "Middle", "Oldest" }, result.Value!.Select(a => a.Name));
    }

    // ✅ Cenário 5: Includes funcionam (Breed, Shelter, Images)
    [Fact]
    public async Task Handle_IncludesRelatedEntities()
    {
        var user = new User { Id = "user-1", UserName = "Owner" };
        var breed = new Breed { Name = "Beagle" };
        var shelter = CreateValidShelter();

        var animal = CreateValidAnimal(user.Id, breed, shelter, "Snoopy", DateTime.UtcNow);
        animal.Images = new List<Image>
        {
            new Image { Url = "url1.jpg", PublicId = "1", IsPrincipal = true, Description = "im1"},
            new Image { Url = "url2.jpg", PublicId = "1", IsPrincipal = false, Description = "im2"}
        };

        await _context.AddRangeAsync(breed, shelter, animal);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);
        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);

        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = result.Value!.First();
        Assert.Equal("Beagle", returned.Breed.Name);
        Assert.Equal(shelter.Name, returned.Shelter.Name);
        Assert.Equal(2, returned.Images.Count);
    }

    // ✅ Cenário 6: Token de cancelamento
    [Fact]
    public async Task Handle_RespectsCancellationToken()
    {
        var user = new User { Id = "user-1", UserName = "Owner" };
        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);
        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.Handle(new GetUserOwnedAnimals.Query(), cts.Token));
    }

    // ✅ Cenário 7: Apenas um animal
    [Fact]
    public async Task Handle_UserWithOneAnimal_ReturnsSingleAnimal()
    {
        var user = new User { Id = "user-1", UserName = "Owner" };
        var breed = new Breed { Name = "Pug" };
        var shelter = CreateValidShelter();
        var animal = CreateValidAnimal(user.Id, breed, shelter, "Bob", DateTime.UtcNow.AddDays(-3));

        await _context.AddRangeAsync(breed, shelter, animal);
        await _context.SaveChangesAsync();

        _userAccessorMock.Setup(x => x.GetUserAsync()).ReturnsAsync(user);
        var handler = new GetUserOwnedAnimals.Handler(_context, _userAccessorMock.Object);

        var result = await handler.Handle(new GetUserOwnedAnimals.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("Bob", result.Value!.First().Name);
    }

    // --- Helpers ---------------------------------------------------

    private static Animal CreateValidAnimal(string? ownerId, Breed breed, Shelter shelter, string name, DateTime startDate)
    {
        return new Animal
        {
            Name = name,
            OwnerId = ownerId,
            BreedId = breed.Id,
            Breed = breed,
            ShelterId = shelter.Id,
            Shelter = shelter,
            AnimalState = AnimalState.HasOwner,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Black",
            BirthDate = new DateOnly(2022, 5, 1),
            Sterilized = true,
            Cost = 50,
            Features = "Friendly",
            OwnershipStartDate = startDate,
            Images = new List<Image> { new Image { Url = "image.jpg",PublicId = "1", IsPrincipal = true, Description = "image"} }
        };
    }

    private static Shelter CreateValidShelter()
    {
        return new Shelter
        {
            Name = "Shelter One",
            Street = "Rua Central 123",
            City = "Porto",
            PostalCode = "4000-123",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0),
            Images = new List<Image> { new Image { Url = "shelter.jpg",PublicId = "1", IsPrincipal = true, Description = "image"} }
        };
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}