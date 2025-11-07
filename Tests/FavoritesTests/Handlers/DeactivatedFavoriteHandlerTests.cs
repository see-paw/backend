using Application.Core;
using Application.Favorites.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Persistence;

namespace Tests.Favorites.Handlers
{
    /// <summary>
    /// Unit tests for DeactivateFavorite.Handler.
    /// </summary>
    public class DeactivateFavoriteHandlerTests
    {
        //codacy: ignore[complexity]

        // Helper: creates an isolated in-memory database
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"FavoritesDb_{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        // Helper: creates a mock user
        private User NewUser() => new()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test User"
        };

        // Helper: creates a mock animal
        private Animal NewAnimal() => new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Rex",
            AnimalState = Domain.Enums.AnimalState.Available,
            Species = Domain.Enums.Species.Dog,
            Size = Domain.Enums.SizeType.Medium,
            Sex = Domain.Enums.SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2021, 1, 1),
            Sterilized = true,
            BreedId = Guid.NewGuid().ToString(),
            Cost = 30,
            Features = "Friendly dog"
        };

        //  Success — deactivates an active favorite
        [Fact]
        public async Task DeactivateFavoriteWithSuccess()
        {
            // Arrange
            var ctx = CreateContext();
            var user = NewUser();
            var breed = new Breed
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Golden Retriever"
            };

            var image = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/image.jpg",
                PublicId = "sample_public_id",
                Description = "sample_description"
            };

            var animal = NewAnimal();
            animal.BreedId = breed.Id;
            animal.Breed = breed;
            animal.Images = new List<Image> { image };

            var favorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                AnimalId = animal.Id,
                IsActive = true
            };

            // Persist all related entities
            await ctx.Users.AddAsync(user);
            await ctx.Breeds.AddAsync(breed);
            await ctx.Images.AddAsync(image);
            await ctx.Animals.AddAsync(animal);
            await ctx.Favorites.AddAsync(favorite);
            await ctx.SaveChangesAsync();

            // Mock dependencies
            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new DeactivateFavorite.Handler(ctx, userAccessor.Object);

            var cmd = new DeactivateFavorite.Command { AnimalId = animal.Id };

            // Assert that the animal exists
            var exists = await ctx.Animals.AnyAsync(a => a.Id == animal.Id);
            Assert.True(exists);

            // Act
            var result = await handler.Handle(cmd, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(ctx.Favorites.First().IsActive);
        }


        // Returns 401 if user is not authenticated
        [Fact]
        public async Task DeactivateFavorite_UserNotAuthenticated_Returns401()
        {
            var ctx = CreateContext();
            var animal = NewAnimal();
            await ctx.Animals.AddAsync(animal);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync((User?)null);

            var mapper = new Mock<IMapper>();
            var handler = new DeactivateFavorite.Handler(ctx, userAccessor.Object);

            var cmd = new DeactivateFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.Equal(401, result.Code);
        }

        // Returns 404 if favorite is not found
        [Fact]
        public async Task DeactivateFavorite_FavoriteNotFound_Returns404()
        {
            var ctx = CreateContext();
            var user = NewUser();
            var animal = NewAnimal();

            await ctx.Users.AddAsync(user);
            await ctx.Animals.AddAsync(animal);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new DeactivateFavorite.Handler(ctx, userAccessor.Object);

            var cmd = new DeactivateFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.Equal(404, result.Code);
        }


        //  SaveChangesAsync fails (simulated DB failure)
        [Fact]
        public async Task DeactivateFavorite_SaveFails()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"Fail_{Guid.NewGuid()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            // Create the failing context defined below in this same file
            var ctx = new FailingAppDbContext(options);

            var user = NewUser();
            var animal = NewAnimal();
            var favorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                AnimalId = animal.Id,
                IsActive = true
            };

            await ctx.Users.AddAsync(user);
            await ctx.Animals.AddAsync(animal);
            await ctx.Favorites.AddAsync(favorite);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new DeactivateFavorite.Handler(ctx, userAccessor.Object);

            var cmd = new DeactivateFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.False(result.IsSuccess);
        }

        // Inner class used to simulate a database save failure
        private class FailingAppDbContext : AppDbContext
        {
            public FailingAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(0); // Always fail
        }
    }
}
