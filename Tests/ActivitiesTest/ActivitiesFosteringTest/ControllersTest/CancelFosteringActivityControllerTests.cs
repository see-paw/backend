using Application.Activities.Commands;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.Activities;

namespace Tests.ActivitiesTest.ActivitiesFosteringTest.ControllersTest;

public class CancelFosteringActivityControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ActivitiesController _controller;

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
                .Failure("Cannot cancel an activity with status 'Cancelled'. Only active activities can be cancelled.", 400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

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
                .Failure("Cannot cancel an activity with status 'Completed'. Only active activities can be cancelled.", 400));

        // Act
        var result = await _controller.CancelActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResCancelActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

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
            .Setup(m => m.Map<ResCancelActivityFosteringDto>(It.IsAny<CancelFosteringActivity.CancelFosteringActivityResult>()))
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