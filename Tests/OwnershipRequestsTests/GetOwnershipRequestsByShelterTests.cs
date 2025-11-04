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
using WebAPI.DTOs.Ownership;
using Xunit;

namespace Tests.OwnershipRequests;

/// <summary>
/// Unit tests for GetOwnershipRequestsByShelter endpoint in OwnershipRequestsController.
/// 
/// These tests validate the query workflow for retrieving paginated ownership requests including:
/// - Successful retrieval with pagination metadata
/// - Authorization validation (shelter administrators only)
/// - Empty result handling
/// - Proper DTO mapping with navigation properties
/// </summary>
public class GetOwnershipRequestsByShelterControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly OwnershipRequestsController _controller;

    public GetOwnershipRequestsByShelterControllerTests()
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

    private static List<OwnershipRequest> CreateOwnershipRequests()
    {
        return new List<OwnershipRequest>
        {
            new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = "animal-1",
                UserId = "user-1",
                Status = OwnershipStatus.Pending,
                Amount = 100,
                Animal = new Animal
                {
                    Id = "animal-1",
                    Name = "Bolinhas",
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
                    Id = "user-1",
                    Name = "Test User",
                    Email = "test@example.com",
                    BirthDate = DateTime.UtcNow.AddYears(-25),
                    Street = "Test Street",
                    City = "Test City",
                    PostalCode = "1234-567"
                }
            },
            new OwnershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = "animal-2",
                UserId = "user-2",
                Status = OwnershipStatus.Analysing,
                Amount = 150,
                Animal = new Animal
                {
                    Id = "animal-2",
                    Name = "Miau",
                    Colour = "White",
                    Cost = 150,
                    Species = Species.Cat,
                    Size = SizeType.Small,
                    Sex = SexType.Female,
                    BirthDate = new DateOnly(2021, 3, 15),
                    Sterilized = false,
                    BreedId = Guid.NewGuid().ToString(),
                    ShelterId = Guid.NewGuid().ToString(),
                    AnimalState = AnimalState.Available
                },
                User = new User
                {
                    Id = "user-2",
                    Name = "Another User",
                    Email = "another@example.com",
                    BirthDate = DateTime.UtcNow.AddYears(-30),
                    Street = "Another Street",
                    City = "Another City",
                    PostalCode = "5678-901"
                }
            }
        };
    }

    private static List<ResOwnershipRequestDto> CreateResponseDtos()
    {
        return new List<ResOwnershipRequestDto>
        {
            new ResOwnershipRequestDto
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = "animal-1",
                AnimalName = "Bolinhas",
                UserId = "user-1",
                UserName = "Test User",
                Amount = 100,
                Status = OwnershipStatus.Pending
            },
            new ResOwnershipRequestDto
            {
                Id = Guid.NewGuid().ToString(),
                AnimalId = "animal-2",
                AnimalName = "Miau",
                UserId = "user-2",
                UserName = "Another User",
                Amount = 150,
                Status = OwnershipStatus.Analysing
            }
        };
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldReturnOkResult_WhenRequestIsValid()
    {
        var ownershipRequests = CreateOwnershipRequests();
        var pagedList = new PagedList<OwnershipRequest>(ownershipRequests, 2, 1, 10);
        var responseDtos = CreateResponseDtos();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(responseDtos);

        var result = await _controller.GetOwnershipRequests(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldReturnOkResult_WhenNoRequestsExist()
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

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldCallMediatorWithCorrectQuery_WhenRequestIsValid()
    {
        var pageNumber = 2;
        var ownershipRequests = CreateOwnershipRequests();
        var pagedList = new PagedList<OwnershipRequest>(ownershipRequests, 2, pageNumber, 10);
        var responseDtos = CreateResponseDtos();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<List<OwnershipRequest>>()))
            .Returns(responseDtos);

        await _controller.GetOwnershipRequests(pageNumber);

        _mockMediator.Verify(m => m.Send(
            It.Is<GetOwnershipRequestsByShelter.Query>(q => q.PageNumber == pageNumber),
            default), Times.Once);
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldCallMapperWithOwnershipRequests_WhenRequestIsValid()
    {
        var ownershipRequests = CreateOwnershipRequests();
        var pagedList = new PagedList<OwnershipRequest>(ownershipRequests, ownershipRequests.Count, 1, 10);
        var responseDtos = CreateResponseDtos();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Success(pagedList, 200));

        _mockMapper
            .Setup(m => m.Map<List<ResOwnershipRequestDto>>(It.IsAny<IEnumerable<OwnershipRequest>>()))
            .Returns(responseDtos);

        await _controller.GetOwnershipRequests(1);

        _mockMapper.Verify(m => m.Map<List<ResOwnershipRequestDto>>(
            It.IsAny<IEnumerable<OwnershipRequest>>()), Times.Once);
    }


    [Fact]
    public async Task GetOwnershipRequests_ShouldReturnForbidden_WhenUserIsNotShelterAdministrator()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Failure(
                "Non Authorized Access: Only shelter administrators can view ownership requests", 403));

        var result = await _controller.GetOwnershipRequests(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldReturnNotFound_WhenShelterDoesNotExist()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Failure("Shelter not found", 404));

        var result = await _controller.GetOwnershipRequests(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetOwnershipRequests_ShouldReturnInternalServerError_WhenDatabaseFailureOccurs()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetOwnershipRequestsByShelter.Query>(), default))
            .ReturnsAsync(Result<PagedList<OwnershipRequest>>.Failure(
                "Failed to retrieve ownership requests", 500));

        var result = await _controller.GetOwnershipRequests(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}