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
/// Unit tests for CreateOwnershipActivity endpoint in ActivitiesController.
/// 
/// These tests validate the activity creation workflow, ensuring that:
/// - Owners can successfully schedule visits with their animals
/// - Proper validation is enforced (ownership, scheduling constraints, shelter hours)
/// - All business rules are correctly applied
/// </summary>
public class CreateActivityControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ActivitiesController _controller;

    public CreateActivityControllerTests()
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

    private static Activity CreateActivity()
    {
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var activityId = Guid.NewGuid().ToString();
        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = startDate.AddHours(3);

        return new Activity
        {
            Id = activityId,
            AnimalId = animalId,
            UserId = userId,
            Type = ActivityType.Ownership,
            Status = ActivityStatus.Active,
            StartDate = startDate,
            EndDate = endDate,
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
    public async Task CreateOwnershipActivity_ShouldReturnCreatedResult_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnCorrectAnimalId_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(activity.AnimalId, returnedDto!.AnimalId);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnCorrectUserId_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(activity.UserId, returnedDto!.UserId);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnOwnershipType_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(ActivityType.Ownership, returnedDto!.Type);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnActiveStatus_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(ActivityStatus.Active, returnedDto!.Status);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnCorrectStartDate_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(activity.StartDate, returnedDto!.StartDate);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnCorrectEndDate_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = result.Result as ObjectResult;
        var returnedDto = objectResult!.Value as ResActivityDto;
        Assert.Equal(activity.EndDate, returnedDto!.EndDate);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldCallMediatorWithCorrectCommand_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        await _controller.CreateOwnershipActivity(dto);

        _mockMediator.Verify(m => m.Send(
            It.Is<CreateOwnershipActivity.Command>(c =>
                c.AnimalId == dto.AnimalId &&
                c.StartDate == dto.StartDate &&
                c.EndDate == dto.EndDate),
            default), Times.Once);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldCallMapperWithActivity_WhenRequestIsValid()
    {
        var activity = CreateActivity();
        var dto = new ReqCreateActivityDto
        {
            AnimalId = activity.AnimalId,
            StartDate = activity.StartDate,
            EndDate = activity.EndDate
        };

        var responseDto = CreateResponseDto(activity);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Success(activity, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityDto>(It.IsAny<Activity>()))
            .Returns(responseDto);

        await _controller.CreateOwnershipActivity(dto);

        _mockMapper.Verify(m => m.Map<ResActivityDto>(
            It.IsAny<Activity>()), Times.Once);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("Animal not found", 404));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("You are not the owner of this animal", 403));

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnBadRequest_WhenAnimalDoesNotHaveOwnerStatus()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("Animal does not have owner status", 400));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnForbidden_WhenNoApprovedOwnershipRequestExists()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("No approved ownership request found", 403));

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnBadRequest_WhenStartDateIsLessThan24HoursInAdvance()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddHours(23),
            EndDate = DateTime.UtcNow.AddHours(25)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "Activities must be scheduled at least 24 hours in advance", 400));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("End date must be after start date", 400));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnBadRequest_WhenScheduleIsOutsideShelterHours()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "Schedule must be within shelter hours (09:00-18:00)", 400));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnBadRequest_WhenConflictWithCompletedActivity()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure(
                "New activity must start after the last completed activity ended at 2025-01-01 14:00", 400));

        var result = await _controller.CreateOwnershipActivity(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipActivity_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        var dto = new ReqCreateActivityDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(2).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipActivity.Command>(), default))
            .ReturnsAsync(Result<Activity>.Failure("Failed to create activity", 500));

        var result = await _controller.CreateOwnershipActivity(dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}