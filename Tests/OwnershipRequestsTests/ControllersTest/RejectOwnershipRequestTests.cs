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
/// Unit tests for RejectRequest endpoint in OwnershipRequestsController.
/// 
/// These tests validate the rejection workflow for ownership requests, ensuring that:
/// - Only requests in 'Analysing' status can be rejected
/// - Optional rejection reasons are properly stored and visible to users
/// - Proper authorization is enforced (shelter administrators only)
/// - All business rules and validations are correctly applied
/// </summary>
public class RejectOwnershipRequestControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public RejectOwnershipRequestControllerTests()
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

    private static OwnershipRequest CreateRejectedOwnershipRequest(string? rejectionReason = null)
    {
        var requestId = Guid.NewGuid().ToString();
        var animalId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid().ToString();

        return new OwnershipRequest
        {
            Id = requestId,
            AnimalId = animalId,
            UserId = userId,
            Amount = 100m,
            Status = OwnershipStatus.Rejected,
            RequestInfo = rejectionReason,
            UpdatedAt = DateTime.UtcNow,
            Animal = new Animal
            {
                Id = animalId,
                Name = "Test Animal",
                Colour = "Brown",
                Cost = 100,
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
            RequestInfo = request.RequestInfo,
            UpdatedAt = request.UpdatedAt
        };
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnOkResult_WhenRequestIsValidWithReason()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "User does not meet shelter requirements"
        };

        var ownershipRequest = CreateRejectedOwnershipRequest("User does not meet shelter requirements");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnRejectedStatus_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        var ownershipRequest = CreateRejectedOwnershipRequest("Test reason");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(OwnershipStatus.Rejected, returnedDto!.Status);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnRejectionReason_WhenReasonIsProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var rejectionReason = "User does not meet shelter requirements";
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = rejectionReason
        };

        var ownershipRequest = CreateRejectedOwnershipRequest(rejectionReason);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(rejectionReason, returnedDto!.RequestInfo);
    }

    [Fact]
    public async Task RejectRequest_ShouldSetUpdatedAtTimestamp_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        var ownershipRequest = CreateRejectedOwnershipRequest("Test reason");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.NotNull(returnedDto!.UpdatedAt);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnOkResult_WhenNoReasonIsProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto();

        var ownershipRequest = CreateRejectedOwnershipRequest(null);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnNullRejectionReason_WhenNoReasonIsProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto();

        var ownershipRequest = CreateRejectedOwnershipRequest(null);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.RejectRequest(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Null(returnedDto!.RequestInfo);
    }

    [Fact]
    public async Task RejectRequest_ShouldCallMediatorWithCorrectCommand_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var rejectionReason = "Test reason";
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = rejectionReason
        };

        var ownershipRequest = CreateRejectedOwnershipRequest(rejectionReason);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.RejectRequest(requestId, dto);

        _mockMediator.Verify(m => m.Send(
            It.Is<RejectOwnershipRequest.Command>(c =>
                c.OwnershipRequestId == requestId &&
                c.RejectionReason == rejectionReason),
            default), Times.Once);
    }

    [Fact]
    public async Task RejectRequest_ShouldCallMapperWithOwnershipRequest_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqRejectOwnershipRequestDto
        {
            RejectionReason = "Test reason"
        };

        var ownershipRequest = CreateRejectedOwnershipRequest("Test reason");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<RejectOwnershipRequest.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.RejectRequest(requestId, dto);

        _mockMapper.Verify(m => m.Map<ResOwnershipRequestDto>(
            It.IsAny<OwnershipRequest>()), Times.Once);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
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

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnBadRequest_WhenStatusIsNotAnalysing()
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

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnForbidden_WhenAnimalIsFromDifferentShelter()
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

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnForbidden_WhenUserIsNotShelterAdministrator()
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

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task RejectRequest_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
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

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}