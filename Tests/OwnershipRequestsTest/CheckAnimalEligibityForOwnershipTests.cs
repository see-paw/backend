using Application.Core;
using Application.OwnershipRequests.Queries;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;

namespace Tests.OwnershipRequestsTest;

/// <summary>
/// Unit test suite for <see cref="OwnershipRequestsController"/> that verifies
/// the behavior of the <c>CheckEligibility</c> endpoint.
/// </summary>
/// <remarks>
/// These tests validate the interaction between the controller, MediatR, and AutoMapper.
/// The controller logic is tested in isolation using mocks for all external dependencies.
/// </remarks>
public class CheckAnimalEligibityForOwnershipTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OwnershipRequestsController _controller;

    /// <summary>
    /// Initializes the test class by configuring mock dependencies and setting up
    /// a fake <see cref="HttpContext"/> so that the controller can access MediatR via dependency injection.
    /// </summary>
    public CheckAnimalEligibityForOwnershipTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        // Inject the mocked mapper into the controller
        _controller = new OwnershipRequestsController(_mapperMock.Object);

        // Mock service provider to supply the IMediator dependency
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        // Create a fake HTTP context that includes the mocked service provider
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            }
        };
    }
    
    /// <summary>
    /// Ensures that when the queried animal does not exist,
    /// the endpoint returns <c>404 Not Found</c>.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_AnimalNotFound_Returns404()
    {
        // Arrange
        var animalId = "non-existent-id";
        var failureResult = Result<bool>.Failure("Animal ID not found", 404);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAnimalEligibilityForOwnership.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.CheckEligibility(animalId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Ensures that when the animal exists but is not eligible,
    /// the endpoint returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_AnimalNotEligible_Returns400()
    {
        // Arrange
        var animalId = "animal-with-owner";
        var failureResult = Result<bool>.Failure("Animal not eligible for ownership", 400);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAnimalEligibilityForOwnership.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _controller.CheckEligibility(animalId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Ensures that when the animal is eligible, the endpoint returns
    /// <c>200 OK</c> with a <c>true</c> boolean result.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_AnimalEligible_Returns200WithTrue()
    {
        // Arrange
        var animalId = "eligible-animal";
        var successResult = Result<bool>.Success(true, 200);
        
        _mediatorMock
            .Setup(m => m.Send(It.Is<CheckAnimalEligibilityForOwnership.Query>(q => q.AnimalId == animalId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        _mapperMock
            .Setup(m => m.Map<bool>(true))
            .Returns(true);

        // Act
        var result = await _controller.CheckEligibility(animalId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.True((bool)okResult.Value);
    }

    /// <summary>
    /// Verifies that the controller sends the correct query object to MediatR.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_CallsMediatorWithCorrectQuery()
    {
        // Arrange
        var animalId = "test-animal-id";
        var successResult = Result<bool>.Success(true, 200);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAnimalEligibilityForOwnership.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        _mapperMock
            .Setup(m => m.Map<bool>(It.IsAny<bool>()))
            .Returns(true);

        // Act
        await _controller.CheckEligibility(animalId);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<CheckAnimalEligibilityForOwnership.Query>(q => q.AnimalId == animalId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that the AutoMapper is invoked only when the query returns a successful result.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_CallsMapperWhenSuccessful()
    {
        // Arrange
        var animalId = "test-animal-id";
        var successResult = Result<bool>.Success(true, 200);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAnimalEligibilityForOwnership.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        _mapperMock
            .Setup(m => m.Map<bool>(true))
            .Returns(true);

        // Act
        await _controller.CheckEligibility(animalId);

        // Assert
        _mapperMock.Verify(m => m.Map<bool>(true), Times.Once);
    }

    /// <summary>
    /// Ensures that the AutoMapper is not called when the query fails.
    /// </summary>
    [Fact]
    public async Task CheckEligibility_DoesNotCallMapperWhenFailed()
    {
        // Arrange
        var animalId = "non-existent";
        var failureResult = Result<bool>.Failure("Animal ID not found", 404);
        
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAnimalEligibilityForOwnership.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        await _controller.CheckEligibility(animalId);

        // Assert
        _mapperMock.Verify(m => m.Map<bool>(It.IsAny<bool>()), Times.Never);
    }
}