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
/// 
/// These tests validate the status transition workflow from Pending or Rejected to Analysing, ensuring that:
/// - Only Pending or Rejected requests can be moved to Analysing status
/// - Animals must be available (not inactive or already owned)
/// - Proper authorization is enforced (shelter administrators only)
/// - Optional analysis notes can be added
/// - All business rules and validations are correctly applied
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
    /// Tests that transitioning from Pending to Analysing with notes returns OK.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_PendingToAnalysingWithNotes_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();
        var updatedAt = DateTime.UtcNow;

        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Reviewing applicant's credentials"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "Reviewing applicant's credentials",
            UpdatedAt = updatedAt,
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
            RequestInfo = "Reviewing applicant's credentials",
            UpdatedAt = updatedAt
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Analysing, returnedDto.Status);
        Assert.Equal("Reviewing applicant's credentials", returnedDto.RequestInfo);
        Assert.NotNull(returnedDto.UpdatedAt);
    }

    /// <summary>
    /// Tests that transitioning from Rejected to Analysing returns OK.
    /// This supports the workflow where rejected users can provide additional information
    /// and have their request reconsidered.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_RejectedToAnalysing_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();

        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "User provided additional references, re-analyzing"
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
            RequestInfo = "User provided additional references, re-analyzing",
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
            RequestInfo = "User provided additional references, re-analyzing"
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Analysing, returnedDto.Status);
    }

    /// <summary>
    /// Tests that transitioning without request info is allowed.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_WithoutRequestInfo_ReturnsOk()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            Amount = 100m,
            Status = OwnershipStatus.Analysing,
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
            Status = OwnershipStatus.Analysing,
            RequestInfo = null
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(OwnershipStatus.Analysing, returnedDto.Status);
        Assert.Null(returnedDto.RequestInfo);
    }

    /// <summary>
    /// Tests that attempting to update a non-existent request returns NotFound.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_RequestNotFound_ReturnsNotFound()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        var result = await _controller.UpdateStatus(requestId, dto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Ownership request not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that attempting to move an Approved or Analysing request returns BadRequest.
    /// Only Pending or Rejected requests can transition to Analysing.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_RequestAlreadyApprovedOrAnalysing_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only pending or rejected requests can be moved to analysis", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only pending or rejected requests can be moved to analysis", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to analyze a request for an animal that already has an owner returns BadRequest.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_AnimalAlreadyHasOwner_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Cannot analyze request, animal already has an owner", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Cannot analyze request, animal already has an owner", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to analyze a request for an inactive animal returns BadRequest.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_AnimalInactive_ReturnsBadRequest()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Cannot analyze request, animal is inactive", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Cannot analyze request, animal is inactive", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to update a request from a different shelter returns Forbidden.
    /// Shelter administrators can only update requests for animals in their own shelter.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_RequestFromDifferentShelter_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You can only update requests for animals in your shelter", 403));

        var result = await _controller.UpdateStatus(requestId, dto);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal("You can only update requests for animals in your shelter", forbiddenResult.Value);
    }

    /// <summary>
    /// Tests that attempting to update without being a shelter administrator returns Forbidden.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_NotShelterAdministrator_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Non Authorized Request: Only shelter administrators can update ownership requests", 403));

        var result = await _controller.UpdateStatus(requestId, dto);

        var forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    /// <summary>
    /// Tests that the shelter referenced by the administrator exists.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_ShelterNotFound_ReturnsNotFound()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Shelter not found", 404));

        var result = await _controller.UpdateStatus(requestId, dto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Shelter not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that a database failure during update returns InternalServerError.
    /// </summary>
    [Fact]
    public async Task UpdateStatus_DatabaseFailure_ReturnsInternalServerError()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to update ownership request status", 500));

        var result = await _controller.UpdateStatus(requestId, dto);

        var serverErrorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("Failed to update ownership request status", serverErrorResult.Value);
    }
}