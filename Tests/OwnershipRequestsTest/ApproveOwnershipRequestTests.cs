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
using Xunit;

namespace Tests.OwnershipRequests;

/// <summary>
/// Unit tests for ApproveRequest endpoint in OwnershipRequestsController.
/// 
/// These tests validate the approval workflow for ownership requests, ensuring that:
/// - Only requests in 'Analysing' status can be approved
/// - Animals must be available (not inactive or already owned)
/// - Proper authorization is enforced (shelter administrators only)
/// - All business rules and validations are correctly applied
/// </summary>
public class ApproveOwnershipRequestControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public ApproveOwnershipRequestControllerTests()
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

    private static OwnershipRequest CreateApprovedOwnershipRequest()
    {
        var requestId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();
        var approvedAt = DateTime.UtcNow;

        return new OwnershipRequest
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
                OwnershipStartDate = approvedAt,
                Colour = "Brown",
                Cost = 100,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                BirthDate = new DateOnly(2020, 1, 1),
                Sterilized = true,
                BreedId = Guid.NewGuid().ToString(),
                ShelterId = Guid.NewGuid().ToString()
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
            ApprovedAt = request.ApprovedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnOkResult_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var ownershipRequest = CreateApprovedOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnApprovedStatus_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var ownershipRequest = CreateApprovedOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.ApproveRequest(requestId);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(OwnershipStatus.Approved, returnedDto!.Status);
    }

    [Fact]
    public async Task ApproveRequest_ShouldSetApprovedAtTimestamp_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var ownershipRequest = CreateApprovedOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.ApproveRequest(requestId);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.NotNull(returnedDto!.ApprovedAt);
    }

    [Fact]
    public async Task ApproveRequest_ShouldCallMediatorWithCorrectCommand_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var ownershipRequest = CreateApprovedOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.ApproveRequest(requestId);

        _mockMediator.Verify(m => m.Send(
            It.Is<ApproveOwnershipRequest.Command>(c => c.OwnershipRequestId == requestId),
            default), Times.Once);
    }

    [Fact]
    public async Task ApproveRequest_ShouldCallMapperWithOwnershipRequest_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var ownershipRequest = CreateApprovedOwnershipRequest();
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.ApproveRequest(requestId);

        _mockMapper.Verify(m => m.Map<ResOwnershipRequestDto>(
            It.IsAny<OwnershipRequest>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnBadRequest_WhenAnimalAlreadyHasOwner()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal already has an owner", 400));

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnBadRequest_WhenAnimalIsInactive()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Animal is inactive", 400));

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnBadRequest_WhenStatusIsNotAnalysing()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only requests in 'Analysing' status can be approved", 400));

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnBadRequest_WhenAnotherRequestIsAlreadyApproved()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Animal already has an approved ownership request", 400));

        var result = await _controller.ApproveRequest(requestId);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnForbidden_WhenAnimalIsFromDifferentShelter()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You can only approve requests for animals in your shelter", 403));

        var result = await _controller.ApproveRequest(requestId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnForbidden_WhenUserIsNotShelterAdministrator()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only shelter administrators can approve ownership requests", 403));

        var result = await _controller.ApproveRequest(requestId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task ApproveRequest_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        var requestId = Guid.NewGuid().ToString();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<ApproveOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to approve ownership request", 500));

        var result = await _controller.ApproveRequest(requestId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}