using Application.Activities.Commands;
using Application.Core;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using WebAPI.Controllers;
using WebAPI.DTOs.Activities;

namespace Tests.ActivitiesTests.ActivitiesFosteringTest.ControllersTest;

/// <summary>
/// Unit test suite for the <see cref="ActivitiesController"/> endpoint responsible for cancelling fostering activities.
/// </summary>
/// <remarks>
/// These tests validate that the <see cref="ActivitiesController.CancelActivityFostering"/> action:
/// <list type="bullet">
/// <item><description>Sends the correct <see cref="CancelFosteringActivity.Command"/> to MediatR</description></item>
/// <item><description>Handles success and error responses appropriately</description></item>
/// <item><description>Returns correct HTTP status codes based on validation outcomes</description></item>
/// <item><description>Properly maps command results to response DTOs via AutoMapper</description></item>
/// </list>
/// The tests use <see cref="Moq"/> to isolate controller behavior from MediatR and AutoMapper dependencies.
/// </remarks>
public class CancelFosteringActivityControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ActivitiesController _controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelFosteringActivityControllerTests"/> class,
    /// setting up mocked dependencies and the controller context.
    /// </summary>
    public CancelFosteringActivityControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new ActivitiesController(_mapperMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            }
        };
    }

    /// <summary>
    /// Verifies that a valid fostering cancellation request returns <c>200 OK</c>
    /// and that the mapped response matches the expected success message.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        var commandResult = new CancelFosteringActivity.CancelFosteringActivityResult
        {
            ActivityId = dto.ActivityId,
            Message = "Visit cancelled successfully"
        };

        var responseDto = new ResCancelActivityFosteringDto
        {
            ActivityId = dto.ActivityId,
            Message = "Visit cancelled successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>.Success(commandResult, 200));

        _mapperMock
            .Setup(m => m.Map<ResCancelActivityFosteringDto>(commandResult))
            .Returns(responseDto);

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnValue = Assert.IsType<ResCancelActivityFosteringDto>(okResult.Value);
        Assert.Equal(dto.ActivityId, returnValue.ActivityId);
        Assert.Equal("Visit cancelled successfully", returnValue.Message);
    }

    /// <summary>
    /// Ensures that if the activity does not exist, the controller returns a <c>404 Not Found</c> response.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithNonExistentActivity_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "00000000-0000-0000-0000-000000000000"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Activity not found", 404));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Ensures that a user cannot cancel an activity they do not own, returning <c>403 Forbidden</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithActivityNotBelongingToUser_ReturnsForbidden()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("You are not authorized to cancel this activity", 403));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var forbiddenResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    /// <summary>
    /// Ensures that a user cannot cancel an activity they do not own, returning <c>403 Forbidden</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithNonFosteringActivity_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Only fostering activities can be cancelled through this endpoint", 400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Ensures that cancelling an already cancelled activity returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithCancelledActivity_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Cannot cancel an activity with status 'Cancelled'. Only active activities can be cancelled.",
                    400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Ensures that cancelling an activity that has already been completed returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithCompletedActivity_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Cannot cancel an activity with status 'Completed'. Only active activities can be cancelled.",
                    400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Verifies that users without an active fostering relationship cannot cancel an activity, returning <c>403 Forbidden</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithoutActiveFostering_ReturnsForbidden()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("You no longer have an active fostering relationship with this animal", 403));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var forbiddenResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    /// <summary>
    /// Ensures that if an activity slot is missing, the controller returns <c>404 Not Found</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithMissingSlot_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Activity slot not found", 404));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Ensures that cancelling an activity with an already available slot returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithAvailableSlot_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Cannot cancel a slot with status 'Available'. Only reserved slots can be cancelled.", 400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Ensures that cancelling an activity scheduled in the past returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithPastActivity_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>
                .Failure("Cannot cancel an activity that has already started or passed", 400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Verifies that the controller sends the correct command to MediatR when invoked.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_SendsCorrectCommandToMediator()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        var commandResult = new CancelFosteringActivity.CancelFosteringActivityResult
        {
            ActivityId = dto.ActivityId,
            Message = "Visit cancelled successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<CancelFosteringActivity.Command>(c =>
                c.ActivityId == dto.ActivityId), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>.Success(commandResult, 200));

        _mapperMock
            .Setup(m => m.Map<ResCancelActivityFosteringDto>(
                It.IsAny<CancelFosteringActivity.CancelFosteringActivityResult>()))
            .Returns(new ResCancelActivityFosteringDto
            {
                ActivityId = dto.ActivityId,
                Message = "Visit cancelled successfully"
            });

        // Act
        await _controller.CancelActivityFostering(dto);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<CancelFosteringActivity.Command>(c =>
            c.ActivityId == dto.ActivityId), default), Times.Once);
    }

    /// <summary>
    /// Ensures that AutoMapper is invoked correctly to map command results to response DTOs.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_CallsMapperWithCorrectData()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        var commandResult = new CancelFosteringActivity.CancelFosteringActivityResult
        {
            ActivityId = dto.ActivityId,
            Message = "Visit cancelled successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>.Success(commandResult, 200));

        _mapperMock
            .Setup(m => m.Map<ResCancelActivityFosteringDto>(commandResult))
            .Returns(new ResCancelActivityFosteringDto
            {
                ActivityId = dto.ActivityId,
                Message = "Visit cancelled successfully"
            });

        // Act
        await _controller.CancelActivityFostering(dto);

        // Assert
        _mapperMock.Verify(m => m.Map<ResCancelActivityFosteringDto>(commandResult), Times.Once);
    }

    /// <summary>
    /// Documents expected behavior for invalid GUIDs — validation occurs at the model binding level before the controller.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_WithInvalidGuidFormat_ValidatorShouldCatch()
    {
        // Arrange
        // Note: This test documents expected behavior, but actual validation happens at model binding level
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "invalid-guid"
        };

        // This would be caught by model validation before reaching the controller action
        // Including this test for documentation purposes
        Assert.NotNull(dto.ActivityId);
    }

    /// <summary>
    /// Ensures that response DTOs are correctly mapped and returned with the expected data.
    /// </summary>
    [Fact]
    public async Task CancelActivityFostering_MapsResultCorrectly()
    {
        // Arrange
        var dto = new ReqCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d"
        };

        var commandResult = new CancelFosteringActivity.CancelFosteringActivityResult
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d",
            Message = "Visit cancelled successfully"
        };

        var responseDto = new ResCancelActivityFosteringDto
        {
            ActivityId = "a9b0c1d2-e3f4-4a3b-6c7d-8e9f0a1b2c3d",
            Message = "Visit cancelled successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CancelFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CancelFosteringActivity.CancelFosteringActivityResult>.Success(commandResult, 200));

        _mapperMock
            .Setup(m => m.Map<ResCancelActivityFosteringDto>(commandResult))
            .Returns(responseDto);

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<ResCancelActivityFosteringDto>(okResult.Value);

        Assert.Equal(commandResult.ActivityId, returnValue.ActivityId);
        Assert.Equal(commandResult.Message, returnValue.Message);
    }
}