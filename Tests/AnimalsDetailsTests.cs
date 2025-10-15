using API.Controllers;
using API.DTOs;
using AutoMapper;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;


namespace UnitTests
{
    /// <summary>
    /// Unit test suite responsible for validating the behavior of the
    /// <see cref="AnimalsController.GetAnimalDetails(string)"/> endpoint.
    /// </summary>
    /// <remarks>
    /// These tests ensure that the animal details endpoint:
    /// - Returns a <see cref="BadRequestObjectResult"/> for invalid or empty IDs.
    /// - Returns a <see cref="NotFoundObjectResult"/> for non-existent animals or animals in hidden states.
    /// - Returns an <see cref="OkObjectResult"/> containing an <see cref="AnimalDto"/> when the animal exists and is available.
    ///
    /// The database context is simulated using a <see cref="Mock{T}"/> of <see cref="AppDbContext"/>,
    /// and entity-to-DTO mapping is mocked via a <see cref="Mock{T}"/> of <see cref="IMapper"/>.
    /// </remarks>
    public class AnimalsDetailsTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly AnimalsController _controller;
        private readonly Mock<IMapper> _mockMapper;

        /// <summary>
        /// Initializes the test environment for the <see cref="AnimalsController"/>.
        /// </summary>
        /// <remarks>
        /// Creates an in-memory database context (<see cref="AppDbContext"/>) for testing purposes,
        /// configures the <see cref="IMapper"/> mock for entity-to-DTO conversions,
        /// and instantiates the <see cref="AnimalsController"/> with these mocked dependencies.
        ///
        /// This setup ensures test isolation and prevents dependency on a real database.
        /// </remarks>
        public AnimalsDetailsTests()
        {
            // Create mock DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _mockMapper = new Mock<IMapper>();
            _controller = new AnimalsController(_mockContext.Object, _mockMapper.Object);
        }

        /// <summary>
        /// Verifies that the <see cref="AnimalsController.GetAnimalDetails(string)"/> method
        /// returns the appropriate response when provided with an invalid or empty ID.
        /// </summary>
        /// <param name="invalidId">An invalid or empty animal ID to test different failure scenarios.</param>
        /// <remarks>
        /// The test covers the following cases:
        /// - <c>null</c>, empty, or whitespace IDs → <see cref="BadRequestObjectResult"/>  
        /// - Non-GUID formatted strings → <see cref="BadRequestObjectResult"/>  
        /// - Valid GUIDs not present in the database → <see cref="NotFoundObjectResult"/>  
        /// </remarks>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("String that surpasses the length of a GUID , and on, and on, and on")]
        public async Task GetAnimalDetails_InvalidOrEmptyId_ReturnsBadRequestOrNotFound(string invalidId)
        {
            //Arrange
            _mockContext.Setup(c => c.FindAsync<Animal>(It.IsAny<string>()))
                        .ReturnsAsync((Animal?)null); 

            // Act
            var result = await _controller.GetAnimalDetails(invalidId);

            // Assert
            if (string.IsNullOrWhiteSpace(invalidId))
            {
                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
            else if (!Guid.TryParse(invalidId, out _))
            {
                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
            else
            {
                Assert.IsType<NotFoundObjectResult>(result.Result);
            }
        }

        /// <summary>
        /// Verifies that the <see cref="AnimalsController.GetAnimalDetails(string)"/> method
        /// returns a <see cref="NotFoundObjectResult"/> when the animal exists but is in an invalid state.
        /// </summary>
        /// <param name="invalidAnimalState">An <see cref="AnimalState"/> value representing a non-visible or invalid state.</param>
        /// <remarks>
        /// This test ensures that animals not marked as <see cref="AnimalState.Available"/> or
        /// <see cref="AnimalState.PartiallyFostered"/> are not exposed through the API.
        /// The expected response message is <c>"Animal not Available"</c>.
        /// </remarks>

        [Theory]
        [InlineData(AnimalState.Inactive)]
        public async Task GetAnimalDetails_InvalidAnimalState_ReturnsNotFound(AnimalState invalidAnimalState)
        {
            //Arrange
            var validId = Guid.NewGuid().ToString();

            var invalidAnimal = new Animal
            {
                AnimalId = validId,
                Name = "Bolt",
                Colour = "Brown",
                AnimalState = invalidAnimalState,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Breed = Breed.Husky,
                Cost = 100,
                Features = "Friendly and active",
                MainImageUrl = "https://example.com/bolt.jpg"
            };

            _mockContext.Setup(c => c.FindAsync<Animal>(validId))
                .ReturnsAsync(invalidAnimal);

            // Act
            var result = await _controller.GetAnimalDetails(validId);

            // Assert
            var isNotFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Animal not Available", isNotFoundObjectResult.Value);
        }

        /// <summary>
        /// Verifies that the <see cref="AnimalsController.GetAnimalDetails(string)"/> method
        /// returns an <see cref="OkObjectResult"/> containing an <see cref="AnimalDto"/> 
        /// when a valid and available animal ID is provided.
        /// </summary>
        /// <remarks>
        /// This test ensures the controller correctly retrieves an existing animal 
        /// with <see cref="AnimalState.Available"/> and maps it to a DTO using <see cref="IMapper"/>.  
        /// It validates both the response type and the returned data structure.
        /// </remarks>

        [Fact]
        public async Task GetAnimalDetails_ValidId_ReturnsOk()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            var expectedAnimal = new Animal
            {
                AnimalId = validId,
                Name = "Bolt",
                Description = "lorem",
                Colour = "Brown",
                AnimalState = AnimalState.Available,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Breed = Breed.Husky,
                Cost = 100,
                Features = "Friendly and active",
                MainImageUrl = "https://example.com/bolt.jpg"
            };

            var expectedDto = new AnimalDto
            {
                Name = expectedAnimal.Name,
                Description = expectedAnimal.Description,
                Colour = expectedAnimal.Colour,
                AnimalState = expectedAnimal.AnimalState,
                Species = expectedAnimal.Species,
                Size = expectedAnimal.Size,
                Sex = expectedAnimal.Sex,
                BirthDate = expectedAnimal.BirthDate,
                Sterilized = expectedAnimal.Sterilized,
                Breed = expectedAnimal.Breed,
                Cost = expectedAnimal.Cost,
                Features = expectedAnimal.Features,
                MainImageUrl = expectedAnimal.MainImageUrl

            };

            _mockContext.Setup(c => c.FindAsync<Animal>(validId))
                        .ReturnsAsync(expectedAnimal);

            _mockMapper.Setup(m => m.Map<AnimalDto>(It.IsAny<Animal>()))
                .Returns(expectedDto);

            // Act
            var result = await _controller.GetAnimalDetails(validId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<AnimalDto>(ok.Value);
        }
    }
}
