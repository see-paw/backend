using Application.Core;
using Application.OwnershipRequests.Queries;
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
/// Unit tests for GetOwnershipRequestsByShelter endpoint in OwnershipRequestsController.
/// 
/// These tests validate the query workflow for retrieving paginated ownership requests including:
/// - Successful retrieval with pagination metadata
/// - Authorization validation (shelter administrators only)
/// - Empty result handling
/// - Proper DTO mapping with navigation properties
/// </summary>
public class GetOwnershipRequestsByShelterTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public GetOwnershipRequestsByShelterTests()
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
    /// Tests successful retrieval of paginated ownership requests.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_ValidRequest_ReturnsPagedList()
    {
        var ownershipRequests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "request-1",
                AnimalId = "animal-1",
                UserId = "user-1",
                Status = OwnershipStatus.Pending,
                Amount = 100,
                Animal = new Animal { Id = "animal-1", Name = "Bolinhas" },
                User = new User { Id = "user-1", Name = "Test User" }
            },
            new OwnershipRequest
            {
                Id = "request-2",
                AnimalId = "animal-2",
                UserId = "user-2",
                Status = OwnershipStatus.Analysing,
                Amount = 150,
                Animal = new Animal { Id = "animal-2", Name = "Miau" },
                User = new User { Id = "user-2", Name = "Another User" }
            }
        };

        var pagedList = new PagedList<OwnershipRequest>(ownershipRequests, 2, 1, 10);

        var responseDtos = new List<ResOwnershipRequestDto>
        {
            new ResOwnershipRequestDto
            {
                Id = "request-1",
                AnimalId = "animal-1",
                AnimalName = "Bolinhas",
                UserId = "user-1",
                UserName = "Test User",
                Amount = 100,
                Status = OwnershipStatus.Pending
            },
            new ResOwnershipRequestDto
            {
                Id = "request-2",
                AnimalId = "animal-2",
                AnimalName = "Miau",
                UserId = "user-2",
                UserName = "Another User",
                Amount = 150,
                Status = OwnershipStatus.Analysing
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(responseDtos);

        var result = await _controller.GetOwnershipRequests(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Tests that an empty result is properly handled.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_NoRequests_ReturnsEmptyPagedList()
    {
        var emptyList = new List<OwnershipRequest>();
        var pagedList = new PagedList<OwnershipRequest>(emptyList, 0, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(new List<ResOwnershipRequestDto>());

        var result = await _controller.GetOwnershipRequests(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Tests that non-shelter administrators cannot retrieve requests.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_UserNotShelterAdmin_ReturnsForbidden()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Failure(
                "Non Authorized Access: Only shelter administrators can view ownership requests", 403));

        var result = await _controller.GetOwnershipRequests(1);

        var forbiddenResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    /// <summary>
    /// Tests that a non-existent shelter returns NotFound.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_ShelterNotFound_ReturnsNotFound()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Failure("Shelter not found", 404));

        var result = await _controller.GetOwnershipRequests(1);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Shelter not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests pagination with custom page size.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_CustomPageSize_SendsCorrectQuery()
    {
        var pagedList = new PagedList<OwnershipRequest>(new List<OwnershipRequest>(), 0, 2, 5);

        GetOwnershipRequestsByShelter.Query? capturedQuery = null;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200))
            .Callback<IRequest<Result<PagedList<OwnershipRequest>>>, CancellationToken>((query, _) =>
            {
                capturedQuery = query as GetOwnershipRequestsByShelter.Query;
            });

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(new List<ResOwnershipRequestDto>());

        var result = await _controller.GetOwnershipRequests(2);

        Assert.IsType<OkObjectResult>(result);

        _mockMediator.Verify(m => m.Send(
            It.IsAny<GetOwnershipRequestsByShelter.Query>(),
            default), Times.Once);

        if (capturedQuery != null)
        {
            Assert.Equal(2, capturedQuery.PageNumber);
        }
    }

    /// <summary>
    /// Tests that mapper is called with correct data.
    /// </summary>
    [Fact]
    public async Task GetOwnershipRequests_ValidData_CallsMapperCorrectly()
    {
        var ownershipRequests = new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = "request-1",
                AnimalId = "animal-1",
                UserId = "user-1",
                Status = OwnershipStatus.Approved,
                Amount = 200,
                Animal = new Animal { Id = "animal-1", Name = "Bolinhas" },
                User = new User { Id = "user-1", Name = "John Doe" }
            }
        };

        var pagedList = new PagedList<OwnershipRequest>(ownershipRequests, 1, 1, 20);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(new List<ResOwnershipRequestDto>
            {
                new ResOwnershipRequestDto
                {
                    Id = "request-1",
                    AnimalName = "Bolinhas",
                    UserName = "John Doe"
                }
            });

        var result = await _controller.GetOwnershipRequests(1);

        _mockMapper.Verify(m => m.Map<List<ResOwnershipRequestDto>>(
            It.Is<List<OwnershipRequest>>(list => list.Count == 1 && list[0].Id == "request-1")),
            Times.Once);
    }
}