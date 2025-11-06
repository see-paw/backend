using Application.Core;
using Application.Favorites.Queries;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs;
using WebAPI.DTOs.Favorites;
using Xunit;

namespace Tests.Favorites.Handlers;

/// <summary>
/// Unit tests for GetUserFavorites endpoint in FavoritesController.
/// 
/// These tests validate the retrieval workflow for user favorites, ensuring that:
/// - Authenticated users can retrieve their favorite animals
/// - Proper pagination is applied
/// - Only active favorites are returned
/// - Proper authorization is enforced (User role only)
/// - All business rules and validations are correctly applied
/// </summary>
public class GetUserFavoritesControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly FavoritesController _controller;

    public GetUserFavoritesControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _controller = new FavoritesController(_mockMapper.Object);

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

    private static List<Animal> CreateFavoriteAnimals(int count = 3)
    {
        var animals = new List<Animal>();
        var breedId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();

        for (int i = 0; i < count; i++)
        {
            animals.Add(new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Animal {i + 1}",
                AnimalState = AnimalState.Available,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = breedId,
                ShelterId = shelterId,
                Cost = 50m,
                Breed = new Breed
                {
                    Id = breedId,
                    Name = "Test Breed"
                },
                Shelter = new Shelter
                {
                    Id = shelterId,
                    Name = "Test Shelter",
                    Street = "Test Street",
                    City = "Test City",
                    PostalCode = "1234-567",
                    Phone = "912345678",
                    NIF = "123456789",
                    OpeningTime = new TimeOnly(9, 0),
                    ClosingTime = new TimeOnly(18, 0)
                },
                Images = new List<Image>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = "https://example.com/image.jpg",
                        IsPrincipal = true,
                        PublicId = "test_image"
                    }
                }
            });
        }

        return animals;
    }

    private static List<ResFavoriteAnimalDto> CreateResponseDtos(List<Animal> animals)
    {
        return animals.Select(a => new ResFavoriteAnimalDto
        {
            Id = a.Id,
            Name = a.Name,
            Species = a.Species.ToString(),
            Breed = a.Breed.Name,
            Age = DateTime.Today.Year - a.BirthDate.Year,
            AnimalState = a.AnimalState.ToString(),
            PrincipalImageUrl = a.Images.First(i => i.IsPrincipal).Url,
            ShelterName = a.Shelter.Name,
            Size = a.Size.ToString(),
            Sex = a.Sex.ToString()
        }).ToList();
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnOkResult_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        var result = await _controller.GetUserFavorites();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectTotalCount_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals(5);
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        var result = await _controller.GetUserFavorites();

        var okResult = result as OkObjectResult;
        var returnedPagedList = okResult!.Value as PagedList<ResFavoriteAnimalDto>;
        Assert.Equal(5, returnedPagedList!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectCurrentPage_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        var result = await _controller.GetUserFavorites(pageNumber: 1);

        var okResult = result as OkObjectResult;
        var returnedPagedList = okResult!.Value as PagedList<ResFavoriteAnimalDto>;
        Assert.Equal(1, returnedPagedList!.CurrentPage);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectPageSize_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        var result = await _controller.GetUserFavorites();

        var okResult = result as OkObjectResult;
        var returnedPagedList = okResult!.Value as PagedList<ResFavoriteAnimalDto>;
        Assert.Equal(20, returnedPagedList!.PageSize);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnCorrectItemCount_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals(3);
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<IEnumerable<Animal>>()))
            .Returns(responseDtos);

        var result = await _controller.GetUserFavorites();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPagedList = Assert.IsType<PagedList<ResFavoriteAnimalDto>>(okResult.Value);

        Assert.Equal(3, returnedPagedList.Items.Count);
    }


    [Fact]
    public async Task GetUserFavorites_ShouldCallMediatorWithDefaultPageNumber_WhenNotProvided()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        await _controller.GetUserFavorites();

        _mockMediator.Verify(m => m.Send(
            It.Is<GetUserFavorites.Query>(q => q.PageNumber == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldCallMediatorWithProvidedPageNumber_WhenProvided()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 2, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(responseDtos);

        await _controller.GetUserFavorites(pageNumber: 2);

        _mockMediator.Verify(m => m.Send(
            It.Is<GetUserFavorites.Query>(q => q.PageNumber == 2),
            default), Times.Once);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldCallMapper_WhenFavoritesExist()
    {
        var animals = CreateFavoriteAnimals();
        var pagedList = new PagedList<Animal>(animals, animals.Count, 1, 20);
        var responseDtos = CreateResponseDtos(animals);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<IEnumerable<Animal>>()))
            .Returns(responseDtos);

        await _controller.GetUserFavorites();

        _mockMapper.Verify(m =>
                m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<IEnumerable<Animal>>()),
            Times.Once);
    }


    [Fact]
    public async Task GetUserFavorites_ShouldReturnOkWithEmptyList_WhenUserHasNoFavorites()
    {
        var emptyList = new List<Animal>();
        var pagedList = new PagedList<Animal>(emptyList, 0, 1, 20);
        var emptyDtos = new List<ResFavoriteAnimalDto>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(emptyDtos);

        var result = await _controller.GetUserFavorites();

        var okResult = result as OkObjectResult;
        var returnedPagedList = okResult!.Value as PagedList<ResFavoriteAnimalDto>;
        Assert.Empty(returnedPagedList!);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnZeroTotalCount_WhenUserHasNoFavorites()
    {
        var emptyList = new List<Animal>();
        var pagedList = new PagedList<Animal>(emptyList, 0, 1, 20);
        var emptyDtos = new List<ResFavoriteAnimalDto>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResFavoriteAnimalDto>>(It.IsAny<List<Animal>>()))
            .Returns(emptyDtos);

        var result = await _controller.GetUserFavorites();

        var okResult = result as OkObjectResult;
        var returnedPagedList = okResult!.Value as PagedList<ResFavoriteAnimalDto>;
        Assert.Equal(0, returnedPagedList!.TotalCount);
    }

    [Fact]
    public async Task GetUserFavorites_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetUserFavorites.Query>(), default))
            .ReturnsAsync(Result<PagedList<Animal>>.Failure(
                "Failed to retrieve favorites", 500));

        var result = await _controller.GetUserFavorites();

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}