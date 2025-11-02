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
using WebAPI.DTOs.Ownership;

namespace Tests.OwnershipRequestsTest;

/// <summary>
/// Unit tests for ApproveRequest endpoint in OwnershipRequestsController.
/// 
/// These tests validate the approval workflow for ownership requests, ensuring that:
/// - Only requests in 'Analysing' status can be approved
/// - Animals must be available (not inactive or already owned)
/// - Proper authorization is enforced (shelter administrators only)
/// - All business rules and validations are correctly applied
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
    /// Tests that a valid approval request returns OK with updated ownership request details.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_ValidRequestInAnalysing_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var approvedAt = DateTime.UtcNow;

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Approved,
            ApprovedAt = approvedAt,
            UpdatedAt = approvedAt,
            Animal = new Animal
            {
                Id = animalId,
                Name = "Test Animal",
                AnimalState = AnimalState.HasOwner,
                OwnerId = userId,
                OwnershipStartDate = approvedAt
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
            ApprovedAt = approvedAt,
            UpdatedAt = approvedAt
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.ApproveRequest(requestId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Approved, returnedDto.Status);
        Assert.NotNull(returnedDto.ApprovedAt);
        Assert.Equal(approvedAt, returnedDto.ApprovedAt);
    }

    /// <summary>
    /// Tests that attempting to approve a non-existent request returns NotFound.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_RequestNotFound_ReturnsNotFound()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        var result = await _controller.ApproveRequest(requestId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Ownership request not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve a request for an animal that already has an owner returns BadRequest.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_AnimalAlreadyHasOwner_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal already has an owner", 400));

        var result = await _controller.ApproveRequest(requestId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Animal already has an owner", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve a request for an inactive animal returns BadRequest.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_AnimalInactive_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal is inactive", 400));

        var result = await _controller.ApproveRequest(requestId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Animal is inactive", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve a request not in 'Analysing' status returns BadRequest.
    /// Only requests that are currently being analyzed can be approved.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_RequestNotInAnalysing_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only requests in 'Analysing' status can be approved", 400));

        var result = await _controller.ApproveRequest(requestId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only requests in 'Analysing' status can be approved", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve a request when another request is already approved returns BadRequest.
    /// This ensures only one ownership request per animal can be approved.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_AlreadyApprovedRequestExists_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Animal already has an approved ownership request", 400));

        var result = await _controller.ApproveRequest(requestId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Animal already has an approved ownership request", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve a request from a different shelter returns Forbidden.
    /// Shelter administrators can only approve requests for animals in their own shelter.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_RequestFromDifferentShelter_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You can only approve requests for animals in your shelter", 403));

        var result = await _controller.ApproveRequest(requestId);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal("You can only approve requests for animals in your shelter", forbiddenResult.Value);
    }

    /// <summary>
    /// Tests that attempting to approve without being a shelter administrator returns Forbidden.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_NotShelterAdministrator_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only shelter administrators can approve ownership requests", 403));

        var result = await _controller.ApproveRequest(requestId);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal("Only shelter administrators can approve ownership requests", forbiddenResult.Value);
    }

    /// <summary>
    /// Tests that a database failure during approval returns InternalServerError.
    /// </summary>
    [Fact]
    public async Task ApproveRequest_DatabaseFailure_ReturnsInternalServerError()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to approve ownership request", 500));

        var result = await _controller.ApproveRequest(requestId);

        var serverErrorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("Failed to approve ownership request", serverErrorResult.Value);
    }
}