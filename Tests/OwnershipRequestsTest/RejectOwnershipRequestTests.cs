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
/// 
/// These tests validate the rejection workflow for ownership requests, ensuring that:
/// - Only requests in 'Analysing' status can be rejected
/// - Optional rejection reasons are properly stored and visible to users
/// - Proper authorization is enforced (shelter administrators only)
/// - All business rules and validations are correctly applied
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

        _controller = new OwnershipRequestsController(_mockMapper.Object);

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

    /// <summary>
    /// Tests that a valid rejection with a reason returns OK with updated request details.
    /// </summary>
    [Fact]
    public async Task RejectRequest_ValidRequestWithReason_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var rejectedAt = DateTime.UtcNow;

        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "User does not meet shelter requirements"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Rejected,
            RequestInfo = "User does not meet shelter requirements",
            UpdatedAt = rejectedAt,
            Animal = new Animal { Id = animalId, Name = "Test Animal" },
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
            Status = OwnershipStatus.Rejected,
            RequestInfo = "User does not meet shelter requirements",
            UpdatedAt = rejectedAt
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Rejected, returnedDto.Status);
        Assert.Equal("User does not meet shelter requirements", returnedDto.RequestInfo);
        Assert.NotNull(returnedDto.UpdatedAt);
    }

    /// <summary>
    /// Tests that rejection without a reason is allowed and returns OK.
    /// </summary>
    [Fact]
    public async Task RejectRequest_WithoutReason_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto();

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Rejected,
            RequestInfo = null,
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
            RequestInfo = null
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Rejected, returnedDto.Status);
        Assert.Null(returnedDto.RequestInfo);
    }

    /// <summary>
    /// Tests that attempting to reject a non-existent request returns NotFound.
    /// </summary>
    [Fact]
    public async Task RejectRequest_RequestNotFound_ReturnsNotFound()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        var result = await _controller.RejectRequest(requestId, dto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Ownership request not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that attempting to reject a request not in 'Analysing' status returns BadRequest.
    /// Only requests currently under analysis can be rejected.
    /// </summary>
    [Fact]
    public async Task RejectRequest_RequestNotInAnalysing_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only requests in 'Analysing' status can be rejected", 400));

        var result = await _controller.RejectRequest(requestId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only requests in 'Analysing' status can be rejected", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to reject a request from a different shelter returns Forbidden.
    /// Shelter administrators can only reject requests for animals in their own shelter.
    /// </summary>
    [Fact]
    public async Task RejectRequest_RequestFromDifferentShelter_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You can only reject requests for animals in your shelter", 403));

        var result = await _controller.RejectRequest(requestId, dto);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal("You can only reject requests for animals in your shelter", forbiddenResult.Value);
    }

    /// <summary>
    /// Tests that attempting to reject without being a shelter administrator returns Forbidden.
    /// </summary>
    [Fact]
    public async Task RejectRequest_NotShelterAdministrator_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only shelter administrators can reject ownership requests", 403));

        var result = await _controller.RejectRequest(requestId, dto);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal("Only shelter administrators can reject ownership requests", forbiddenResult.Value);
    }

    /// <summary>
    /// Tests that a database failure during rejection returns InternalServerError.
    /// </summary>
    [Fact]
    public async Task RejectRequest_DatabaseFailure_ReturnsInternalServerError()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to reject ownership request", 500));

        var result = await _controller.RejectRequest(requestId, dto);

        var serverErrorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("Failed to reject ownership request", serverErrorResult.Value);
    }
}