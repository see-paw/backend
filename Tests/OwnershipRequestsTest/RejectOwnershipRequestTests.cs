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
/// Unit tests for RejectRequest endpoint in OwnershipRequestsController.
/// </summary>
public class RejectOwnershipRequestTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public RejectOwnershipRequestTests()
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
    public async Task RejectRequest_ValidRequest_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Does not meet requirements"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Rejected,
            RequestInfo = "Does not meet requirements",
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
            Status = OwnershipStatus.Rejected,
            RequestInfo = "Does not meet requirements"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.RejectRequest(requestId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Rejected, returnedDto.Status);
        Assert.Equal("Does not meet requirements", returnedDto.RequestInfo);
    }

    [Fact]
    public async Task RejectRequest_RequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        // Act
        var result = await _controller.RejectRequest(requestId, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Only requests in 'Analysing' status can be rejected", 400));

        // Act
        var result = await _controller.RejectRequest(requestId, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_WithoutReason_ReturnsOk()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto(); // No rejection reason

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Rejected,
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
            Status = OwnershipStatus.Rejected
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        // Act
        var result = await _controller.RejectRequest(requestId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Rejected, returnedDto.Status);
    }
}