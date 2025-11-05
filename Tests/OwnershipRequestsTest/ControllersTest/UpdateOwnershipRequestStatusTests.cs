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

namespace Tests.OwnershipRequestsTest.ControllersTest;

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
public class UpdateOwnershipRequestStatusControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public UpdateOwnershipRequestStatusControllerTests()
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

    private static OwnershipRequest CreateAnalysingOwnershipRequest(string? requestInfo = null)
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
            Status = OwnershipStatus.Analysing,
            RequestInfo = requestInfo,
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
    public async Task UpdateStatus_ShouldReturnOkResult_WhenRequestIsValidWithNotes()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Reviewing applicant's credentials"
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest("Reviewing applicant's credentials");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnAnalysingStatus_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Reviewing applicant's credentials"
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest("Reviewing applicant's credentials");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(OwnershipStatus.Analysing, returnedDto!.Status);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnRequestInfo_WhenNotesAreProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var requestInfo = "Reviewing applicant's credentials";
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = requestInfo
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest(requestInfo);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Equal(requestInfo, returnedDto!.RequestInfo);
    }

    [Fact]
    public async Task UpdateStatus_ShouldSetUpdatedAtTimestamp_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Test notes"
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest("Test notes");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.NotNull(returnedDto!.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnOkResult_WhenTransitioningFromRejectedToAnalysing()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "User provided additional references, re-analyzing"
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest("User provided additional references, re-analyzing");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnOkResult_WhenNoRequestInfoIsProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        var ownershipRequest = CreateAnalysingOwnershipRequest(null);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNullRequestInfo_WhenNoNotesAreProvided()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        var ownershipRequest = CreateAnalysingOwnershipRequest(null);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        var result = await _controller.UpdateStatus(requestId, dto);

        var okResult = result.Result as OkObjectResult;
        var returnedDto = okResult!.Value as ResOwnershipRequestDto;
        Assert.Null(returnedDto!.RequestInfo);
    }

    [Fact]
    public async Task UpdateStatus_ShouldCallMediatorWithCorrectCommand_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var requestInfo = "Test notes";
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = requestInfo
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest(requestInfo);
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.UpdateStatus(requestId, dto);

        _mockMediator.Verify(m => m.Send(
            It.Is<UpdateOwnershipRequestStatus.Command>(c =>
                c.OwnershipRequestId == requestId &&
                c.RequestInfo == requestInfo),
            default), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ShouldCallMapperWithOwnershipRequest_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto
        {
            RequestInfo = "Test notes"
        };

        var ownershipRequest = CreateAnalysingOwnershipRequest("Test notes");
        var responseDto = CreateResponseDto(ownershipRequest);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Success(ownershipRequest, 200));

        _mockMapper
            .Setup(m => m.Map<ResOwnershipRequestDto>(It.IsAny<OwnershipRequest>()))
            .Returns(responseDto);

        await _controller.UpdateStatus(requestId, dto);

        _mockMapper.Verify(m => m.Map<ResOwnershipRequestDto>(
            It.IsAny<OwnershipRequest>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Ownership request not found", 404));

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenShelterDoesNotExist()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure("Shelter not found", 404));

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnBadRequest_WhenRequestIsAlreadyApprovedOrAnalysing()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Only pending or rejected requests can be moved to analysis", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnBadRequest_WhenAnimalAlreadyHasOwner()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Cannot analyze request, animal already has an owner", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnBadRequest_WhenAnimalIsInactive()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Cannot analyze request, animal is inactive", 400));

        var result = await _controller.UpdateStatus(requestId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnForbidden_WhenAnimalIsFromDifferentShelter()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "You can only update requests for animals in your shelter", 403));

        var result = await _controller.UpdateStatus(requestId, dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnForbidden_WhenUserIsNotShelterAdministrator()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Non Authorized Request: Only shelter administrators can update ownership requests", 403));

        var result = await _controller.UpdateStatus(requestId, dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        var requestId = Guid.NewGuid().ToString();
        var dto = new ReqUpdateOwnershipStatusDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<UpdateOwnershipRequestStatus.Command>(), default))
            .ReturnsAsync(Result<OwnershipRequest>.Failure(
                "Failed to update ownership request status", 500));

        var result = await _controller.UpdateStatus(requestId, dto);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}