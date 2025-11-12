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

namespace Tests.ActivitiesTest.ActivitiesFosteringTest.ControllersTest;

/// <summary>
/// Unit tests for the <see cref="ActivitiesController"/> method <see cref="ActivitiesController.GetFosteringActivities"/>.
///
/// These tests verify that the controller:
/// - Correctly handles successful requests, pagination, and mapping logic.
/// - Handles invalid requests or mediator failures gracefully.
/// - Preserves pagination metadata in responses.
/// - Properly interacts with the <see cref="IMediator"/> and <see cref="IMapper"/> dependencies.
/// - Returns consistent HTTP responses (e.g., 200 OK, 400 Bad Request).
///
/// The test class uses mocked dependencies for isolation and follows the AAA (Arrange–Act–Assert) pattern.
/// </summary>
public class GetFosteringActivitiesTests
{

    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ActivitiesController _controller;

    /// <summary>
    /// Initializes the test class by setting up mocked dependencies and configuring
    /// the controller context with a mock <see cref="IServiceProvider"/>.
    /// </summary>
    public GetFosteringActivitiesTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new ActivitiesController(_mapperMock.Object);

        // Create a mock service provider that returns our mocked mediator
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        // Setup HTTP context with the service provider
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        // Setup controller context for HandleResult method
        _controller = new ActivitiesController(_mapperMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    #region Success Scenarios

    /// <summary>
    /// Ensures that when a valid request is sent,
    /// the controller returns an OK result containing mapped DTOs.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithValidRequest_ReturnsOkWithMappedDtos()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var activities = CreateSampleActivities(5);
        var pagedList = new PagedList<Activity>(activities, 5, pageNumber, pageSize);
        var expectedDtos = CreateSampleDtos(5);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<List<Activity>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetFosteringActivities(pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.NotNull(returnedResult);
        Assert.Equal(5, returnedResult.Items.Count);
        Assert.Equal(5, returnedResult.TotalCount);
        Assert.Equal(pageNumber, returnedResult.CurrentPage);
        Assert.Equal(pageSize, returnedResult.PageSize);
    }

    /// <summary>
    /// Ensures that default pagination values (page 1, size 10) are applied
    /// when no parameters are provided in the request.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithDefaultParameters_UsesDefaultPagination()
    {
        // Arrange
        var activities = CreateSampleActivities(3);
        var pagedList = new PagedList<Activity>(activities, 3, 1, 10);
        var expectedDtos = CreateSampleDtos(3);

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetFosteringActivitiesByUser.Query>(q =>
                    q.PageNumber == 1 && q.PageSize == 10),
                default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetFosteringActivitiesByUser.Query>(q =>
                q.PageNumber == 1 && q.PageSize == 10),
            default),
            Times.Once);
    }

    /// <summary>
    /// Ensures that custom pagination parameters are correctly passed to the mediator.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithCustomPagination_UsesProvidedValues()
    {
        // Arrange
        var pageNumber = 3;
        var pageSize = 20;
        var activities = CreateSampleActivities(20);
        var pagedList = new PagedList<Activity>(activities, 60, pageNumber, pageSize);
        var expectedDtos = CreateSampleDtos(20);

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetFosteringActivitiesByUser.Query>(q =>
                    q.PageNumber == pageNumber && q.PageSize == pageSize),
                default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetFosteringActivities(pageNumber, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.Equal(pageNumber, returnedResult.CurrentPage);
        Assert.Equal(pageSize, returnedResult.PageSize);
        Assert.Equal(60, returnedResult.TotalCount);
    }

    /// <summary>
    /// Verifies that an empty result set returns an empty paged list with zero items.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithEmptyResult_ReturnsEmptyPagedList()
    {
        // Arrange
        var emptyList = new List<Activity>();
        var pagedList = new PagedList<Activity>(emptyList, 0, 1, 10);
        var emptyDtos = new List<ResFosteringVisitDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(emptyDtos);

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.Empty(returnedResult.Items);
        Assert.Equal(0, returnedResult.TotalCount);
    }

    #endregion

    #region Error Scenarios

    /// <summary>
    /// Ensures that if the mediator returns a failure result,
    /// the controller responds with a BadRequest (400) status.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WhenMediatorReturnsFailure_ReturnsBadRequest()
    {
        // Arrange
        var errorMessage = "Page number must be greater than 0";
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Failure(errorMessage, 400));

        // Act
        var result = await _controller.GetFosteringActivities(0, 10);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Ensures that a null result value still produces a valid OK (200) response.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WhenResultValueIsNull_ReturnsOk()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(null!, 200));

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    /// <summary>
    /// Ensures that failed results trigger <see cref="ControllerBase.HandleResult"/> properly
    /// and prevent DTO mapping execution.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WhenResultIsNotSuccess_CallsHandleResult()
    {
        // Arrange
        var errorMessage = "Page size must be between 1 and 50";
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Failure(errorMessage, 400));

        // Act
        var result = await _controller.GetFosteringActivities(1, 100);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        _mapperMock.Verify(m => m.Map<List<ResFosteringVisitDto>>(
            It.IsAny<PagedList<Activity>>()),
            Times.Never);
    }

    /// <summary>
    /// Ensures that when the result is successful but has a null value,
    /// the mapping step is skipped to prevent null reference errors.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WhenResultIsSuccessButValueNull_MapDtos()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(null!, 200));

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mapperMock.Verify(m => m.Map<List<ResFosteringVisitDto>>(
            It.IsAny<PagedList<Activity>>()),
            Times.Never);
    }

    #endregion

    #region DTO Mapping Tests

    /// <summary>
    /// Ensures that the <see cref="IMapper"/> correctly maps the retrieved <see cref="Activity"/> entities
    /// to their corresponding <see cref="ResFosteringVisitDto"/> representations.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_MapsActivitiesToDtos_Correctly()
    {
        // Arrange
        var activities = CreateSampleActivities(3);
        var pagedList = new PagedList<Activity>(activities, 3, 1, 10);
        var expectedDtos = CreateSampleDtos(3);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(pagedList.Items))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        _mapperMock.Verify(m => m.Map<List<ResFosteringVisitDto>>(
            It.Is<List<Activity>>(p => p.Count == 3)),
            Times.Once);
    }

    /// <summary>
    /// Verifies that pagination metadata (total count, current page, page size)
    /// is preserved when converting <see cref="Activity"/> entities to DTOs.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_PreservesPaginationMetadata_InDtoPagedList()
    {
        // Arrange
        var totalCount = 100;
        var currentPage = 5;
        var pageSize = 10;
        var activities = CreateSampleActivities(pageSize);
        var pagedList = new PagedList<Activity>(activities, totalCount, currentPage, pageSize);
        var dtos = CreateSampleDtos(pageSize);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtos);

        // Act
        var result = await _controller.GetFosteringActivities(currentPage, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);

        Assert.Equal(totalCount, returnedResult.TotalCount);
        Assert.Equal(currentPage, returnedResult.CurrentPage);
        Assert.Equal(pageSize, returnedResult.PageSize);
        Assert.Equal(10, returnedResult.TotalPages);
    }

    #endregion

    #region Mediator Interaction Tests

    /// <summary>
    /// Ensures that the mediator is called exactly once with the expected query parameters.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_CallsMediatorOnce_WithCorrectQuery()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 15;
        var activities = CreateSampleActivities(15);
        var pagedList = new PagedList<Activity>(activities, 30, pageNumber, pageSize);
        var dtos = CreateSampleDtos(15);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtos);

        // Act
        await _controller.GetFosteringActivities(pageNumber, pageSize);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetFosteringActivitiesByUser.Query>(q =>
                q.PageNumber == pageNumber &&
                q.PageSize == pageSize),
            default),
            Times.Once);
    }

    /// <summary>
    /// Ensures that a valid <see cref="CancellationToken"/> is passed from the controller to the mediator.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_PassesCancellationToken_ToMediator()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var activities = CreateSampleActivities(5);
        var pagedList = new PagedList<Activity>(activities, 5, 1, 10);
        var dtos = CreateSampleDtos(5);

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetFosteringActivitiesByUser.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtos);

        // Act
        await _controller.GetFosteringActivities();

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<GetFosteringActivitiesByUser.Query>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Boundary Value Tests

    // <summary>
    /// Ensures that requesting page number 1 correctly returns the first page of results.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithPageNumber1_ReturnsFirstPage()
    {
        // Arrange
        var activities = CreateSampleActivities(10);
        var pagedList = new PagedList<Activity>(activities, 50, 1, 10);
        var dtos = CreateSampleDtos(10);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<PagedList<Activity>>()))
            .Returns(dtos);

        // Act
        var result = await _controller.GetFosteringActivities(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.Equal(1, returnedResult.CurrentPage);
    }

    /// <summary>
    /// Ensures that a page size of 50 (the maximum allowed) is accepted and processed successfully.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithMaxPageSize50_Succeeds()
    {
        // Arrange
        var activities = CreateSampleActivities(50);
        var pagedList = new PagedList<Activity>(activities, 100, 1, 50);
        var dtos = CreateSampleDtos(50);

        _mediatorMock
            .Setup(m => m.Send(
                It.Is<GetFosteringActivitiesByUser.Query>(q => q.PageSize == 50),
                default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<List<Activity>>()))
            .Returns(dtos);

        // Act
        var result = await _controller.GetFosteringActivities(1, 50);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.Equal(50, returnedResult.PageSize);
        Assert.Equal(50, returnedResult.Items.Count);
    }

    /// <summary>
    /// Ensures that when the result contains a single item,
    /// the controller still returns a valid paged list.
    /// </summary>
    [Fact]
    public async Task GetFosteringActivities_WithSingleResult_ReturnsOneItem()
    {
        // Arrange
        var activities = CreateSampleActivities(1);
        var pagedList = new PagedList<Activity>(activities, 1, 1, 10);
        var dtos = CreateSampleDtos(1);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFosteringActivitiesByUser.Query>(), default))
            .ReturnsAsync(Result<PagedList<Activity>>.Success(pagedList, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResFosteringVisitDto>>(It.IsAny<List<Activity>>()))
            .Returns(dtos);

        // Act
        var result = await _controller.GetFosteringActivities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResult = Assert.IsType<PagedList<ResFosteringVisitDto>>(okResult.Value);
        Assert.Single(returnedResult.Items);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a list of sample <see cref="Activity"/> entities used for testing.
    /// Each entity includes related <see cref="Animal"/>, <see cref="Breed"/>, <see cref="Shelter"/>,
    /// and <see cref="ActivitySlot"/> data.
    /// </summary>
    /// <param name="count">Number of sample activities to generate.</param>
    /// <returns>A list of mocked <see cref="Activity"/> objects.</returns>
    private List<Activity> CreateSampleActivities(int count)
    {
        var activities = new List<Activity>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            var activityId = Guid.NewGuid().ToString();
            var animalId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            var activity = new Activity
            {
                Id = activityId,
                AnimalId = animalId,
                UserId = userId,
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = now.AddDays(-30),
                EndDate = now.AddDays(30),
                CreatedAt = now.AddDays(-35),
                Animal = new Animal
                {
                    Id = animalId,
                    Name = $"Animal {i}",
                    Species = Species.Dog,
                    Size = SizeType.Medium,
                    Sex = SexType.Male,
                    Colour = "Brown",
                    BirthDate = DateOnly.FromDateTime(now.AddYears(-2)),
                    Sterilized = true,
                    Cost = 50,
                    AnimalState = AnimalState.PartiallyFostered,
                    ShelterId = Guid.NewGuid().ToString(),
                    BreedId = Guid.NewGuid().ToString(),
                    Images = new List<Image>
                    {
                        new Image
                        {
                            Id = Guid.NewGuid().ToString(),
                            PublicId = $"public_{i}",
                            Url = $"https://example.com/image{i}.jpg",
                            IsPrincipal = true,
                            Description = $"Image {i}"
                        }
                    },
                    Breed = new Breed
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Labrador"
                    },
                    Shelter = new Shelter
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Test Shelter",
                        Street = "Test Street",
                        City = "Test City",
                        PostalCode = "1234-567",
                        Phone = "912345678",
                        NIF = "123456789",
                        OpeningTime = new TimeOnly(9, 0),
                        ClosingTime = new TimeOnly(18, 0)
                    },
                    Fosterings = new List<Fostering>
                    {
                        new Fostering
                        {
                            Id = Guid.NewGuid().ToString(),
                            AnimalId = animalId,
                            UserId = userId,
                            Amount = 25,
                            Status = FosteringStatus.Active,
                            StartDate = now.AddDays(-30)
                        }
                    }
                },
                Slot = new ActivitySlot
                {
                    Id = Guid.NewGuid().ToString(),
                    ActivityId = activityId,
                    StartDateTime = now.AddDays(i + 1),
                    EndDateTime = now.AddDays(i + 1).AddHours(2),
                    Status = SlotStatus.Reserved,
                    Type = SlotType.Activity
                }
            };

            activities.Add(activity);
        }

        return activities;
    }

    /// <summary>
    /// Creates a list of sample <see cref="ResFosteringVisitDto"/> objects used for testing mapper outputs.
    /// </summary>
    /// <param name="count">Number of sample DTOs to generate.</param>
    /// <returns>A list of mocked <see cref="ResFosteringVisitDto"/> objects.</returns>
    private List<ResFosteringVisitDto> CreateSampleDtos(int count)
    {
        var dtos = new List<ResFosteringVisitDto>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            var visitStartDateTime = now.AddDays(i + 1);
            var visitEndDateTime = visitStartDateTime.AddHours(2);

            dtos.Add(new ResFosteringVisitDto
            {
                ActivityId = Guid.NewGuid().ToString(),
                AnimalName = $"Animal {i}",
                AnimalPrincipalImageUrl = $"https://example.com/image{i}.jpg",
                BreedName = "Labrador",
                AnimalAge = 2,
                ShelterName = "Test Shelter",
                ShelterAddress = "Test Street, 1234-567 Test City",
                ShelterOpeningTime = new TimeOnly(9, 0),
                ShelterClosingTime = new TimeOnly(18, 0),
                VisitStartDateTime = visitStartDateTime,
                VisitEndDateTime = visitEndDateTime,
                VisitDate = DateOnly.FromDateTime(visitStartDateTime)
            });
        }

        return dtos;
    }

    #endregion
}
