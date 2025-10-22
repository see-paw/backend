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
/// Unit tests for CreateOwnershipRequest endpoint in OwnershipRequestsController.
/// </summary>
public class CreateOwnershipRequestTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public CreateOwnershipRequestTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _controller = new OwnershipRequestsController();

        // Mock HttpContext to provide IMediator and IMapper
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
    public async Task CreateOwnershipRequest_ValidRequest_ReturnsOk()
    {
        // Arrange
        var animalId = Guid.NewGuid().ToString();
        var userId = "temporary-user-id";

        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = animalId,
            RequestInfo = "I would like to adopt this animal"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Pending,
            RequestInfo = "I would like to adopt this animal",
            Animal = new Animal { Id = animalId, Name = "Test Animal" },
            User = new User { Id = userId, Name = "Test User" }
        };

        var responseDto = new ResOwnershipRequestDto
        {
            Id = ownershipRequest.Id,
            AnimalId = animalId,
            AnimalName = "Test Animal",
            UserId = userId,
            UserName = "Test User",
            Amount = 100m,
            Status = OwnershipStatus.Pending,
            RequestInfo = "I would like to adopt this animal"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(animalId, returnedDto.AnimalId);
        Assert.Equal(userId, returnedDto.UserId);
        Assert.Equal(OwnershipStatus.Pending, returnedDto.Status);
    }

    [Fact]
    public async Task CreateOwnershipRequest_AnimalNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            RequestInfo = "Test"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not found", 404));

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_AnimalHasOwner_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            RequestInfo = "Test"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            RequestInfo = "Test"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("User not found", 404));

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ExistingRequest_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            RequestInfo = "Test"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("You already have an ownership request for this animal", 400));

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_AnimalInactive_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString(),
            RequestInfo = "Test"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        // Act
        var result = await _controller.CreateOwnershipRequest(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}