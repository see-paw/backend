using Application.Core;
using Application.Favorites.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Persistence;

namespace Tests.FavoritesTests
{
    /// <summary>
    /// Unit tests for AddFavorite.Handler.
    /// </summary>
    /// <remarks>
    /// Verifies creation and reactivation logic, including animal state validation and user authentication.
    /// </remarks>
    public class AddFavoriteHandlerTests
    {
        //codacy: ignore[complexity]

        // Fixed GUIDs to ensure consistency across tests
        private const string FixedUserId = "11111111-1111-1111-1111-111111111111";
        private const string FixedAnimalId = "22222222-2222-2222-2222-222222222222";
        private const string FixedBreedId = "33333333-3333-3333-3333-333333333333";

        // Creates an isolated in-memory database context
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"FavoritesDb_{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        // Creates a fake user with a fixed GUID
        private User NewUser() => new()
        {
            Id = FixedUserId,
            UserName = "test@test.com",
            Email = "test@test.com",
            Name = "Test User"
        };

        // Creates a fake animal with a fixed GUID
        private Animal NewAnimal(AnimalState state = AnimalState.Available) => new()
        {
            Id = FixedAnimalId,
            Name = "Rex",
            AnimalState = state,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2021, 1, 1),
            Sterilized = true,
            BreedId = FixedBreedId,
            Cost = 30,
            Features = "Friendly dog"
        };

        // Successfully adds a new favorite
        [Fact]
        public async Task AddFavoriteWithSuccess()
        {
            var ctx = CreateContext();

            var breed = new Breed
            {
                Id = FixedBreedId,
                Name = "Mixed"
            };
            await ctx.Breeds.AddAsync(breed);

            var user = NewUser();
            var animal = NewAnimal();

            await ctx.Users.AddAsync(user);
            await ctx.Animals.AddAsync(animal);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);


            Assert.Equal(201, result.Code);
   
        }

        // Returns 401 if user is not authenticated
        [Fact]
        public async Task AddFavoriteWithUserNotAuthenticated()
        {
            var ctx = CreateContext();

            var breed = new Breed
            {
                Id = FixedBreedId,
                Name = "Mixed"
            };
            await ctx.Breeds.AddAsync(breed);

            var animal = NewAnimal();
            await ctx.Animals.AddAsync(animal);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync((User?)null);

            var mapper = new Mock<IMapper>();
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

        
            Assert.Equal(401, result.Code);

        }

        //  Returns 404 if animal does not exist
        [Fact]
        public async Task AddFavoriteWithAnimalNotFound()
        {
            var ctx = CreateContext();
            var user = NewUser();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = Guid.NewGuid().ToString() }; // Nonexistent ID
            var result = await handler.Handle(cmd, default);

            Assert.Equal(404, result.Code);
            
        }

        // Returns 409 if animal is not available to favorite
        [Fact]
        public async Task AddFavoriteWtihAnimalNotAvailable()
        {
            var ctx = CreateContext();

            var breed = new Breed
            {
                Id = FixedBreedId,
                Name = "Mixed"
            };
            await ctx.Breeds.AddAsync(breed);

            var user = NewUser();
            var animal = NewAnimal(AnimalState.HasOwner);

            await ctx.Users.AddAsync(user);
            await ctx.Animals.AddAsync(animal);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.False(result.IsSuccess);
         
        }

        // Returns 409 if favorite already exists and is active
        [Fact]
        public async Task AddFavoriteWithAlreadyActiveFavorite()
        {
            var ctx = CreateContext();

            var breed = new Breed
            {
                Id = FixedBreedId,
                Name = "Mixed"
            };
            await ctx.Breeds.AddAsync(breed);

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
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.False(result.IsSuccess);

        }

        //  Reactivates an existing inactive favorite
        [Fact]
        public async Task AddFavorite_ReactivatesInactiveFavorite()
        {
            var ctx = CreateContext();

            var breed = new Breed
            {
                Id = FixedBreedId,
                Name = "Mixed"
            };
            await ctx.Breeds.AddAsync(breed);

            var user = NewUser();
            var animal = NewAnimal();

            var favorite = new Favorite
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                AnimalId = animal.Id,
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };

            await ctx.Users.AddAsync(user);
            await ctx.Animals.AddAsync(animal);
            await ctx.Favorites.AddAsync(favorite);
            await ctx.SaveChangesAsync();

            var userAccessor = new Mock<IUserAccessor>();
            userAccessor.Setup(u => u.GetUserAsync()).ReturnsAsync(user);

            var mapper = new Mock<IMapper>();
            var handler = new AddFavorite.Handler(ctx, userAccessor.Object, mapper.Object);

            var cmd = new AddFavorite.Command { AnimalId = animal.Id };
            var result = await handler.Handle(cmd, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.Code);

            var reactivated = await ctx.Favorites.FirstAsync();
            Assert.True(reactivated.IsActive);
        }
    }
}
