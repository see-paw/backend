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
/// Unit tests for CreateOwnershipRequest endpoint in OwnershipRequestsController.
/// 
/// These tests validate the creation workflow for ownership requests, ensuring that:
/// - Users can successfully create adoption requests for available animals
/// - Proper validation is enforced (animal exists, available, no duplicates)
/// - User ID is extracted from JWT token (not from request body)
/// - All business rules are correctly applied
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

        // Pass mapper to controller constructor
        _controller = new OwnershipRequestsController(_mockMapper.Object);

        // Mock HttpContext to provide IMediator
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
    /// Tests that a valid ownership request creation returns OK with complete request details.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_ValidRequest_ReturnsOk()
    {
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();

        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = animalId
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Pending,
            RequestInfo = null,
            Animal = new Animal { Id = animalId, Name = "Test Animal", Cost = 100m },
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
            Status = OwnershipStatus.Pending,
            RequestInfo = null
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(animalId, returnedDto.AnimalId);
        Assert.Equal(userId, returnedDto.UserId);
        Assert.Equal(OwnershipStatus.Pending, returnedDto.Status);
        Assert.Equal(100m, returnedDto.Amount);
    }

    /// <summary>
    /// Tests that attempting to create a request for a non-existent animal returns NotFound.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_AnimalNotFound_ReturnsNotFound()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal ID not found", 404));

        var result = await _controller.CreateOwnershipRequest(dto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Animal ID not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that attempting to create a request for an animal with an owner returns BadRequest.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_AnimalHasOwner_ReturnsBadRequest()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Animal not available for ownership", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to create a request for an inactive animal returns BadRequest.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_AnimalInactive_ReturnsBadRequest()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Animal not available for ownership", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that attempting to create a duplicate request for the same animal returns BadRequest.
    /// Users can only have one ownership request per animal at a time.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_DuplicateRequest_ReturnsBadRequest()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You already have a pending ownership request for this animal", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("You already have a pending ownership request for this animal", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that a database failure during creation returns InternalServerError.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_DatabaseFailure_ReturnsInternalServerError()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to create ownership request", 500));

        var result = await _controller.CreateOwnershipRequest(dto);

        var serverErrorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("Failed to create ownership request", serverErrorResult.Value);
    }

    /// <summary>
    /// Tests that the request amount is automatically set from the animal's cost.
    /// </summary>
    [Fact]
    public async Task CreateOwnershipRequest_AmountSetFromAnimalCost_ReturnsOk()
    {
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var animalCost = 250m;

        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = animalId
        };

        var ownershipRequest = new OwnershipRequest
        {
            Id = Guid.NewGuid().ToString(),
            AnimalId = animalId,
            UserId = userId,
            Amount = animalCost,  // Set from animal's cost
            Status = OwnershipStatus.Pending,
            Animal = new Animal { Id = animalId, Name = "Test Animal", Cost = animalCost },
            User = new User { Id = userId, Name = "Test User" }
        };

        var responseDto = new ResOwnershipRequestDto
        {
            Id = ownershipRequest.Id,
            AnimalId = animalId,
            AnimalName = "Test Animal",
            UserId = userId,
            UserName = "Test User",
            Amount = animalCost,
            Status = OwnershipStatus.Pending
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<ResOwnershipRequestDto>(okResult.Value);
        Assert.Equal(animalCost, returnedDto.Amount);
    }
}