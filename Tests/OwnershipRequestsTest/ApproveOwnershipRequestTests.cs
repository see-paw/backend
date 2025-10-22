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
/// Unit tests for ApproveRequest endpoint in OwnershipRequestsController.
/// </summary>
public class ApproveOwnershipRequestTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public ApproveOwnershipRequestTests()
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
    public async Task ApproveRequest_ValidRequest_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Approved,
            ApprovedAt = DateTime.UtcNow,
            Animal = new Animal
            {
                Id = animalId,
                Name = "Test Animal",
                AnimalState = AnimalState.HasOwner,
                OwnerId = userId
            },
            User = new User { Id = userId, Name = "Test User" }
        };

        var responseDto = new ResOwnershipRequestDto
        {
            Id = requestId,
            AnimalId = animalId,
            AnimalName = "Test Animal",
            UserId = userId,
            UserName = "Test User",
            Amount = 100m,
            Status = OwnershipStatus.Approved,
            ApprovedAt = ownershipRequest.ApprovedAt
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Approved, returnedDto.Status);
        Assert.NotNull(returnedDto.ApprovedAt);
    }

    [Fact]
    public async Task ApproveRequest_RequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_AnimalAlreadyHasOwner_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal already has an owner", 400));

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_AnimalInactive_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal is inactive", 400));

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Only requests in 'Analysing' or 'Rejected' status can be approved", 400));

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_AlreadyApprovedRequest_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal already has an approved ownership request", 400));

        // Act
        var result = await _controller.ApproveRequest(requestId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}