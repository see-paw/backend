using Application.Activities.Queries;
using Application.Core;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.Activities;
using Xunit;

namespace Tests.Activities;

/// <summary>
/// Unit tests for GetOwnershipActivities endpoint in ActivitiesController.
/// 
/// These tests validate the retrieval workflow for ownership activities, ensuring that:
/// - Users can successfully retrieve their ownership activities
/// - Filtering by status works correctly
/// - Pagination is applied properly
/// - All business rules are correctly applied
/// </summary>
public class GetOwnershipActivitiesByUserControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ActivitiesController _controller;

    public GetOwnershipActivitiesByUserControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _controller = new ActivitiesController(_mockMapper.Object);

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

    private static PagedList<Activity> CreatePagedActivities(int count = 3, int pageNumber = 1, int pageSize = 20)
    {
        var activities = new List<Activity>();
        var userId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();

        for (int i = 0; i < count; i++)
        {
            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = animalId,
                UserId = userId,
                Type = ActivityType.Ownership,
                Status = ActivityStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(i + 2),
                EndDate = DateTime.UtcNow.AddDays(i + 2).AddHours(2),
                Animal = new Animal
                {
                    Id = animalId,
                    Name = "Max",
                    Colour = "Brown",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    BirthDate = new DateOnly(2020, 1, 1),
                    Sterilized = true,
                    Cost = 150,
                    BreedId = Guid.NewGuid().ToString(),
                    ShelterId = Guid.NewGuid().ToString(),
                    AnimalState = AnimalState.HasOwner,
                    OwnerId = userId
                },
                User = new User
                {
                    Id = userId,
                    Name = "John Doe",
                    Email = "john@example.com",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    Street = "Test Street",
                    City = "Test City",
                    PostalCode = "1234-567"
                }
            };
            activities.Add(activity);
        }

        return new PagedList<Activity>(activities, count, pageNumber, pageSize);
    }

    private static List<ResActivityDto> CreateResponseDtos(PagedList<Activity> activities)
    {
        return activities.Select(a => new ResActivityDto
        {
            Id = a.Id,
            AnimalId = a.AnimalId,
            AnimalName = a.Animal.Name,
            UserId = a.UserId,
            UserName = a.User.Name,
            Type = a.Type,
            Status = a.Status,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnOkResult_WhenRequestIsValid()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectTotalCount()
    {
        var pagedActivities = CreatePagedActivities(count: 15);
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities();

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Equal(15, pagedResult!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectCurrentPage()
    {
        var pagedActivities = CreatePagedActivities(pageNumber: 2);
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities(pageNumber: 2);

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Equal(2, pagedResult!.CurrentPage);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectPageSize()
    {
        var pagedActivities = CreatePagedActivities(count: 5, pageSize: 5);
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities();

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Equal(5, pagedResult!.PageSize);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnCorrectItemCount()
    {
        var pagedActivities = CreatePagedActivities(count: 7);
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities();

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Equal(7, pagedResult!.Count);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldCallMediatorWithDefaultPageNumber_WhenNotProvided()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        await _controller.GetOwnershipActivities();

        _mockMediator.Verify(m => m.Send(
            It.Is<GetOwnershipActivitiesByUser.Query>(q => q.PageNumber == 1),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldCallMediatorWithProvidedPageNumber()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        await _controller.GetOwnershipActivities(pageNumber: 3);

        _mockMediator.Verify(m => m.Send(
            It.Is<GetOwnershipActivitiesByUser.Query>(q => q.PageNumber == 3),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldCallMediatorWithNullStatus_WhenNotProvided()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        await _controller.GetOwnershipActivities();

        _mockMediator.Verify(m => m.Send(
            It.Is<GetOwnershipActivitiesByUser.Query>(q => q.Status == null),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldCallMediatorWithProvidedStatus()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        await _controller.GetOwnershipActivities(status: "Active");

        _mockMediator.Verify(m => m.Send(
            It.Is<GetOwnershipActivitiesByUser.Query>(q => q.Status == "Active"),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldCallMapperWithActivities()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        await _controller.GetOwnershipActivities();

        _mockMapper.Verify(m => m.Map<List<ResActivityDto>>(
            It.IsAny<PagedList<Activity>>()), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Failure(
                "Invalid status value: InvalidStatus. Valid values are: Active, Completed, Canceled, All", 400));

        var result = await _controller.GetOwnershipActivities(status: "InvalidStatus");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnEmptyList_WhenUserHasNoActivities()
    {
        var emptyPagedActivities = new PagedList<Activity>(new List<Activity>(), 0, 1, 20);
        var emptyDtoList = new List<ResActivityDto>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(emptyPagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(emptyDtoList);

        var result = await _controller.GetOwnershipActivities();

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Empty(pagedResult!);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldReturnZeroTotalCount_WhenUserHasNoActivities()
    {
        var emptyPagedActivities = new PagedList<Activity>(new List<Activity>(), 0, 1, 20);
        var emptyDtoList = new List<ResActivityDto>();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(emptyPagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(emptyDtoList);

        var result = await _controller.GetOwnershipActivities();

        var okResult = result as OkObjectResult;
        var pagedResult = okResult!.Value as PagedList<ResActivityDto>;
        Assert.Equal(0, pagedResult!.TotalCount);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldHandleStatusActive()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities(status: "Active");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldHandleStatusCompleted()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities(status: "Completed");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldHandleStatusCanceled()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities(status: "Canceled");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipActivities_ShouldHandleStatusAll()
    {
        var pagedActivities = CreatePagedActivities();
        var dtoList = CreateResponseDtos(pagedActivities);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedActivities, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResActivityDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtoList);

        var result = await _controller.GetOwnershipActivities(status: "All");

        Assert.IsType<OkObjectResult>(result);
    }
}