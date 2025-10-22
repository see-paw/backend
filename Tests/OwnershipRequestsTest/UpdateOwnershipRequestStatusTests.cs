using Application.Core;
using Application.OwnershipRequests.Commands;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs;

namespace Tests.OwnershipRequestsTest;

/// <summary>
/// Unit tests for UpdateStatus endpoint in OwnershipRequestsController.
/// </summary>
public class UpdateOwnershipRequestStatusTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public UpdateOwnershipRequestStatusTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _controller = new OwnershipRequestsController();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mockMediator.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMapper)))
            .Returns(_mockMapper.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            }
        };
    }

    [Fact]
    public async Task UpdateStatus_PendingToAnalysing_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Moving to analysis"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "Moving to analysis",
            Animal = new Animal { Id = Guid.NewGuid().ToString(), Name = "Test Animal" },
            User = new User { Id = Guid.NewGuid().ToString(), Name = "Test User" }
        };

        var responseDto = new ResOwnershipRequestDto
        {
            Id = requestId,
            AnimalId = ownershipRequest.AnimalId,
            AnimalName = "Test Animal",
            UserId = ownershipRequest.UserId,
            UserName = "Test User",
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "Moving to analysis"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.UpdateStatus(requestId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Analysing, returnedDto.Status);
    }

    [Fact]
    public async Task UpdateStatus_RequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        // Act
        var result = await _controller.UpdateStatus(requestId, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_NotPendingStatus_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Only pending requests can be moved to analysis", 400));

        // Act
        var result = await _controller.UpdateStatus(requestId, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_WithRequestInfo_UpdatesSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Additional information added"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "Additional information added",
            Animal = new Animal { Id = Guid.NewGuid().ToString(), Name = "Test Animal" },
            User = new User { Id = Guid.NewGuid().ToString(), Name = "Test User" }
        };

        var responseDto = new ResOwnershipRequestDto
        {
            Id = requestId,
            AnimalId = ownershipRequest.AnimalId,
            AnimalName = "Test Animal",
            UserId = ownershipRequest.UserId,
            UserName = "Test User",
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "Additional information added"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.UpdateStatus(requestId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal("Additional information added", returnedDto.RequestInfo);
    }
}