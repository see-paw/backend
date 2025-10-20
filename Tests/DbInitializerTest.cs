

using Domain;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Unit tests for DbInitializer.
    /// Validates database seeding logic for shelters, breeds, animals, and images.
    /// </summary>
    public class DbInitializerTest
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public DbInitializerTest()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Tests that SeedData creates shelters when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesShelters()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(2, await context.Shelters.CountAsync());
            Assert.Contains(context.Shelters, s => s.Name == "Test Shelter");
            Assert.Contains(context.Shelters, s => s.Name == "Test Shelter 2");
        }

        /// <summary>
        /// Tests that SeedData creates breeds when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesBreeds()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(3, await context.Breeds.CountAsync());
            Assert.Contains(context.Breeds, b => b.Name == "Siamês");
            Assert.Contains(context.Breeds, b => b.Name == "Beagle");
            Assert.Contains(context.Breeds, b => b.Name == "Pastor Alemão");
        }

        /// <summary>
        /// Tests that SeedData creates animals when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesAnimals()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

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
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            var count = await context.Animals.CountAsync(a => a.AnimalState == state);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that SeedData creates images when database is empty.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesImages()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

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
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            var shelterImages = await context.Images.CountAsync(i => i.ShelterId != null);
            var animalImages = await context.Images.CountAsync(i => i.AnimalId != null);

            Assert.Equal(3, shelterImages);
            Assert.Equal(6, animalImages);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate shelters if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_SheltersExist_DoesNotDuplicate()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            var existingShelter = new Shelter
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Shelter",
                Street = "Existing Street",
                City = "Porto",
                PostalCode = "4000-000",
                Phone = "900000000",
                Nif = "000000000",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            };

            context.Shelters.Add(existingShelter);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(1, await context.Shelters.CountAsync());
            Assert.Equal("Existing Shelter", (await context.Shelters.FirstAsync()).Name);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate breeds if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_BreedsExist_DoesNotDuplicate()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            var existingBreed = new Breed
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Existing Breed"
            };

            context.Breeds.Add(existingBreed);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(1, await context.Breeds.CountAsync());
            Assert.Equal("Existing Breed", (await context.Breeds.FirstAsync()).Name);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate animals if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_AnimalsExist_DoesNotDuplicate()
        {
            // Arrange
            using var context = new AppDbContext(_options);

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
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(1, await context.Animals.CountAsync());
            Assert.Equal("Existing Animal", (await context.Animals.FirstAsync()).Name);
        }

        /// <summary>
        /// Tests that SeedData does not duplicate images if already exist.
        /// </summary>
        [Fact]
        public async Task SeedData_ImagesExist_DoesNotDuplicate()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            var existingImage = new Image
            {
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com/existing.jpg",
                IsPrincipal = true,
                ShelterId = Guid.NewGuid().ToString()
            };

            context.Images.Add(existingImage);
            await context.SaveChangesAsync();

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            Assert.Equal(1, await context.Images.CountAsync());
            Assert.Equal("https://example.com/existing.jpg", (await context.Images.FirstAsync()).Url);
        }

        /// <summary>
        /// Tests that SeedData creates entities in correct order (respecting foreign keys).
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesEntitiesInCorrectOrder()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            // Verify shelters and breeds exist first (no FK dependencies)
            Assert.True(await context.Shelters.AnyAsync());
            Assert.True(await context.Breeds.AnyAsync());

            // Verify animals exist (depend on shelters and breeds)
            Assert.True(await context.Animals.AnyAsync());

            // Verify all animals have valid foreign keys
            var animals = await context.Animals.ToListAsync();
            Assert.All(animals, a =>
            {
                Assert.NotNull(a.ShelterId);
                Assert.NotNull(a.BreedId);
            });
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
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            var count = await context.Animals.CountAsync(a => a.Species == species);
            Assert.Equal(expectedCount, count);
        }

        /// <summary>
        /// Tests that principal images are correctly marked.
        /// </summary>
        [Fact]
        public async Task SeedData_EmptyDatabase_CreatesPrincipalImages()
        {
            // Arrange
            using var context = new AppDbContext(_options);

            // Act
            await DbInitializer.SeedData(context);

            // Assert
            var principalImages = await context.Images.CountAsync(i => i.IsPrincipal);
            Assert.True(principalImages > 0);
            Assert.Equal(8, principalImages); // All seeded images are principal
        }
    }
}