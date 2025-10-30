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

        #region User & Role Tests

        /// <summary>
        /// Tests that SeedData creates all required roles.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesAllRoles()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.True(await roleManager.RoleExistsAsync("PlatformAdmin"));
            Assert.True(await roleManager.RoleExistsAsync("AdminCAA"));
            Assert.True(await roleManager.RoleExistsAsync("User"));
        }

        /// <summary>
        /// Tests that SeedData creates 6 users with correct data.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_Creates6Users()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(6, userManager.Users.Count());
        }

        /// <summary>
        /// Tests that Bob is assigned PlatformAdmin role.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_AssignsPlatformAdminRole()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var bob = await userManager.FindByEmailAsync("bob@test.com");
            Assert.NotNull(bob);
            Assert.True(await userManager.IsInRoleAsync(bob, "PlatformAdmin"));
        }

        /// <summary>
        /// Tests that Alice and Filipe are assigned AdminCAA role and have ShelterId.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_AssignsAdminCAARole()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var alice = await userManager.FindByEmailAsync("alice@test.com");
            Assert.NotNull(alice);
            Assert.True(await userManager.IsInRoleAsync(alice, "AdminCAA"));
            Assert.NotNull(alice.ShelterId);
            
            var filipe = await userManager.FindByEmailAsync("filipe@test.com");
            Assert.NotNull(filipe);
            Assert.True(await userManager.IsInRoleAsync(filipe, "AdminCAA"));
            Assert.NotNull(filipe.ShelterId);
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
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

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
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

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

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(1, userManager.Users.Count());
            Assert.Equal("Existing User", userManager.Users.First().Name);
        }

        /// <summary>
        /// Tests that all users have valid phone numbers.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesUsersWithValidPhoneNumbers()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var users = userManager.Users.ToList();
            Assert.All(users, user => Assert.NotNull(user.PhoneNumber));
            Assert.All(users, user => Assert.Matches(@"^9\d{8}$", user.PhoneNumber));
        }

        #endregion

        #region Shelter Tests

        /// <summary>
        /// Tests that SeedData creates shelters when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesShelters()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(2, await context.Shelters.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData does not duplicate shelters if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_SheltersExist_DoesNotDuplicate()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingShelter = new Shelter
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Shelter",
                Street = "Test Street",
                City = "Porto",
                PostalCode = "4000-000",
                Phone = "900000000",
                NIF = "999999999",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            };

            context.Shelters.Add(existingShelter);
            await context.SaveChangesAsync();

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(1, await context.Shelters.CountAsync());
            Assert.Equal("Existing Shelter", (await context.Shelters.FirstAsync()).Name);
        }

        #endregion

        #region Breed Tests

        /// <summary>
        /// Tests that SeedData creates breeds when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesBreeds()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(3, await context.Breeds.CountAsync());
        }

        #endregion

        #region Animal Tests

        /// <summary>
        /// Tests that SeedData creates 13 animals when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_Creates13Animals()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(18, await context.Animals.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData creates animals with different states.
        /// </summary>
        [Theory]
        [InlineData(AnimalState.Available, 10)]
        [InlineData(AnimalState.Inactive, 2)]
        [InlineData(AnimalState.HasOwner, 2)]
        [InlineData(AnimalState.TotallyFostered, 2)]
        [InlineData(AnimalState.PartiallyFostered, 2)]
        public async Task SeedData_EmptyDatabase_CreatesAnimalsWithDifferentStates(AnimalState state, int expectedCount)
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var count = await context.Animals.CountAsync(a => a.AnimalState == state);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that seeded animals have correct species distribution.
        /// </summary>
        [Theory]
        [InlineData(Species.Dog, 11)]
        [InlineData(Species.Cat, 7)]
        public async Task SeedData_EmptyDatabase_CreatesAnimalsWithCorrectSpecies(Species species, int expectedCount)
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var count = await context.Animals.CountAsync(a => a.Species == species);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate animals if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_AnimalsExist_DoesNotDuplicate()
        {
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

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(1, await context.Animals.CountAsync());
            Assert.Equal("Existing Animal", (await context.Animals.FirstAsync()).Name);
        }

        #endregion

        #region Image Tests

        /// <summary>
        /// Tests that SeedData creates 18 images when database is empty.
        /// 4 shelter images (2 per shelter) + 14 animal images (2 per animal for 7 animals).
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesImages()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(18, await context.Images.CountAsync());
        }

        /// <summary>
        /// Tests that SeedData creates images for shelters and animals with correct distribution.
        /// 4 shelter images and 14 animal images.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesImagesForSheltersAndAnimals()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var shelterImages = await context.Images.CountAsync(i => i.ShelterId != null);
            var animalImages = await context.Images.CountAsync(i => i.AnimalId != null);

            Assert.Equal(4, shelterImages);
            Assert.Equal(14, animalImages);
        }

        /// <summary>
        /// Tests that principal images are correctly marked.
        /// 2 shelter principal images (1 per shelter) + 7 animal principal images = 9 total.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesPrincipalImages()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var principalImages = await context.Images.CountAsync(i => i.IsPrincipal);
            Assert.Equal(9, principalImages);
        }

        /// <summary>
        /// Tests that each shelter has exactly one principal image.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_EachShelterHasOnePrincipalImage()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var shelters = await context.Shelters.Include(s => s.Images).ToListAsync();
            
            foreach (var shelter in shelters)
            {
                var principalCount = shelter.Images.Count(i => i.IsPrincipal);
                Assert.Equal(1, principalCount);
            }
        }

        /// <summary>
        /// Tests that each animal with images has exactly one principal image.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_AnimalsWithImagesHaveOnePrincipalImage()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var animalsWithImages = await context.Animals
                .Include(a => a.Images)
                .Where(a => a.Images.Any())
                .ToListAsync();
            
            Assert.Equal(7, animalsWithImages.Count);
            
            foreach (var animal in animalsWithImages)
            {
                var principalCount = animal.Images.Count(i => i.IsPrincipal);
                Assert.Equal(1, principalCount);
            }
        }

        /// <summary>
        /// Tests that SeedData does not duplicate images if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_ImagesExist_DoesNotDuplicate()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            var existingImage = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/existing.jpg",
                IsPrincipal = true,
                Description = "A very pretty shelter",
                ShelterId = Guid.NewGuid().ToString(),
                PublicId = "images_cq2q0f"
            };

            context.Images.Add(existingImage);
            await context.SaveChangesAsync();

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(1, await context.Images.CountAsync());
            Assert.Equal("https://example.com/existing.jpg", (await context.Images.FirstAsync()).Url);
        }

        #endregion

        #region Fostering Tests

        /// <summary>
        /// Tests that SeedData creates fosterings when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesFosterings()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            Assert.Equal(2, await context.Fosterings.CountAsync());
        }

        /// <summary>
        /// Tests that all created fosterings have Active status.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesFosteringsWithActiveStatus()
        {
            await using var context = new AppDbContext(_options);
            var (userManager, roleManager) = await CreateUserAndRoleManagers(context);

            await DbInitializer.SeedData(context, userManager, roleManager, _loggerFactory);

            var fosterings = await context.Fosterings.ToListAsync();
            Assert.All(fosterings, f => Assert.Equal(FosteringStatus.Active, f.Status));
        }

        #endregion
    }
}