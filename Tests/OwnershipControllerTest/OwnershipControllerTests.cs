using Application.Core;
using Application.Interfaces;
using Application.Ownerships.Queries;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs;

namespace Tests.OwnershipControllerTest;

public class OwnershipControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly OwnershipsController _controller;

    public OwnershipControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new OwnershipsController(_mapperMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            }
        };
    }

    #region GetUserOwnerships Tests

    [Fact]
    public async Task GetUserOwnerships_WhenSuccessful_ReturnsOkWithMappedData()
    {
        // Arrange
        var ownershipRequests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "req1",
                AnimalId = "animal1",
                UserId = "user1",
                Amount = 50,
                Status = OwnershipStatus.Pending,
                Animal = new Animal { Id = "animal1", Name = "Max" },
                User = new User { Id = "user1", Name = "John Doe" }
            }
        };

        var expectedDtos = new List<ResUserOwnershipsDto>
        {
            new ResUserOwnershipsDto
            {
                Id = "req1",
                AnimalId = "animal1",
                AnimalName = "Max",
                Amount = 50,
                OwnershipStatus = OwnershipStatus.Pending
            }
        };

        var mediatorResult = Result<List<OwnershipRequest>>.Success(ownershipRequests, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownershipRequests))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Single(returnedDtos);
        Assert.Equal("req1", returnedDtos[0].Id);
    }

    [Fact]
    public async Task GetUserOwnerships_WhenEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<OwnershipRequest>();
        var emptyDtoList = new List<ResUserOwnershipsDto>();
        
        var mediatorResult = Result<List<OwnershipRequest>>.Success(emptyList, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(emptyList))
            .Returns(emptyDtoList);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Empty(returnedDtos);
    }

    [Fact]
    public async Task GetUserOwnerships_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var mediatorResult = Result<List<OwnershipRequest>>.Failure("User not found", 404);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetUserOwnerships_WhenUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var mediatorResult = Result<List<OwnershipRequest>>.Failure("Unauthorized", 401);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetUserOwnerships_WhenInternalServerError_ReturnsInternalServerError()
    {
        // Arrange
        var mediatorResult = Result<List<OwnershipRequest>>.Failure("Internal server error", 500);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    [Fact]
    public async Task GetUserOwnerships_WithMultipleStatuses_ReturnsAllMappedCorrectly()
    {
        // Arrange
        var ownershipRequests = new List<OwnershipRequest>
        {
            new OwnershipRequest { Id = "req1", Status = OwnershipStatus.Pending, Animal = new Animal(), User = new User() },
            new OwnershipRequest { Id = "req2", Status = OwnershipStatus.Analysing, Animal = new Animal(), User = new User() },
            new OwnershipRequest { Id = "req3", Status = OwnershipStatus.Rejected, Animal = new Animal(), User = new User() }
        };

        var expectedDtos = new List<ResUserOwnershipsDto>
        {
            new ResUserOwnershipsDto { Id = "req1", OwnershipStatus = OwnershipStatus.Pending },
            new ResUserOwnershipsDto { Id = "req2", OwnershipStatus = OwnershipStatus.Analysing },
            new ResUserOwnershipsDto { Id = "req3", OwnershipStatus = OwnershipStatus.Rejected }
        };

        var mediatorResult = Result<List<OwnershipRequest>>.Success(ownershipRequests, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownershipRequests))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetUserOwnerships();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Equal(3, returnedDtos.Count);
    }

    #endregion

    #region GetOwnedAnimals Tests

    [Fact]
    public async Task GetOwnedAnimals_WhenSuccessful_ReturnsOkWithMappedData()
    {
        // Arrange
        var ownedAnimals = new List<Animal>
        {
            new Animal
            {
                Id = "animal1",
                Name = "Max",
                OwnerId = "user1",
                AnimalState = AnimalState.HasOwner,
                Cost = 100
            }
        };

        var expectedDtos = new List<ResUserOwnershipsDto>
        {
            new ResUserOwnershipsDto
            {
                Id = "animal1",
                AnimalId = "animal1",
                AnimalName = "Max",
                Amount = 100,
                OwnershipStatus = null,
                AnimalState = AnimalState.HasOwner
            }
        };

        var mediatorResult = Result<List<Animal>>.Success(ownedAnimals, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownedAnimals))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Single(returnedDtos);
        Assert.Equal("animal1", returnedDtos[0].Id);
        Assert.Null(returnedDtos[0].OwnershipStatus);
    }

    [Fact]
    public async Task GetOwnedAnimals_WhenEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyList = new List<Animal>();
        var emptyDtoList = new List<ResUserOwnershipsDto>();
        
        var mediatorResult = Result<List<Animal>>.Success(emptyList, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(emptyList))
            .Returns(emptyDtoList);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Empty(returnedDtos);
    }

    [Fact]
    public async Task GetOwnedAnimals_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var mediatorResult = Result<List<Animal>>.Failure("User not found", 404);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetOwnedAnimals_WhenUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var mediatorResult = Result<List<Animal>>.Failure("Unauthorized", 401);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task GetOwnedAnimals_WhenInternalServerError_ReturnsInternalServerError()
    {
        // Arrange
        var mediatorResult = Result<List<Animal>>.Failure("Internal server error", 500);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    [Fact]
    public async Task GetOwnedAnimals_WithMultipleAnimals_ReturnsAllMappedCorrectly()
    {
        // Arrange
        var ownedAnimals = new List<Animal>
        {
            new Animal { Id = "animal1", Name = "Max", AnimalState = AnimalState.HasOwner },
            new Animal { Id = "animal2", Name = "Bella", AnimalState = AnimalState.HasOwner },
            new Animal { Id = "animal3", Name = "Charlie", AnimalState = AnimalState.HasOwner }
        };

        var expectedDtos = new List<ResUserOwnershipsDto>
        {
            new ResUserOwnershipsDto { Id = "animal1", AnimalName = "Max", OwnershipStatus = null },
            new ResUserOwnershipsDto { Id = "animal2", AnimalName = "Bella", OwnershipStatus = null },
            new ResUserOwnershipsDto { Id = "animal3", AnimalName = "Charlie", OwnershipStatus = null }
        };

        var mediatorResult = Result<List<Animal>>.Success(ownedAnimals, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownedAnimals))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Equal(3, returnedDtos.Count);
        Assert.All(returnedDtos, dto => Assert.Null(dto.OwnershipStatus));
    }

    [Fact]
    public async Task GetOwnedAnimals_VerifiesStatusIsNull_ForOwnedAnimals()
    {
        // Arrange
        var ownedAnimals = new List<Animal>
        {
            new Animal { Id = "animal1", Name = "Max", OwnerId = "user1" }
        };

        var expectedDtos = new List<ResUserOwnershipsDto>
        {
            new ResUserOwnershipsDto 
            { 
                Id = "animal1", 
                AnimalName = "Max",
                OwnershipStatus = null // Critical: owned animals don't have ownership status
            }
        };

        var mediatorResult = Result<List<Animal>>.Success(ownedAnimals, 200);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownedAnimals))
            .Returns(expectedDtos);

        // Act
        var result = await _controller.GetOwnedAnimals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDtos = Assert.IsType<List<ResUserOwnershipsDto>>(okResult.Value);
        Assert.Null(returnedDtos[0].OwnershipStatus);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task BothEndpoints_CanBeCalledSequentially_WithoutInterference()
    {
        // Arrange
        var ownershipRequests = new List<OwnershipRequest> { new OwnershipRequest { Animal = new Animal(), User = new User() } };
        var ownedAnimals = new List<Animal> { new Animal() };
        var requestDtos = new List<ResUserOwnershipsDto> { new ResUserOwnershipsDto { OwnershipStatus = OwnershipStatus.Pending } };
        var animalDtos = new List<ResUserOwnershipsDto> { new ResUserOwnershipsDto { OwnershipStatus = null } };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByUser.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OwnershipRequest>>.Success(ownershipRequests, 200));

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetUserOwnedAnimals.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<Animal>>.Success(ownedAnimals, 200));

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownershipRequests))
            .Returns(requestDtos);

        _mapperMock
            .Setup(m => m.Map<List<ResUserOwnershipsDto>>(ownedAnimals))
            .Returns(animalDtos);

        // Act
        var requestsResult = await _controller.GetUserOwnerships();
        var animalsResult = await _controller.GetOwnedAnimals();

        // Assert
        Assert.IsType<OkObjectResult>(requestsResult);
        Assert.IsType<OkObjectResult>(animalsResult);
        
        var requestsData = ((OkObjectResult)requestsResult).Value as List<ResUserOwnershipsDto>;
        var animalsData = ((OkObjectResult)animalsResult).Value as List<ResUserOwnershipsDto>;
        
        Assert.NotNull(requestsData![0].OwnershipStatus);
        Assert.Null(animalsData![0].OwnershipStatus);
    }

    #endregion
}
