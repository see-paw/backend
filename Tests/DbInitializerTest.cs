using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence;

namespace Tests
{
    /// <summary>
    /// Unit tests for DbInitializer.
    /// Validates database seeding logic for users, roles, shelters, breeds, animals, and images.
    /// </summary>
    public class DbInitializerTest
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly ILoggerFactory _loggerFactory;

        public DbInitializerTest()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            _loggerFactory = NullLoggerFactory.Instance;
        }

        private async Task<(UserManager<User>, RoleManager<IdentityRole>)> CreateUserAndRoleManagers(AppDbContext context)
        {
            var userStore = new UserStore<User>(context);
            var roleStore = new RoleStore<IdentityRole>(context);

            var userManager = new UserManager<User>(
                userStore,
                null,
                new PasswordHasher<User>(),
                null,
                null,
                null,
                null,
                null,
                null);

            var roleManager = new RoleManager<IdentityRole>(
                roleStore,
                null,
                null,
                null,
                null);

            return (userManager, roleManager);
        }

        // ========== USER & ROLE TESTS ==========

        /// <summary>
        /// Tests that SeedData creates all required roles.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesAllRoles()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.True(await roleManager.RoleExistsAsync("PlatformAdmin"));
            Assert.True(await roleManager.RoleExistsAsync("AdminCAA"));
            Assert.True(await roleManager.RoleExistsAsync("User"));
        }

        /// <summary>
        /// Tests that SeedData creates 5 users with correct data.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_Creates5Users()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(5, userManager.Users.Count());
        }

        /// <summary>
        /// Tests that Bob is assigned PlatformAdmin role.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_AssignsPlatformAdminRole()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var bob = await userManager.FindByEmailAsync("bob@test.com");
            Assert.NotNull(bob);
            Assert.True(await userManager.IsInRoleAsync(bob, "PlatformAdmin"));
        }

        /// <summary>
        /// Tests that Alice is assigned AdminCAA role and has ShelterId.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_AssignsAdminCAARole()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var alice = await userManager.FindByEmailAsync("alice@test.com");
            Assert.NotNull(alice);
            Assert.True(await userManager.IsInRoleAsync(alice, "AdminCAA"));
            Assert.NotNull(alice.ShelterId);
        }

        /// <summary>
        /// Tests that regular users (Carlos, Diana, Eduardo) are assigned User role.
        /// </summary>
        [Theory]
        [InlineData("carlos@test.com")]
        [InlineData("diana@test.com")]
        [InlineData("eduardo@test.com")]
        public async Task SeedData_EmptyDatabase_AssignsUserRole(string email)
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var user = await userManager.FindByEmailAsync(email);
            Assert.NotNull(user);
            Assert.True(await userManager.IsInRoleAsync(user, "User"));
        }

        /// <summary>
        /// Tests that users are created with correct password.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesUsersWithCorrectPassword()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var bob = await userManager.FindByEmailAsync("bob@test.com");
            var passwordCheck = await userManager.CheckPasswordAsync(bob, "Pa$$w0rd");
            Assert.True(passwordCheck);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate users if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_UsersExist_DoesNotDuplicate()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingUser = new User
            {
                Name = "Existing User",
                UserName = "existing@test.com",
                Email = "existing@test.com",
                City = "Porto",
                Street = "Test Street",
                PostalCode = "4000-000",
                BirthDate = new DateTime(1990, 1, 1),
                PhoneNumber = "900000000"
            };

            await userManager.CreateAsync(existingUser, "Pa$$w0rd");

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(1, userManager.Users.Count());
            Assert.Equal("Existing User", userManager.Users.First().Name);
        }

        /// <summary>
        /// Tests that all users have valid phone numbers.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesUsersWithValidPhoneNumbers()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var users = userManager.Users.ToList();
            Assert.All(users, u =>
            {
                Assert.Matches(@"^[29]\d{8}$", u.PhoneNumber);
            });
        }

        // ========== SHELTER TESTS ==========

        /// <summary>
        /// Tests that SeedData creates shelters when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesShelters()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(2, await context.Shelters.CountAsync());
            Assert.Contains(context.Shelters, s => s.Name == "Test Shelter");
            Assert.Contains(context.Shelters, s => s.Name == "Test Shelter 2");
        }

        /// <summary>
        /// Tests that SeedData does not duplicate shelters if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_SheltersExist_DoesNotDuplicate()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingShelter = new Shelter
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Shelter",
                Street = "Existing Street",
                City = "Porto",
                PostalCode = "4000-000",
                Phone = "900000000",
                NIF = "000000000",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            };

            context.Shelters.Add(existingShelter);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(1, await context.Shelters.CountAsync());
            Assert.Equal("Existing Shelter", (await context.Shelters.FirstAsync()).Name);
        }

        // ========== BREED TESTS ==========

        /// <summary>
        /// Tests that SeedData creates 3 breeds.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesBreeds()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(3, await context.Breeds.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData does not duplicate breeds if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_BreedsExist_DoesNotDuplicate()
        {
            // Arrange
           await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingBreed = new Breed
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Breed"
            };

            context.Breeds.Add(existingBreed);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(1, await context.Breeds.CountAsync());
            Assert.Equal("Existing Breed", (await context.Breeds.FirstAsync()).Name);
        }

        // ========== ANIMAL TESTS ==========

        /// <summary>
        /// Tests that SeedData creates animals when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesAnimals()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(13, await context.Animals.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData creates animals with different states.
        /// </summary>
        [Theory]
        [InlineData(AnimalState.Available, 9)]
        [InlineData(AnimalState.Inactive, 1)]
        [InlineData(AnimalState.HasOwner, 1)]
        [InlineData(AnimalState.TotallyFostered, 1)]
        [InlineData(AnimalState.PartiallyFostered, 1)]
        public async Task SeedData_EmptyDatabase_CreatesAnimalsWithDifferentStates(AnimalState state, int expectedCount)
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var count = await context.Animals.CountAsync(a => a.AnimalState == state);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that seeded animals have correct species distribution.
        /// </summary>
        [Theory]
        [InlineData(Species.Dog, 8)]
        [InlineData(Species.Cat, 5)]
        public async Task SeedData_EmptyDatabase_CreatesAnimalsWithCorrectSpecies(Species species, int expectedCount)
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var count = await context.Animals.CountAsync(a => a.Species == species);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate animals if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_AnimalsExist_DoesNotDuplicate()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingAnimal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Animal",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Cost = 100m,
                ShelterId = Guid.NewGuid().ToString(),
                BreedId = Guid.NewGuid().ToString()
            };

            context.Animals.Add(existingAnimal);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(1, await context.Animals.CountAsync());
            Assert.Equal("Existing Animal", (await context.Animals.FirstAsync()).Name);
        }

        // ========== IMAGE TESTS ==========

        /// <summary>
        /// Tests that SeedData creates images when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesImages()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(9, await context.Images.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData creates images for shelters and animals.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesImagesForSheltersAndAnimals()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var shelterImages = await context.Images.CountAsync(i => i.ShelterId != null);
            var animalImages = await context.Images.CountAsync(i => i.AnimalId != null);

            Assert.Equal(3, shelterImages);
            Assert.Equal(6, animalImages);
        }

        /// <summary>
        /// Tests that principal images are correctly marked.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesPrincipalImages()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            var principalImages = await context.Images.CountAsync(i => i.IsPrincipal);
            Assert.True(principalImages > 0);
            Assert.Equal(8, principalImages);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate images if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_ImagesExist_DoesNotDuplicate()
        {
            // Arrange
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingImage = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/existing.jpg",
                IsPrincipal = true,
                Description = "A very pretty shelter",
                ShelterId = Guid.NewGuid().ToString()
            };

            context.Images.Add(existingImage);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            // Assert
            Assert.Equal(1, await context.Images.CountAsync());
            Assert.Equal("https://example.com/existing.jpg", (await context.Images.FirstAsync()).Url);
        }
    }
}