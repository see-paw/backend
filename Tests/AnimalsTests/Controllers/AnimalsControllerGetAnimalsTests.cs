using Application.Animals.Filters;
using Application.Animals.Queries;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.Animals;

namespace Tests.AnimalsTests.Controllers;

/// <summary>
/// Tests for AnimalsController.GetAnimals using Equivalence Class Partitioning and Boundary Value Analysis.
/// Tests parameter validation, mapping, pagination boundaries, and response handling.
/// </summary>
public class AnimalsControllerGetAnimalsTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly AnimalsController _controller;

    public AnimalsControllerGetAnimalsTests()
    {
        _mapperMock = new Mock<IMapper>();
        _mediatorMock = new Mock<IMediator>();
        _userAccessorMock = new Mock<IUserAccessor>();
        
        _controller = new AnimalsController(_mapperMock.Object, _userAccessorMock.Object);
        
        var mediatorField = typeof(AnimalsController).BaseType!
            .GetField("_mediator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mediatorField!.SetValue(_controller, _mediatorMock.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAnimals_PageNumberBelowBoundary_ReturnsFailure(int pageNumber)
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Failure("Page number must be 1 or greater", 404));

        var result = await _controller.GetAnimals(filterDto, null, null, pageNumber);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        var objectResult = result.Result as ObjectResult;
        Assert.Equal(404, objectResult!.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    public async Task GetAnimals_PageNumberAtAndAboveBoundary_CallsHandler(int pageNumber)
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, pageNumber);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAnimalList.Query>(q => q.PageNumber == pageNumber),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAnimals_NullSortByAndOrder_PassesNullToQuery()
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAnimalList.Query>(q => q.SortBy == null && q.Order == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("name", "asc")]
    [InlineData("age", "desc")]
    [InlineData("created", "asc")]
    [InlineData("INVALID", "INVALID")]
    public async Task GetAnimals_WithSortParameters_PassesToQuery(string sortBy, string order)
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, sortBy, order, 1);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAnimalList.Query>(q => q.SortBy == sortBy && q.Order == order),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAnimals_EmptyFilterDto_MapsAndPasses()
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mapperMock.Verify(m => m.Map<AnimalFilterModel>(filterDto), Times.Once);
    }

    [Fact]
    public async Task GetAnimals_FullyPopulatedFilterDto_MapsAllProperties()
    {
        var filterDto = new AnimalFilterDto
        {
            Age = 5,
            Breed = "Labrador",
            Name = "Buddy",
            Sex = "Male",
            ShelterName = "Happy",
            Size = "Medium",
            Species = "Dog"
        };
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mapperMock.Verify(m => m.Map<AnimalFilterModel>(filterDto), Times.Once);
    }

    [Fact]
    public async Task GetAnimals_HandlerReturnsFailure_ReturnsFailureResponse()
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Failure("No animals found", 404));

        var result = await _controller.GetAnimals(filterDto, null, null, 1);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAnimals_HandlerReturnsSuccess_MapsToDto()
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var animals = new List<Animal>
        {
            new Animal
            {
                Id = "1",
                Name = "Buddy",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = "breed-id",
                Cost = 100m,
                ShelterId = "shelter-id"
            }
        };
        var pagedList = new PagedList<Animal>(animals, 1, 1, 20);
        var dtoList = new List<ResAnimalDto>();
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(dtoList);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mapperMock.Verify(m => m.Map<List<ResAnimalDto>>(animals), Times.Once);
    }

    [Theory]
    [InlineData(100, 5, 20)]
    public async Task GetAnimals_DifferentPaginationScenarios_PreservesPaginationInfo(int totalCount, int currentPage, int pageSize)
    {
        var filterDto = new AnimalFilterDto();
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), totalCount, currentPage, pageSize);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        var result = await _controller.GetAnimals(filterDto, null, null, currentPage);

        Assert.IsType<OkObjectResult>(result.Result);
        var objectResult = result.Result as OkObjectResult;
        var value = objectResult!.Value as PagedList<ResAnimalDto>;
        Assert.Equal(totalCount, value!.TotalCount);
        Assert.Equal(currentPage, value.CurrentPage);
        Assert.Equal(pageSize, value.PageSize);
    }

    [Fact]
    public async Task GetAnimals_FilterDtoWithBoundaryValues_PassesThrough()
    {
        var filterDto = new AnimalFilterDto
        {
            Age = 0,
            Breed = new string('a', 100),
            Name = new string('x', 100),
            ShelterName = new string('s', 200)
        };
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mapperMock.Verify(m => m.Map<AnimalFilterModel>(filterDto), Times.Once);
    }

    [Fact]
    public async Task GetAnimals_InvalidEnumsInFilterDto_StillProcesses()
    {
        var filterDto = new AnimalFilterDto
        {
            Sex = "InvalidSex",
            Size = "InvalidSize",
            Species = "InvalidSpecies"
        };
        var filterModel = new AnimalFilterModel();
        var pagedList = new PagedList<Animal>(new List<Animal>(), 0, 1, 20);
        
        _mapperMock.Setup(m => m.Map<AnimalFilterModel>(filterDto)).Returns(filterModel);
        _mapperMock.Setup(m => m.Map<List<ResAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(new List<ResAnimalDto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAnimalList.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        await _controller.GetAnimals(filterDto, null, null, 1);

        _mapperMock.Verify(m => m.Map<AnimalFilterModel>(filterDto), Times.Once);
    }
}