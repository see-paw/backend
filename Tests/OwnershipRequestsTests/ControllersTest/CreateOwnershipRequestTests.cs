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
using WebAPI.DTOs.Ownership;

namespace Tests.OwnershipRequestsTests.ControllersTest;

/// <summary>
/// Unit tests for CreateOwnershipRequest endpoint in OwnershipRequestsController.
/// 
/// These tests validate the creation workflow for ownership requests, ensuring that:
/// - Users can successfully create adoption requests for available animals
/// - Proper validation is enforced (animal exists, available, no duplicates)
/// - User ID is extracted from JWT token (not from request body)
/// - All business rules are correctly applied
/// </summary>
public class CreateOwnershipRequestControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public CreateOwnershipRequestControllerTests()
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

    private static OwnershipRequest CreateOwnershipRequest(decimal animalCost = 100m)
    {
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();

        return new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = animalCost,
            Status = OwnershipStatus.Pending,
            RequestInfo = null,
            Animal = new Animal
            {
                Id = animalId,
                Name = "Test Animal",
                Cost = animalCost,
                Colour = "Brown",
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString(),
                AnimalState = AnimalState.Available
            },
            User = new User
            {
                Id = userId,
                Name = "Test User",
                Email = "test@example.com",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Street = "Test Street",
                City = "Test City",
                PostalCode = "1234-567"
            }
        };
    }

    private static ResOwnershipRequestDto CreateResponseDto(OwnershipRequest request)
    {
        return new ResOwnershipRequestDto
        {
            Id = request.Id,
            AnimalId = request.AnimalId,
            AnimalName = request.Animal.Name,
            UserId = request.UserId,
            UserName = request.User.Name,
            Amount = request.Amount,
            Status = request.Status,
            RequestInfo = request.RequestInfo
        };
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnOkResult_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnCorrectAnimalId_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(ownershipRequest.AnimalId, returnedDto!.AnimalId);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnCorrectUserId_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(ownershipRequest.UserId, returnedDto!.UserId);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnPendingStatus_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(OwnershipStatus.Pending, returnedDto!.Status);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnAmountFromAnimalCost_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var animalCost = 250m;
        var ownershipRequest = CreateOwnershipRequest(animalCost);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.CreateOwnershipRequest(dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(animalCost, returnedDto!.Amount);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCallMediatorWithCorrectCommand_WhenRequestIsValid()
    {
        var animalId = Guid.NewGuid().ToString();
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = animalId
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.CreateOwnershipRequest(dto);

        _mockMediator.Verify(m => m.Send(
            It.Is<CreateOwnershipRequest.Command>(c => c.AnimalID == animalId),
            default), Times.Once);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldCallMapperWithOwnershipRequest_WhenRequestIsValid()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        var ownershipRequest = CreateOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.CreateOwnershipRequest(dto);

        _mockMapper.Verify(m => m.Map<ResOwnershipRequestDto>(
            It.IsAny<OwnershipRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnNotFound_WhenAnimalDoesNotExist()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal ID not found", 404));

        var result = await _controller.CreateOwnershipRequest(dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenAnimalHasOwner()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenAnimalIsInactive()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal not available for ownership", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnBadRequest_WhenDuplicateRequestExists()
    {
        var dto = new ReqCreateOwnershipRequestDto
        {
            AnimalId = Guid.NewGuid().ToString()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "User already has a pending ownership request for this animal", 400));

        var result = await _controller.CreateOwnershipRequest(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOwnershipRequest_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
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

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}