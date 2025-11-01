using Application.Activities.Commands;
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
/// Unit tests for CancelOwnershipActivity endpoint in ActivitiesController.
/// 
/// These tests validate the cancellation workflow for ownership activities, ensuring that:
/// - Owners can successfully cancel their scheduled activities
/// - Proper validation is enforced (activity exists, user is owner, valid status)
/// - Only Active activities can be cancelled
/// - All business rules are correctly applied
/// </summary>
public class CancelOwnershipActivityControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ActivitiesController _controller;

    public CancelOwnershipActivityControllerTests()
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

    private static Activity CreateActivity(ActivityStatus status = ActivityStatus.Active)
    {
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var activityId = Guid.NewGuid().ToString();

        return new Activity
        {
            Id = activityId,
            AnimalId = animalId,
            UserId = userId,
            Type = ActivityType.Ownership,
            Status = status,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(3),
            Animal = new Animal
            {
                Id = animalId,
                Name = "Max",
                Cost = 150,
                Colour = "Brown",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                AnimalState = AnimalState.HasOwner,
                OwnerId = userId
            },
            User = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Street = "Test Street",
                City = "Test City",
                PostalCode = "1234-567"
            }
        };
    }

    private static ResActivityDto CreateResponseDto(Activity activity)
    {
        return new ResActivityDto
        {
            Id = activity.Id,
            AnimalId = activity.AnimalId,
            AnimalName = activity.Animal.Name,
            UserId = activity.UserId,
            UserName = activity.User.Name,
            Type = activity.Type,
            Status = activity.Status,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate,
            CreatedAt = activity.CreatedAt
        };
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnOkResult_WhenCancellationIsSuccessful()
    {
        var activityId = Guid.NewGuid().ToString();
        var activity = CreateActivity(ActivityStatus.Cancelled);
        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 200));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CancelOwnershipActivity(activityId);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnCancelledStatus_WhenCancellationIsSuccessful()
    {
        var activityId = Guid.NewGuid().ToString();
        var activity = CreateActivity(ActivityStatus.Cancelled);
        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 200));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CancelOwnershipActivity(activityId);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResActivityDto;
        Assert.Equal(ActivityStatus.Cancelled, returnedDto!.Status);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnCorrectActivityId()
    {
        var activityId = Guid.NewGuid().ToString();
        var activity = CreateActivity(ActivityStatus.Cancelled);
        activity.Id = activityId;
        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 200));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CancelOwnershipActivity(activityId);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResActivityDto;
        Assert.Equal(activityId, returnedDto!.Id);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldCallMediatorWithCorrectCommand()
    {
        var activityId = Guid.NewGuid().ToString();
        var activity = CreateActivity(ActivityStatus.Cancelled);
        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 200));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        await _controller.CancelOwnershipActivity(activityId);

        _mockMediator.Verify(m => m.Send(
            It.Is<CancelOwnershipActivity.Command>(c => c.ActivityId == activityId),
            default), Times.Once);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldCallMapperWithActivity()
    {
        var activityId = Guid.NewGuid().ToString();
        var activity = CreateActivity(ActivityStatus.Cancelled);
        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 200));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        await _controller.CancelOwnershipActivity(activityId);

        _mockMapper.Verify(m => m.Map<ResActivityDto>(
            It.IsAny<Activity>()), Times.Once);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("Activity not found", 404));

        var result = await _controller.CancelOwnershipActivity(activityId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "You are not authorized to cancel this activity", 403));

        var result = await _controller.CancelOwnershipActivity(activityId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsNotOwnershipType()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "Only ownership activities can be cancelled through this endpoint", 400));

        var result = await _controller.CancelOwnershipActivity(activityId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsAlreadyCancelled()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "This activity has already been cancelled", 400));

        var result = await _controller.CancelOwnershipActivity(activityId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnBadRequest_WhenActivityIsCompleted()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "Cannot cancel a completed activity", 400));

        var result = await _controller.CancelOwnershipActivity(activityId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CancelOwnershipActivity_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        var activityId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CancelOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "Failed to cancel activity", 500));

        var result = await _controller.CancelOwnershipActivity(activityId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}