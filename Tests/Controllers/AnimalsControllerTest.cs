using Application.Animals.Commands;
using Application.Animals.Queries;
using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Breeds;
using WebAPI.DTOs.Images;

namespace Tests.Controllers;

/// <summary>
/// Unit tests for AnimalsController using equivalence partitioning and boundary value analysis.
/// Tests all HTTP endpoints for animal management operations.
/// </summary>
public class AnimalsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly AnimalsController _controller;

    public AnimalsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _controller = new AnimalsController(_mockMapper.Object, _mockUserAccessor.Object);

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

    #region Helper Methods

    private static Animal CreateValidAnimal(string? id = null)
    {
        return new Animal
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Name = "Test Animal",
            AnimalState = AnimalState.Available,
            Description = "Test description",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50m,
            ShelterId = Guid.NewGuid().ToString(),
            BreedId = Guid.NewGuid().ToString()
        };
    }

    private static ResAnimalDto CreateValidResAnimalDto()
    {
        return new ResAnimalDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Animal",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50m,
            shelterId = Guid.NewGuid().ToString(),
            Breed = new ResBreedDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Breed"
            }
        };
    }

    private static ReqCreateAnimalDto CreateValidReqCreateAnimalDto()
    {
        return new ReqCreateAnimalDto
        {
            Name = "New Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50m,
            BreedId = Guid.NewGuid().ToString(),
            Images = new List<ReqImageDto>
            {
                new ReqImageDto
                {
                    File = CreateMockFormFile().Object,
                    IsPrincipal = true
                }
            }
        };
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName = "test.jpg", long length = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        var stream = new MemoryStream();
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        return mockFile;
    }

    private void SetupUserWithShelter(string shelterId)
    {
        var user = new User { ShelterId = shelterId };
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);
    }

    #endregion

    #region GetAnimals Tests

    /// <summary>
    /// Tests GetAnimals with default page number (1).
    /// Equivalence Class: Valid page number, animals exist.
    /// Boundary: Minimum valid page number.
    /// </summary>
    [Fact]
    public async Task GetAnimals_DefaultPageNumber_ReturnsOk()
    {
        var animals = new List<Animal> { CreateValidAnimal() };
        var pagedList = new PagedList<Animal>(animals, 1, 1, 10);
        var dtoList = new List<ResAnimalDto> { CreateValidResAnimalDto() };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(dtoList);

        var result = await _controller.GetAnimals();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PagedList<ResAnimalDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    /// <summary>
    /// Tests GetAnimals with various valid page numbers.
    /// Equivalence Class: Valid page numbers (positive integers).
    /// Boundary: Page 1, mid-range pages, high page numbers.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetAnimals_ValidPageNumbers_ReturnsOk(int pageNumber)
    {
        var animals = new List<Animal> { CreateValidAnimal() };
        var pagedList = new PagedList<Animal>(animals, 1, pageNumber, 10);
        var dtoList = new List<ResAnimalDto> { CreateValidResAnimalDto() };

        _mockMediator
            .Setup(m => m.Send(It.Is<GetAnimalList.Query>(q => q.PageNumber == pageNumber), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(dtoList);

        var result = await _controller.GetAnimals(pageNumber);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Tests GetAnimals with invalid page numbers.
    /// Equivalence Class: Invalid page numbers (zero, negative).
    /// Boundary: Page 0, page -1.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetAnimals_InvalidPageNumbers_ReturnsBadRequest(int pageNumber)
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Failure("Invalid page number", 400));

        var result = await _controller.GetAnimals(pageNumber);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    /// Tests GetAnimals when no animals exist.
    /// Equivalence Class: Empty result set.
    /// </summary>
    [Fact]
    public async Task GetAnimals_NoAnimalsExist_ReturnsEmptyList()
    {
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 10);
        var dtoList = new List<ResAnimalDto>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(dtoList);

        var result = await _controller.GetAnimals();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<PagedList<ResAnimalDto>>(okResult.Value);
        Assert.Empty(returnValue);
    }

    /// <summary>
    /// Tests GetAnimals when query fails.
    /// Equivalence Class: Database/service errors.
    /// </summary>
    [Fact]
    public async Task GetAnimals_QueryFails_ReturnsInternalServerError()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Failure("Internal error", 500));

        var result = await _controller.GetAnimals();

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    #endregion

    #region GetAnimalDetails Tests

    /// <summary>
    /// Tests GetAnimalDetails with valid GUID.
    /// Equivalence Class: Valid animal ID, animal exists.
    /// </summary>
    [Fact]
    public async Task GetAnimalDetails_ValidId_ReturnsOk()
    {
        var animalId = Guid.NewGuid().ToString();
        var animal = CreateValidAnimal(animalId);
        var dto = CreateValidResAnimalDto();

        _mockMediator
            .Setup(m => m.Send(It.Is<GetAnimalDetails.Query>(q => q.Id == animalId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Success(animal, 200));

        _mockMapper
            .Setup(m => m.Map<ResAnimalDto>(It.IsAny<Animal>()))
            .Returns(dto);

        var result = await _controller.GetAnimalDetails(animalId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ResAnimalDto>(okResult.Value);
    }

    /// <summary>
    /// Tests GetAnimalDetails with non-existent ID.
    /// Equivalence Class: Valid format but animal doesn't exist.
    /// </summary>
    [Fact]
    public async Task GetAnimalDetails_NonExistentId_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalDetails.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Animal not found", 404));

        var result = await _controller.GetAnimalDetails(animalId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    /// Tests GetAnimalDetails with various invalid ID formats.
    /// Equivalence Class: Invalid GUID formats.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-id")]
    [InlineData("123")]
    [InlineData("not-a-guid")]
    public async Task GetAnimalDetails_InvalidIdFormat_ReturnsNotFound(string invalidId)
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalDetails.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Invalid ID format", 404));

        var result = await _controller.GetAnimalDetails(invalidId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region CreateAnimal Tests

    /// <summary>
    /// Tests CreateAnimal with valid data and single image.
    /// Equivalence Class: Valid animal data, valid shelter, minimum images (1).
    /// Boundary: Minimum image count.
    /// </summary>
    [Fact]
    public async Task CreateAnimal_ValidDataSingleImage_ReturnsCreated()
    {
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var dto = CreateValidReqCreateAnimalDto();
        var animal = CreateValidAnimal();
        var animalId = animal.Id;

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqCreateAnimalDto>()))
            .Returns(animal);

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(animalId, 201));

        var result = await _controller.CreateAnimal(dto);

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    /// <summary>
    /// Tests CreateAnimal with multiple images.
    /// Equivalence Class: Valid animal data, multiple images.
    /// Boundary: 2, 5, 10 images.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task CreateAnimal_ValidDataMultipleImages_ReturnsCreated(int imageCount)
    {
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var dto = CreateValidReqCreateAnimalDto();
        dto.Images = new List<ReqImageDto>();
        
        for (int i = 0; i < imageCount; i++)
        {
            dto.Images.Add(new ReqImageDto
            {
                File = CreateMockFormFile($"image{i}.jpg").Object,
                IsPrincipal = i == 0
            });
        }

        var animal = CreateValidAnimal();

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqCreateAnimalDto>()))
            .Returns(animal);

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(animal.Id, 201));

        var result = await _controller.CreateAnimal(dto);

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    /// <summary>
    /// Tests CreateAnimal without shelter ID.
    /// Equivalence Class: User without shelter association.
    /// </summary>
    [Fact]
    public async Task CreateAnimal_NoShelterId_ReturnsUnauthorized()
    {
        var user = new User { ShelterId = null };
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var dto = CreateValidReqCreateAnimalDto();

        var result = await _controller.CreateAnimal(dto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid shelter token", unauthorizedResult.Value);
    }

    /// <summary>
    /// Tests CreateAnimal with empty shelter ID.
    /// Equivalence Class: Empty string shelter ID.
    /// Boundary: Empty vs null shelter ID.
    /// </summary>
    [Fact]
    public async Task CreateAnimal_EmptyShelterId_ReturnsUnauthorized()
    {
        var user = new User { ShelterId = string.Empty };
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var dto = CreateValidReqCreateAnimalDto();

        var result = await _controller.CreateAnimal(dto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid shelter token", unauthorizedResult.Value);
    }

    /// <summary>
    /// Tests CreateAnimal without images.
    /// Equivalence Class: No images provided.
    /// Boundary: Zero images.
    /// </summary>
    [Fact]
    public async Task CreateAnimal_NoImages_ReturnsBadRequest()
    {
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var dto = CreateValidReqCreateAnimalDto();
        dto.Images = new List<ReqImageDto>();

        var result = await _controller.CreateAnimal(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("At least one image is required when creating an animal.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests CreateAnimal with empty file in image.
    /// Equivalence Class: Invalid file (zero length).
    /// Boundary: File length = 0.
    /// </summary>
    [Fact]
    public async Task CreateAnimal_EmptyFile_ReturnsBadRequest()
    {
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var dto = CreateValidReqCreateAnimalDto();
        dto.Images = new List<ReqImageDto>
        {
            new ReqImageDto
            {
                File = CreateMockFormFile("empty.jpg", 0).Object,
                IsPrincipal = true
            }
        };

        var result = await _controller.CreateAnimal(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Each image must include a valid file.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests CreateAnimal with boundary file sizes.
    /// Equivalence Class: Valid file sizes.
    /// Boundary: 1 byte, typical size, large size.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(1048576)]
    [InlineData(5242880)]
    public async Task CreateAnimal_BoundaryFileSizes_ReturnsCreated(long fileSize)
    {
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var dto = CreateValidReqCreateAnimalDto();
        dto.Images = new List<ReqImageDto>
        {
            new ReqImageDto
            {
                File = CreateMockFormFile("image.jpg", fileSize).Object,
                IsPrincipal = true
            }
        };

        var animal = CreateValidAnimal();

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqCreateAnimalDto>()))
            .Returns(animal);

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(animal.Id, 201));

        var result = await _controller.CreateAnimal(dto);

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    #endregion

    #region EditAnimal Tests

    /// <summary>
    /// Tests EditAnimal with valid data.
    /// Equivalence Class: Valid animal ID, valid update data.
    /// </summary>
    [Fact]
    public async Task EditAnimal_ValidData_ReturnsOk()
    {
        var animalId = Guid.NewGuid().ToString();
        var editDto = new ReqEditAnimalDto
        {
            Name = "Updated Name",
            AnimalState = AnimalState.Available,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Black",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 75m,
            BreedId = Guid.NewGuid().ToString()
        };

        var animal = CreateValidAnimal(animalId);
        var responseDto = CreateValidResAnimalDto();

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqEditAnimalDto>()))
            .Returns(animal);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<EditAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Success(animal, 200));

        _mockMapper
            .Setup(m => m.Map<ResAnimalDto>(It.IsAny<Animal>()))
            .Returns(responseDto);

        var result = await _controller.EditAnimal(animalId, editDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ResAnimalDto>(okResult.Value);
    }

    /// <summary>
    /// Tests EditAnimal with non-existent ID.
    /// Equivalence Class: Animal doesn't exist.
    /// </summary>
    [Fact]
    public async Task EditAnimal_NonExistentId_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var editDto = new ReqEditAnimalDto
        {
            Name = "Updated Name",
            BreedId = Guid.NewGuid().ToString(),
            Cost = 50m
        };

        var animal = CreateValidAnimal(animalId);

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqEditAnimalDto>()))
            .Returns(animal);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<EditAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Animal not found", 404));

        var result = await _controller.EditAnimal(animalId, editDto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    /// Tests EditAnimal with boundary cost values.
    /// Equivalence Class: Valid cost range [0, 1000].
    /// Boundary: 0, 0.01, 999.99, 1000.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(50.00)]
    [InlineData(999.99)]
    [InlineData(1000)]
    public async Task EditAnimal_BoundaryCostValues_ReturnsOk(decimal cost)
    {
        var animalId = Guid.NewGuid().ToString();
        var editDto = new ReqEditAnimalDto
        {
            Name = "Test",
            Cost = cost,
            BreedId = Guid.NewGuid().ToString()
        };

        var animal = CreateValidAnimal(animalId);
        animal.Cost = cost;
        var responseDto = CreateValidResAnimalDto();

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqEditAnimalDto>()))
            .Returns(animal);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<EditAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Success(animal, 200));

        _mockMapper
            .Setup(m => m.Map<ResAnimalDto>(It.IsAny<Animal>()))
            .Returns(responseDto);

        var result = await _controller.EditAnimal(animalId, editDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Tests EditAnimal with all valid AnimalState transitions.
    /// Equivalence Class: All valid enum states.
    /// </summary>
    [Theory]
    [InlineData(AnimalState.Available)]
    [InlineData(AnimalState.PartiallyFostered)]
    [InlineData(AnimalState.TotallyFostered)]
    [InlineData(AnimalState.HasOwner)]
    [InlineData(AnimalState.Inactive)]
    public async Task EditAnimal_AllValidStates_ReturnsOk(AnimalState state)
    {
        var animalId = Guid.NewGuid().ToString();
        var editDto = new ReqEditAnimalDto
        {
            Name = "Test",
            AnimalState = state,
            BreedId = Guid.NewGuid().ToString(),
            Cost = 50m
        };

        var animal = CreateValidAnimal(animalId);
        animal.AnimalState = state;
        var responseDto = CreateValidResAnimalDto();

        _mockMapper
            .Setup(m => m.Map<Animal>(It.IsAny<ReqEditAnimalDto>()))
            .Returns(animal);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<EditAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Success(animal, 200));

        _mockMapper
            .Setup(m => m.Map<ResAnimalDto>(It.IsAny<Animal>()))
            .Returns(responseDto);

        var result = await _controller.EditAnimal(animalId, editDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region AddImagesToAnimal Tests

    /// <summary>
    /// Tests AddImagesToAnimal with valid data and single image.
    /// Equivalence Class: Valid animal, minimum images (1).
    /// Boundary: Minimum image count.
    /// </summary>
    [Fact]
    public async Task AddImagesToAnimal_SingleImage_ReturnsCreated()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>
            {
                new ReqImageDto
                {
                    File = CreateMockFormFile().Object,
                    IsPrincipal = false,
                    Description = "Test image"
                }
            }
        };

        var domainImages = new List<Image>
        {
            new Image
            {
                Id = Guid.NewGuid().ToString(),
                PublicId = "test-public-id",
                Url = "https://test.com/image.jpg",
                IsPrincipal = false,
                Description = "Test image"
            }
        };

        var responseDtos = new List<ResImageDto>
        {
            new ResImageDto
            {
                Id = domainImages[0].Id,
                PublicId = domainImages[0].PublicId,
                Url = domainImages[0].Url,
                IsPrincipal = false,
                Description = "Test image"
            }
        };

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<AddImagesAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Image>>.Success(domainImages, 201));

        _mockMapper
            .Setup(m => m.Map<List<ResImageDto>>(It.IsAny<List<Image>>()))
            .Returns(responseDtos);

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedDtos = Assert.IsType<List<ResImageDto>>(createdResult.Value);
        Assert.Single(returnedDtos);
    }

    /// <summary>
    /// Tests AddImagesToAnimal with multiple images.
    /// Equivalence Class: Multiple images.
    /// Boundary: 2, 5, 10 images.
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task AddImagesToAnimal_MultipleImages_ReturnsCreated(int imageCount)
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>()
        };

        for (int i = 0; i < imageCount; i++)
        {
            dto.Images.Add(new ReqImageDto
            {
                File = CreateMockFormFile($"image{i}.jpg").Object,
                IsPrincipal = i == 0,
                Description = $"Test image {i}"
            });
        }

        var domainImages = new List<Image>();
        var responseDtos = new List<ResImageDto>();

        for (int i = 0; i < imageCount; i++)
        {
            var img = new Image
            {
                Id = Guid.NewGuid().ToString(),
                PublicId = $"test-public-id-{i}",
                Url = $"https://test.com/image{i}.jpg",
                IsPrincipal = i == 0,
                Description = $"Test image {i}"
            };
            domainImages.Add(img);

            responseDtos.Add(new ResImageDto
            {
                Id = img.Id,
                PublicId = img.PublicId,
                Url = img.Url,
                IsPrincipal = img.IsPrincipal,
                Description = img.Description
            });
        }

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<AddImagesAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Image>>.Success(domainImages, 201));

        _mockMapper
            .Setup(m => m.Map<List<ResImageDto>>(It.IsAny<List<Image>>()))
            .Returns(responseDtos);

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnedDtos = Assert.IsType<List<ResImageDto>>(createdResult.Value);
        Assert.Equal(imageCount, returnedDtos.Count);
    }

    /// <summary>
    /// Tests AddImagesToAnimal with no images.
    /// Equivalence Class: Empty image list.
    /// Boundary: Zero images.
    /// </summary>
    [Fact]
    public async Task AddImagesToAnimal_NoImages_ReturnsBadRequest()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>()
        };

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("No image was provided", badRequestResult.Value);
    }

    /// <summary>
    /// Tests AddImagesToAnimal with non-existent animal.
    /// Equivalence Class: Animal doesn't exist.
    /// </summary>
    [Fact]
    public async Task AddImagesToAnimal_NonExistentAnimal_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>
            {
                new ReqImageDto
                {
                    File = CreateMockFormFile().Object,
                    IsPrincipal = false
                }
            }
        };

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<AddImagesAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Image>>.Failure("Animal not found", 404));

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    /// Tests AddImagesToAnimal when image upload fails.
    /// Equivalence Class: Image service failure.
    /// </summary>
    [Fact]
    public async Task AddImagesToAnimal_ImageUploadFails_ReturnsBadRequest()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>
            {
                new ReqImageDto
                {
                    File = CreateMockFormFile().Object,
                    IsPrincipal = false
                }
            }
        };

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<AddImagesAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Image>>.Failure("Image upload failed: Cloudinary error", 400));

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Image upload failed", badRequestResult.Value?.ToString());
    }

    /// <summary>
    /// Tests AddImagesToAnimal with mismatch between files and metadata.
    /// Equivalence Class: Invalid input - count mismatch.
    /// </summary>
    [Fact]
    public async Task AddImagesToAnimal_MismatchedFilesAndMetadata_ReturnsBadRequest()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqAddImagesDto
        {
            Images = new List<ReqImageDto>
            {
                new ReqImageDto
                {
                    File = CreateMockFormFile().Object,
                    IsPrincipal = false
                }
            }
        };

        _mockMapper
            .Setup(m => m.Map<List<Image>>(It.IsAny<List<ReqImageDto>>()))
            .Returns(new List<Image>());

        _mockMediator
            .Setup(m => m.Send(It.IsAny<AddImagesAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Image>>.Failure("Mismatch between files and image metadata.", 400));

        var result = await _controller.AddImagesToAnimal(animalId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Mismatch between files and image metadata.", badRequestResult.Value);
    }

    #endregion

    #region DeleteAnimalImage Tests

    /// <summary>
    /// Tests DeleteAnimalImage with valid IDs.
    /// Equivalence Class: Valid animal and image IDs, image exists.
    /// </summary>
    [Fact]
    public async Task DeleteAnimalImage_ValidIds_ReturnsNoContent()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteAnimalImage.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 204));

        var result = await _controller.DeleteAnimalImage(animalId, imageId);

        Assert.IsType<NoContentResult>(result.Result);
    }

    /// <summary>
    /// Tests DeleteAnimalImage with non-existent image.
    /// Equivalence Class: Image doesn't exist.
    /// </summary>
    [Fact]
    public async Task DeleteAnimalImage_NonExistentImage_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteAnimalImage.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Image not found", 404));

        var result = await _controller.DeleteAnimalImage(animalId, imageId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    /// Tests DeleteAnimalImage with non-existent animal.
    /// Equivalence Class: Animal doesn't exist.
    /// </summary>
    [Fact]
    public async Task DeleteAnimalImage_NonExistentAnimal_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeleteAnimalImage.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Animal not found", 404));

        var result = await _controller.DeleteAnimalImage(animalId, imageId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region SetAnimalPrincipalImage Tests

    /// <summary>
    /// Tests SetAnimalPrincipalImage with valid IDs.
    /// Equivalence Class: Valid animal and image IDs, image belongs to animal.
    /// </summary>
    [Fact]
    public async Task SetAnimalPrincipalImage_ValidIds_ReturnsNoContent()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<SetAnimalPrincipalImage.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value, 204));

        var result = await _controller.SetAnimalPrincipalImage(animalId, imageId);

        Assert.IsType<NoContentResult>(result.Result);
    }

    /// <summary>
    /// Tests SetAnimalPrincipalImage with non-existent image.
    /// Equivalence Class: Image doesn't exist or doesn't belong to animal.
    /// </summary>
    [Fact]
    public async Task SetAnimalPrincipalImage_NonExistentImage_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var imageId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<SetAnimalPrincipalImage.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Image not found", 404));

        var result = await _controller.SetAnimalPrincipalImage(animalId, imageId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    #endregion

    #region DeactivateAnimal Tests

    /// <summary>
    /// Tests DeactivateAnimal with valid data.
    /// Equivalence Class: Valid animal, belongs to shelter, no active associations.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_ValidData_ReturnsOk()
    {
        var animalId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        var animal = CreateValidAnimal(animalId);
        animal.AnimalState = AnimalState.Inactive;
        var responseDto = CreateValidResAnimalDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeactivateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Success(animal, 200));

        _mockMapper
            .Setup(m => m.Map<ResAnimalDto>(It.IsAny<Animal>()))
            .Returns(responseDto);

        var result = await _controller.DeactivateAnimal(animalId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ResAnimalDto>(okResult.Value);
    }

    /// <summary>
    /// Tests DeactivateAnimal without shelter ID.
    /// Equivalence Class: User without shelter association.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_NoShelterId_ReturnsUnauthorized()
    {
        var user = new User { ShelterId = null };
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var animalId = Guid.NewGuid().ToString();

        var result = await _controller.DeactivateAnimal(animalId);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid shelter token", unauthorizedResult.Value);
    }

    /// <summary>
    /// Tests DeactivateAnimal with empty shelter ID.
    /// Equivalence Class: Empty shelter ID.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_EmptyShelterId_ReturnsUnauthorized()
    {
        var user = new User { ShelterId = string.Empty };
        _mockUserAccessor.Setup(x => x.GetUserAsync()).ReturnsAsync(user);

        var animalId = Guid.NewGuid().ToString();

        var result = await _controller.DeactivateAnimal(animalId);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid shelter token", unauthorizedResult.Value);
    }

    /// <summary>
    /// Tests DeactivateAnimal with non-existent animal.
    /// Equivalence Class: Animal doesn't exist.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_NonExistentAnimal_ReturnsNotFound()
    {
        var animalId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeactivateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Animal not found", 404));

        var result = await _controller.DeactivateAnimal(animalId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Tests DeactivateAnimal with animal from different shelter.
    /// Equivalence Class: Animal doesn't belong to user's shelter.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_DifferentShelter_ReturnsForbidden()
    {
        var animalId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeactivateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Not authorized", 403));

        var result = await _controller.DeactivateAnimal(animalId);

        var forbiddenResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    /// <summary>
    /// Tests DeactivateAnimal with active associations.
    /// Equivalence Class: Animal has active fosterings or ownership.
    /// </summary>
    [Fact]
    public async Task DeactivateAnimal_HasActiveAssociations_ReturnsConflict()
    {
        var animalId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();
        SetupUserWithShelter(shelterId);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<DeactivateAnimal.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Animal>.Failure("Cannot deactivate animal with active associations", 409));

        var result = await _controller.DeactivateAnimal(animalId);

        Assert.IsType<ConflictObjectResult>(result);
    }

    #endregion
}