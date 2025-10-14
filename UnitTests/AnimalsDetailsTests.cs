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
    public class AnimalsDetailsTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly AnimalsController _controller;
        private readonly Mock<IMapper> _mockMapper;

        public AnimalsDetailsTests()
        {
            // Criar mock do DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _mockMapper = new Mock<IMapper>();
            _controller = new AnimalsController(_mockContext.Object, _mockMapper.Object);
        }

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

            var expectedDto = new AnimalDTO
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

            _mockMapper.Setup(m => m.Map<AnimalDTO>(It.IsAny<Animal>()))
                .Returns(expectedDto);

            // Act
            var result = await _controller.GetAnimalDetails(validId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<AnimalDTO>(ok.Value);
        }
    }
}
