using WebAPI.Controllers;
using Application.Activities.Commands;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Domain;
using Domain.Enums;
using WebAPI.DTOs.Activities;

namespace Tests.ActivitiesTest.ActivitiesFosteringTest.ControllersTest;



/// <summary>
/// Unit tests for Activities Controller - ScheduleVisit endpoint with >70% coverage.
/// </summary>
public class CreateActivityControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ActivitiesController _controller;

    // Test constants
    private const string TestAnimalId = "12345678-1234-1234-1234-123456789abc";
    private const string TestUserId = "test-user-123";
    private const string TestActivityId = "test-activity-123";
    private const string TestSlotId = "test-slot-123";
    private const string TestShelterId = "test-shelter-123";
    private const string TestBreedId = "test-breed-123";

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

    #region Helper Methods

    private ReqCreateActivityFosteringDto CreateValidRequest()
    {
        return new ReqCreateActivityFosteringDto
        {
            AnimalId = TestAnimalId,
            StartDateTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(12)
        };
    }

    private ResActivityFosteringDto CreateValidResponse()
    {
        return new ResActivityFosteringDto
        {
            ActivityId = TestActivityId,
            ActivitySlotId = TestSlotId,
            StartDateTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(12),
            Animal = new AnimalVisitInfoDto
            {
                Id = TestAnimalId,
                Name = "Test Animal",
                PrincipalImageUrl = "https://example.com/image.jpg"
            },
            Shelter = new ShelterVisitInfoDto
            {
                Name = "Test Shelter",
                Address = "Test Street, Test City, 1234-567",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            },
            Message = "Visit successfully scheduled"
        };
    }

    /// <summary>
    /// Creates the anonymous object that the Handler returns
    /// This simulates what CreateFosteringActivity.Handler.Handle returns
    /// </summary>
    private object CreateHandlerSuccessResult()
    {
        var activity = new Activity
        {
            Id = TestActivityId,
            AnimalId = TestAnimalId,
            UserId = TestUserId,
            Type = ActivityType.Fostering,
            Status = ActivityStatus.Active,
            StartDate = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndDate = DateTime.UtcNow.AddDays(1).AddHours(12)
        };

        var activitySlot = new ActivitySlot
        {
            Id = TestSlotId,
            ActivityId = TestActivityId,
            StartDateTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(12),
            Status = SlotStatus.Reserved,
            Type = SlotType.Activity
        };

        var animal = new Animal
        {
            Id = TestAnimalId,
            Name = "Test Animal",
            AnimalState = AnimalState.PartiallyFostered,
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = new DateOnly(2020, 1, 1),
            Sterilized = true,
            Cost = 50,
            ShelterId = TestShelterId,
            BreedId = TestBreedId
        };

        // Add principal image
        animal.Images.Add(new Image
        {
            Id = "image-123",
            PublicId = "public-123",
            Url = "https://example.com/image.jpg",
            IsPrincipal = true,
            AnimalId = TestAnimalId
        });

        var shelter = new Shelter
        {
            Id = TestShelterId,
            Name = "Test Shelter",
            Street = "Test Street",
            City = "Test City",
            PostalCode = "1234-567",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        // This is the exact structure returned by the Handler
        return new
        {
            Activity = activity,
            ActivitySlot = activitySlot,
            Animal = animal,
            Shelter = shelter
        };
    }

    #endregion

    #region Success Cases

    [Fact]
    public async Task ScheduleVisit_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = CreateValidRequest();
        var handlerResult = CreateHandlerSuccessResult();
        var expectedResponse = CreateValidResponse();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var createdResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        
        var response = Assert.IsType<ResActivityFosteringDto>(createdResult.Value);
        Assert.Equal(TestActivityId, response.ActivityId);
        Assert.Equal(TestSlotId, response.ActivitySlotId);
        Assert.NotNull(response.Animal);
        Assert.Equal("Test Animal", response.Animal.Name);
        Assert.NotNull(response.Shelter);
        Assert.Equal("Test Shelter", response.Shelter.Name);

        _mockMediator.Verify(
            m => m.Send(It.Is<CreateFosteringActivity.Command>(
                c => c.AnimalId == request.AnimalId 
                     && c.StartDateTime == request.StartDateTime
                     && c.EndDateTime == request.EndDateTime), 
                It.IsAny<CancellationToken>()), 
            Times.Once);

        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.Is<object>(o => o == handlerResult)), 
            Times.Once);
    }

    [Fact]
    public async Task ScheduleVisit_ValidRequest_MapsCommandCorrectly()
    {
        // Arrange
        var request = CreateValidRequest();
        var handlerResult = CreateHandlerSuccessResult();
        var expectedResponse = CreateValidResponse();

        CreateFosteringActivity.Command capturedCommand = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<object>>, CancellationToken>((cmd, ct) => 
            {
                capturedCommand = cmd as CreateFosteringActivity.Command;
            })
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal(request.AnimalId, capturedCommand.AnimalId);
        Assert.Equal(request.StartDateTime, capturedCommand.StartDateTime);
        Assert.Equal(request.EndDateTime, capturedCommand.EndDateTime);
    }

    [Fact]
    public async Task ScheduleVisit_Success_MapperReceivesAnonymousObject()
    {
        // Arrange
        var request = CreateValidRequest();
        var handlerResult = CreateHandlerSuccessResult();
        var expectedResponse = CreateValidResponse();

        object capturedMapperInput = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Callback<object>(input => capturedMapperInput = input)
            .Returns(expectedResponse);

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        Assert.NotNull(capturedMapperInput);
        Assert.Equal(handlerResult, capturedMapperInput);

        // Verify the anonymous object has the expected properties
        var inputType = capturedMapperInput.GetType();
        Assert.NotNull(inputType.GetProperty("Activity"));
        Assert.NotNull(inputType.GetProperty("ActivitySlot"));
        Assert.NotNull(inputType.GetProperty("Animal"));
        Assert.NotNull(inputType.GetProperty("Shelter"));
    }

    #endregion

    #region Failure Cases - Bad Request (400)

    [Fact]
    public async Task ScheduleVisit_InvalidAnimalState_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Animal cannot be visited", 400));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        
        // Mapper should NOT be called on failure
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_ValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Invalid request data", 400));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Failure Cases - Not Found (404)

    [Fact]
    public async Task ScheduleVisit_AnimalNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Animal not found", 404));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_UserNotFostering_ReturnsNotFound()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("You are not currently fostering this animal", 404));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Failure Cases - Conflict (409)

    [Fact]
    public async Task ScheduleVisit_TimeSlotConflict_ReturnsConflict()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("The animal has another visit scheduled during this time", 409));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_ShelterUnavailable_ReturnsConflict()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Shelter is unavailable during the requested time", 409));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_ActivityConflict_ReturnsConflict()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("The animal has another activity scheduled during this time", 409));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Failure Cases - Unprocessable Entity (422)

    [Fact]
    public async Task ScheduleVisit_OutsideOperatingHours_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Visit cannot start before shelter opening time (09:00:00)", 422));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var unprocessableResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(422, unprocessableResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Failure Cases - Internal Server Error (500)

    [Fact]
    public async Task ScheduleVisit_DatabaseFailure_ReturnsInternalServerError()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Failed to create fostering activity", 500));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, errorResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ScheduleVisit_NullRequest_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () =>
        {
            await _controller.ScheduleVisit(null);
        });
        
        // Mediator and Mapper should not be called
        _mockMediator.Verify(
            m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()), 
            Times.Never);
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var request = CreateValidRequest();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mediator error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _controller.ScheduleVisit(request);
        });
        
        Assert.Equal("Mediator error", exception.Message);
        
        // Mapper should not be called
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_MapperThrowsException_PropagatesException()
    {
        // Arrange
        var request = CreateValidRequest();
        var handlerResult = CreateHandlerSuccessResult();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Throws(new Exception("Mapping error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _controller.ScheduleVisit(request);
        });
        
        Assert.Equal("Mapping error", exception.Message);
    }

    #endregion

    #region DTO Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    public async Task ScheduleVisit_InvalidAnimalId_HandledByValidation(string animalId)
    {
        // Arrange
        var request = new ReqCreateActivityFosteringDto
        {
            AnimalId = animalId,
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Animal not found", 404));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_PastStartDate_HandledByBusinessLogic()
    {
        // Arrange
        var request = new ReqCreateActivityFosteringDto
        {
            AnimalId = TestAnimalId,
            StartDateTime = DateTime.UtcNow.AddDays(-1), // Past date
            EndDateTime = DateTime.UtcNow.AddDays(-1).AddHours(2)
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("Visit must be scheduled at least 24 hours in advance", 400));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    [Fact]
    public async Task ScheduleVisit_EndBeforeStart_HandledByBusinessLogic()
    {
        // Arrange
        var request = new ReqCreateActivityFosteringDto
        {
            AnimalId = TestAnimalId,
            StartDateTime = DateTime.UtcNow.AddDays(1).AddHours(12),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(10) // Before start
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Failure("End time must be after start time", 400));

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Never);
    }

    #endregion

    #region Multiple Requests Tests

    [Fact]
    public async Task ScheduleVisit_MultipleSuccessfulRequests_AllSucceed()
    {
        // Arrange
        var request1 = CreateValidRequest();
        var request2 = new ReqCreateActivityFosteringDto
        {
            AnimalId = "87654321-4321-4321-4321-cba987654321",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(12)
        };

        var handlerResult = CreateHandlerSuccessResult();
        var expectedResponse = CreateValidResponse();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Returns(expectedResponse);

        // Act
        var result1 = await _controller.ScheduleVisit(request1);
        var result2 = await _controller.ScheduleVisit(request2);

        // Assert
        var createdResult1 = Assert.IsType<ObjectResult>(result1.Result);
        var createdResult2 = Assert.IsType<ObjectResult>(result2.Result);
        
        Assert.Equal(201, createdResult1.StatusCode);
        Assert.Equal(201, createdResult2.StatusCode);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
        
        _mockMapper.Verify(
            m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()), 
            Times.Exactly(2));
    }

    #endregion

    #region Handler Result Validation

    [Fact]
    public async Task ScheduleVisit_Success_HandlerReturnsCorrectAnonymousObjectStructure()
    {
        // Arrange
        var request = CreateValidRequest();
        var handlerResult = CreateHandlerSuccessResult();
        var expectedResponse = CreateValidResponse();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<object>.Success(handlerResult, 201));

        _mockMapper
            .Setup(m => m.Map<ResActivityFosteringDto>(It.IsAny<object>()))
            .Returns((object input) =>
            {
                // Validate the structure of the anonymous object
                var type = input.GetType();
                var activityProp = type.GetProperty("Activity");
                var slotProp = type.GetProperty("ActivitySlot");
                var animalProp = type.GetProperty("Animal");
                var shelterProp = type.GetProperty("Shelter");

                Assert.NotNull(activityProp);
                Assert.NotNull(slotProp);
                Assert.NotNull(animalProp);
                Assert.NotNull(shelterProp);

                var activity = activityProp.GetValue(input) as Activity;
                var slot = slotProp.GetValue(input) as ActivitySlot;
                var animal = animalProp.GetValue(input) as Animal;
                var shelter = shelterProp.GetValue(input) as Shelter;

                Assert.NotNull(activity);
                Assert.NotNull(slot);
                Assert.NotNull(animal);
                Assert.NotNull(shelter);

                Assert.Equal(TestActivityId, activity.Id);
                Assert.Equal(TestSlotId, slot.Id);
                Assert.Equal(TestAnimalId, animal.Id);
                Assert.Equal(TestShelterId, shelter.Id);

                return expectedResponse;
            });

        // Act
        var result = await _controller.ScheduleVisit(request);

        // Assert
        Assert.IsType<ObjectResult>(result.Result);
    }

    #endregion
}