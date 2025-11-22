using Application.Favorites.Queries;
using Application.Interfaces;

using Domain;
using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Moq;

using Persistence;

namespace Tests.FavoritesTests.Handlers;

/// <summary>
/// Unit tests for GetUserFavorites.Handler.
/// 
/// These tests validate the query handler logic for retrieving user favorites, ensuring that:
/// - Only active favorites are returned
/// - Only favorites belonging to the authenticated user are returned
/// - Results are properly paginated
/// - Related entities (Animal, Breed, Shelter, Images) are included
/// - Results are ordered by creation date (newest first)
/// </summary>
public class GetUserFavoritesHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IUserAccessor> _mockUserAccessor;

    public GetUserFavoritesHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockUserAccessor = new Mock<IUserAccessor>();
    }

    private async Task<(User user, List<Animal> animals, Shelter shelter, Breed breed)> SeedFavoritesAsync(
        int favoriteCount = 3,
        bool includeInactiveFavorites = false,
        bool includeOtherUserFavorites = false)
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
            Name = "Test Breed",
            Description = "Test breed description"
        };

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test User",
            Email = "test@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "User Street",
            City = "User City",
            PostalCode = "1234-567"
        };

        var animals = new List<Animal>();

        for (int i = 0; i < favoriteCount; i++)
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Animal {i + 1}",
                AnimalState = AnimalState.Available,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                Cost = 50m,
                BreedId = breed.Id,
                ShelterId = shelter.Id
            };

            var image = new Image
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animal.Id,
                Url = $"https://example.com/animal{i + 1}.jpg",
                IsPrincipal = true,
                PublicId = $"test_image_{i + 1}",
                Description = $"Image of Animal {i + 1}"
            };

            animal.Images.Add(image);
            animals.Add(animal);

            var favorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                AnimalId = animal.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };

            _context.Favorites.Add(favorite);
        }

        // Add inactive favorite if requested
        if (includeInactiveFavorites)
        {
            var inactiveAnimal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Inactive Favorite Animal",
                AnimalState = AnimalState.Available,
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                Colour = "White",
                BirthDate = new DateOnly(2021, 5, 10),
                Sterilized = true,
                Cost = 30m,
                BreedId = breed.Id,
                ShelterId = shelter.Id
            };

            var inactiveImage = new Image
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = inactiveAnimal.Id,
                Url = "https://example.com/inactive.jpg",
                IsPrincipal = true,
                PublicId = "inactive_image",
                Description = "Inactive favorite"
            };

            inactiveAnimal.Images.Add(inactiveImage);
            animals.Add(inactiveAnimal);

            var inactiveFavorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                AnimalId = inactiveAnimal.Id,
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            _context.Favorites.Add(inactiveFavorite);
        }

        // Add other user's favorites if requested
        if (includeOtherUserFavorites)
        {
            var otherUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Other User",
                Email = "other@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Street = "Other Street",
                City = "Other City",
                PostalCode = "5678-901"
            };

            var otherAnimal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Other User's Favorite",
                AnimalState = AnimalState.Available,
                Species = Species.Dog,
                Size = SizeType.Large,
                Sex = SexType.Male,
                Colour = "Black",
                BirthDate = new DateOnly(2019, 3, 15),
                Sterilized = true,
                Cost = 70m,
                BreedId = breed.Id,
                ShelterId = shelter.Id
            };

            var otherImage = new Image
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = otherAnimal.Id,
                Url = "https://example.com/other.jpg",
                IsPrincipal = true,
                PublicId = "other_image",
                Description = "Other user's favorite"
            };

            otherAnimal.Images.Add(otherImage);
            animals.Add(otherAnimal);

            var otherFavorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = otherUser.Id,
                AnimalId = otherAnimal.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            _context.Users.Add(otherUser);
            _context.Favorites.Add(otherFavorite);
        }

        _context.Shelters.Add(shelter);
        _context.Breeds.Add(breed);
        _context.Animals.AddRange(animals);
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return (user, animals, shelter, breed);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnSuccess_WhenFavoritesExist()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectTotalCount_WhenFavoritesExist()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(favoriteCount: 5);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Equal(5, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnOnlyActiveFavorites()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(
            favoriteCount: 3,
            includeInactiveFavorites: true);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Equal(3, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnOnlyCurrentUserFavorites()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(
            favoriteCount: 3,
            includeOtherUserFavorites: true);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Equal(3, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnAnimalsInDescendingOrderByCreatedAt()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(favoriteCount: 3);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        var returnedAnimals = result.Value!.Items.ToList();
        Assert.Equal("Animal 1", returnedAnimals[0].Name);
        Assert.Equal("Animal 2", returnedAnimals[1].Name);
        Assert.Equal("Animal 3", returnedAnimals[2].Name);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectCurrentPage()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query { PageNumber = 1 }, default);

        Assert.Equal(1, result.Value!.CurrentPage);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectPageSize()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query { PageSize = 20 }, default);

        Assert.Equal(20, result.Value!.PageSize);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectTotalPages_WhenMultiplePagesExist()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(favoriteCount: 25);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query { PageSize = 10 }, default);

        Assert.Equal(3, result.Value!.TotalPages);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectItemsForPage2()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync(favoriteCount: 25);
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query
        {
            PageNumber = 2,
            PageSize = 10
        }, default);

        Assert.Equal(10, result.Value!.Items.Count);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldIncludeAnimalEntity()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.NotNull(result.Value!.Items.First().Name);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldIncludeBreedEntity()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.NotNull(result.Value!.Items.First().Breed);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldIncludeShelterEntity()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.NotNull(result.Value!.Items.First().Shelter);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldIncludeImagesEntity()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.NotEmpty(result.Value!.Items.First().Images);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnSuccessWithEmptyList_WhenUserHasNoFavorites()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User Without Favorites",
            Email = "nofavorites@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Street",
            City = "City",
            PostalCode = "1234-567"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnZeroTotalCount_WhenUserHasNoFavorites()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User Without Favorites",
            Email = "nofavorites@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Street",
            City = "City",
            PostalCode = "1234-567"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Equal(0, result.Value!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnEmptyList_WhenUserHasNoFavorites()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User Without Favorites",
            Email = "nofavorites@example.com",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Street = "Street",
            City = "City",
            PostalCode = "1234-567"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Empty(result.Value!.Items);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturn200StatusCode()
    {
        var (user, animals, shelter, breed) = await SeedFavoritesAsync();
        _mockUserAccessor.Setup(x => x.GetUserId()).Returns(user.Id);

        var handler = new GetUserFavorites.Handler(_context, _mockUserAccessor.Object);

        var result = await handler.Handle(new GetUserFavorites.Query(), default);

        Assert.Equal(200, result.Code);
    }
}