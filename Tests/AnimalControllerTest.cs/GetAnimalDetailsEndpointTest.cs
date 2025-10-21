/*using Application.Animals.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs;

namespace Tests.AnimalControllerTest.cs
{
    /// <summary>
    /// Contains unit tests for the GetAnimalDetails endpoint of the AnimalsController.
    /// </summary>
    /// <remarks>
    /// Verifies different scenarios including invalid IDs, unavailable animals,
    /// and successful retrieval of valid animal details.
    /// </remarks>
    public class GetAnimalDetailsEndpointTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AnimalsController _controller;

        /// <summary>
        /// Initializes the test class and configures mocked dependencies.
        /// </summary>
        public GetAnimalDetailsEndpointTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockMapper = new Mock<IMapper>();
            _controller = new AnimalsController(_mockMapper.Object);

            // Mock HttpContext to provide IMediator
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IMediator)))
                .Returns(_mockMediator.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProviderMock.Object
                }
            };
        }

        /// <summary>
        /// Ensures that invalid or malformed IDs return a BadRequest response.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("not-a-valid-guid")]
        [InlineData("12345")]
        public async Task GetAnimalDetails_InvalidId_ReturnsBadRequest(string invalidId)
        {
            // Arrange
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAnimalDetails.Query>(), default))
                .ReturnsAsync(Result<Animal>.Failure("Invalid ID format", 400));

            // Act
            var result = await _controller.GetAnimalDetails(invalidId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Ensures that non-existent animal IDs return a NotFound response.
        /// </summary>
        [Fact]
        public async Task GetAnimalDetails_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            _mockMediator
                .Setup(m => m.Send(It.Is<GetAnimalDetails.Query>(q => q.Id == validId), default))
                .ReturnsAsync(Result<Animal>.Failure("Animal not found", 404));

            // Act
            var result = await _controller.GetAnimalDetails(validId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Ensures that animals in invalid states (inactive, owned, or fully fostered) return NotFound.
        /// </summary>
        [Theory]
        [InlineData(AnimalState.Inactive)]
        [InlineData(AnimalState.TotallyFostered)]
        [InlineData(AnimalState.HasOwner)]
        public async Task GetAnimalDetails_InvalidAnimalState_ReturnsNotFound(AnimalState invalidState)
        {
            // Arrange
            var validId = Guid.NewGuid().ToString();

            _mockMediator
                .Setup(m => m.Send(It.Is<GetAnimalDetails.Query>(q => q.Id == validId), default))
                .ReturnsAsync(Result<Animal>.Failure("Animal not retrievable", 404));

            // Act
            var result = await _controller.GetAnimalDetails(validId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Animal not retrievable", notFound.Value);
        }

        /// <summary>
        /// Ensures that a valid animal ID returns an Ok response with a complete DTO.
        /// </summary>
        [Fact]
        public async Task GetAnimalDetails_ValidId_ReturnsOkWithCompleteDto()
        {
            // Arrange
            var animalId = Guid.NewGuid().ToString();
            var breedId = Guid.NewGuid().ToString();
            var shelterId = Guid.NewGuid().ToString();

            var animal = new Animal
            {
                Id = animalId,
                Name = "Max",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                Sterilized = true,
                Cost = 150m,
                Description = "Friendly and energetic dog",
                Features = "Good with children, loves to play",
                ShelterId = shelterId,
                BreedId = breedId,
                Breed = new Breed
                {
                    Id = breedId,
                    Name = "Golden Retriever"
                },
                Images = new List<Image>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = "https://example.com/max1.jpg",
                        IsPrincipal = true,
                        Description = "Main photo"
                    }
                }
            };

            var expectedDto = new ResAnimalDTO
            {
                Id = animalId,
                Name = "Max",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                AnimalState = AnimalState.Available,
                Colour = "Brown",
                BirthDate = animal.BirthDate,
                Age = 3,
                Sterilized = true,
                Cost = 150m,
                Description = "Friendly and energetic dog",
                Features = "Good with children, loves to play",
                BreedName = "Golden Retriever",
                Images = new List<ResImageDto>
                {
                    new()
                    {
                        Id = animal.Images.First().Id,
                        Url = "https://example.com/max1.jpg",
                        IsPrincipal = true,
                        Description = "Main photo"
                    }
                }
            };

            _mockMediator
                .Setup(m => m.Send(It.Is<GetAnimalDetails.Query>(q => q.Id == animalId), default))
                .ReturnsAsync(Result<Animal>.Success(animal, 200));

            _mockMapper
                .Setup(m => m.Map<ResAnimalDTO>(It.IsAny<Animal>()))
                .Returns(expectedDto);

            // Act
            var result = await _controller.GetAnimalDetails(animalId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ResAnimalDTO>(okResult.Value);

            Assert.Equal("Max", dto.Name);
            Assert.Equal(Species.Dog, dto.Species);
            Assert.Equal(SizeType.Medium, dto.Size);
            Assert.Equal(AnimalState.Available, dto.AnimalState);
            Assert.Equal("Brown", dto.Colour);
            Assert.True(dto.Sterilized);
            Assert.Equal(150m, dto.Cost);
            Assert.Equal("Golden Retriever", dto.BreedName);
            Assert.Single(dto.Images);
            Assert.True(dto.Images.First().IsPrincipal);
        }

        /// <summary>
        /// Ensures that partially fostered animals return Ok responses.
        /// </summary>
        [Fact]
        public async Task GetAnimalDetails_PartiallyFosteredAnimal_ReturnsOk()
        {
            // Arrange
            var animalId = Guid.NewGuid().ToString();

            var animal = new Animal
            {
                Id = animalId,
                Name = "Luna",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                AnimalState = AnimalState.PartiallyFostered,
                Colour = "White",
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
                Sterilized = true,
                Cost = 80m,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString()
            };

            var expectedDto = new ResAnimalDTO
            {
                Id = animalId,
                Name = "Luna",
                Species = Species.Cat,
                Size = SizeType.Small,
                Sex = SexType.Female,
                AnimalState = AnimalState.PartiallyFostered,
                Colour = "White",
                BirthDate = animal.BirthDate,
                Age = 2,
                Sterilized = true,
                Cost = 80m,
                BreedName = "Mixed"
            };

            _mockMediator
                .Setup(m => m.Send(It.Is<GetAnimalDetails.Query>(q => q.Id == animalId), default))
                .ReturnsAsync(Result<Animal>.Success(animal, 200));

            _mockMapper
                .Setup(m => m.Map<ResAnimalDTO>(It.IsAny<Animal>()))
                .Returns(expectedDto);

            // Act
            var result = await _controller.GetAnimalDetails(animalId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ResAnimalDTO>(okResult.Value);
            Assert.Equal(AnimalState.PartiallyFostered, dto.AnimalState);
        }
    }
}*/
