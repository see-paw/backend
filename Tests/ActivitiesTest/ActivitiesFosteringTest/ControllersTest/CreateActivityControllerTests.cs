using WebAPI.Controllers;
using Application.Activities.Commands;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Domain;
using Domain.Enums;
using WebAPI.DTOs.Activities;

namespace Tests.ActivitiesTest.ActivitiesFosteringTest.ControllersTest;

/// <summary>
/// Unit test suite for the <see cref="ActivitiesController"/> fostering scheduling endpoint (<c>ScheduleActivityFostering</c>).
/// </summary>
/// <remarks>
/// These tests cover the main success and failure scenarios for creating fostering activity visits,
/// ensuring that:
/// <list type="bullet">
/// <item><description>The correct <see cref="CreateFosteringActivity.Command"/> is sent to MediatR.</description></item>
/// <item><description>HTTP status codes reflect validation outcomes (e.g., 201, 400, 404, 409, 422).</description></item>
/// <item><description>AutoMapper correctly maps domain results to response DTOs.</description></item>
/// <item><description>All business rules enforced by the handler are represented in controller responses.</description></item>
/// </list>
/// </remarks>
public class CreateActivityControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ActivitiesController _controller;

    /// <summary>
    /// Initializes the controller and mock dependencies for testing.
    /// </summary>
    public CreateActivityControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _controller = new ActivitiesController(_mapperMock.Object);

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


    /// <summary>
    /// Verifies that a valid fostering visit scheduling request
    /// returns a <c>201 Created</c> result with correctly mapped data.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        var commandResult = new CreateFosteringActivity.CreateFosteringActivityResult
        {
            Activity = new Activity
            {
                Id = "activity-001",
                AnimalId = dto.AnimalId,
                UserId = "user-001",
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = startTime,
                EndDate = endTime
            },
            ActivitySlot = new ActivitySlot
            {
                Id = "slot-001",
                ActivityId = "activity-001",
                StartDateTime = startTime,
                EndDateTime = endTime,
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            },
            Animal = new Animal
            {
                Id = dto.AnimalId,
                Name = "Rex",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = "shelter-001",
                BreedId = "breed-001",
                Images = new List<Image>
                {
                    new Image
                    {
                        Id = "image-001",
                        PublicId = "test/rex",
                        Url = "https://example.com/rex.jpg",
                        Description = "Rex image",
                        IsPrincipal = true,
                        AnimalId = dto.AnimalId
                    }
                }
            },
            Shelter = new Shelter
            {
                Id = "shelter-001",
                Name = "Test Shelter",
                Street = "Test Street 123",
                City = "Porto",
                PostalCode = "4000-001",
                Phone = "223456789",
                NIF = "123456789",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            }
        };

        var responseDto = new ResActivityFosteringDto
        {
            ActivitySlotId = "slot-001",
            ActivityId = "activity-001",
            StartDateTime = startTime,
            EndDateTime = endTime,
            Animal = new AnimalVisitInfoDto
            {
                Id = dto.AnimalId,
                Name = "Rex",
                PrincipalImageUrl = "https://example.com/rex.jpg"
            },
            Shelter = new ShelterVisitInfoDto
            {
                Name = "Test Shelter",
                Address = "Test Street 123, Porto, 4000-001",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            },
            Message = "Visit scheduled successfully"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>.Success(commandResult, 201));

        _mapperMock
            .Setup(m => m.Map<ResActivityFosteringDto>(commandResult))
            .Returns(responseDto);

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var okResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(201, okResult.StatusCode);
        var returnValue = Assert.IsType<ResActivityFosteringDto>(okResult.Value);
        Assert.Equal("slot-001", returnValue.ActivitySlotId);
        Assert.Equal("activity-001", returnValue.ActivityId);
        Assert.Equal("Rex", returnValue.Animal.Name);
        Assert.Equal("Test Shelter", returnValue.Shelter.Name);
    }

    /// <summary>
    /// Ensures that if the requested animal does not exist, the controller returns <c>404 Not Found</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithAnimalNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "00000000-0000-0000-0000-000000000000",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(12)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("Animal not found", 404));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Verifies that a user who is not fostering the specified animal
    /// receives a <c>404 Not Found</c> response.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithUserNotFostering_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(12)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("You are not currently fostering this animal", 404));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    /// <summary>
    /// Ensures that scheduling a visit for an inactive animal returns <c>400 Bad Request</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithInactiveAnimal_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "a7b8c9d0-e1f2-4a1b-4c5d-6e7f8a9b0c1d",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(10),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(12)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("Animal cannot be visited", 400));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    /// <summary>
    /// Verifies that visits starting before the shelter's opening time return <c>422 Unprocessable Entity</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithStartBeforeOpeningTime_ReturnsUnprocessableEntity()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(8),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(10)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("Visit cannot start before shelter opening time (09:00:00)", 422));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var unprocessableResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(422, unprocessableResult.StatusCode);
    }

    /// <summary>
    /// Verifies that visits ending after the shelter's closing time return <c>422 Unprocessable Entity</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithEndAfterClosingTime_ReturnsUnprocessableEntity()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(16),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(19)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("Visit cannot end after shelter closing time (18:00:00)", 422));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var unprocessableResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(422, unprocessableResult.StatusCode);
    }

    /// <summary>
    /// Ensures that when a shelter is unavailable during the requested period, a <c>409 Conflict</c> is returned.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithShelterUnavailable_ReturnsConflict()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(14).AddMinutes(30),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(15).AddMinutes(30)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("Shelter is unavailable during the requested time", 409));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    /// <summary>
    /// Ensures that overlapping activity slots for the same animal result in a <c>409 Conflict</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithActivitySlotOverlap_ReturnsConflict()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "c9d0e1f2-a3b4-4c3d-6e7f-8a9b0c1d2e3f",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(10).AddMinutes(30),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(11).AddMinutes(30)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("The animal has another visit scheduled during this time", 409));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    /// <summary>
    /// Ensures that overlapping active activities for the same animal result in a <c>409 Conflict</c>.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_WithActivityOverlap_ReturnsConflict()
    {
        // Arrange
        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "d0e1f2a3-b4c5-4d4e-7f8a-9b0c1d2e3f4a",
            StartDateTime = DateTime.UtcNow.AddDays(2).AddHours(11),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(13)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>
                .Failure("The animal has another activity scheduled during this time", 409));

        // Act
        var result = await _controller.ScheduleActivityFostering(dto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ResActivityFosteringDto>>(result);
        var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    /// <summary>
    /// Verifies that the controller sends the correct command to MediatR
    /// containing the same data as the incoming request.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_SendsCorrectCommandToMediator()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        var commandResult = new CreateFosteringActivity.CreateFosteringActivityResult
        {
            Activity = new Activity
            {
                Id = "activity-001",
                AnimalId = dto.AnimalId,
                UserId = "user-001",
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = startTime,
                EndDate = endTime
            },
            ActivitySlot = new ActivitySlot
            {
                Id = "slot-001",
                ActivityId = "activity-001",
                StartDateTime = startTime,
                EndDateTime = endTime,
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            },
            Animal = new Animal
            {
                Id = dto.AnimalId,
                Name = "Rex",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = "shelter-001",
                BreedId = "breed-001",
                Images = new List<Image>()
            },
            Shelter = new Shelter
            {
                Id = "shelter-001",
                Name = "Test Shelter",
                Street = "Test Street 123",
                City = "Porto",
                PostalCode = "4000-001",
                Phone = "223456789",
                NIF = "123456789",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<CreateFosteringActivity.Command>(c =>
                c.AnimalId == dto.AnimalId &&
                c.StartDateTime == dto.StartDateTime &&
                c.EndDateTime == dto.EndDateTime), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>.Success(commandResult, 201));

        _mapperMock
            .Setup(m => m.Map<ResActivityFosteringDto>(
                It.IsAny<CreateFosteringActivity.CreateFosteringActivityResult>()))
            .Returns(new ResActivityFosteringDto
            {
                ActivitySlotId = "slot-001",
                ActivityId = "activity-001",
                StartDateTime = startTime,
                EndDateTime = endTime,
                Animal = new AnimalVisitInfoDto(),
                Shelter = new ShelterVisitInfoDto(),
                Message = "Visit scheduled successfully"
            });

        // Act
        await _controller.ScheduleActivityFostering(dto);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<CreateFosteringActivity.Command>(c =>
            c.AnimalId == dto.AnimalId &&
            c.StartDateTime == dto.StartDateTime &&
            c.EndDateTime == dto.EndDateTime), default), Times.Once);
    }

    /// <summary>
    /// Ensures that AutoMapper is called exactly once with the correct command result.
    /// </summary>
    [Fact]
    public async Task ScheduleVisit_CallsMapperWithCorrectData()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddDays(2).AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).AddHours(12);

        var dto = new ReqCreateActivityFosteringDto
        {
            AnimalId = "e5f6a7b8-c9d0-4e9f-2a3b-4c5d6e7f8a9b",
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        var commandResult = new CreateFosteringActivity.CreateFosteringActivityResult
        {
            Activity = new Activity
            {
                Id = "activity-001",
                AnimalId = dto.AnimalId,
                UserId = "user-001",
                Type = ActivityType.Fostering,
                Status = ActivityStatus.Active,
                StartDate = startTime,
                EndDate = endTime
            },
            ActivitySlot = new ActivitySlot
            {
                Id = "slot-001",
                ActivityId = "activity-001",
                StartDateTime = startTime,
                EndDateTime = endTime,
                Status = SlotStatus.Reserved,
                Type = SlotType.Activity
            },
            Animal = new Animal
            {
                Id = dto.AnimalId,
                Name = "Rex",
                AnimalState = AnimalState.PartiallyFostered,
                Species = Species.Dog,
                Size = SizeType.Medium,
                Sex = SexType.Male,
                Colour = "Brown",
                BirthDate = new DateOnly(2020, 3, 15),
                Sterilized = true,
                Cost = 50.00m,
                ShelterId = "shelter-001",
                BreedId = "breed-001",
                Images = new List<Image>()
            },
            Shelter = new Shelter
            {
                Id = "shelter-001",
                Name = "Test Shelter",
                Street = "Test Street 123",
                City = "Porto",
                PostalCode = "4000-001",
                Phone = "223456789",
                NIF = "123456789",
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0)
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFosteringActivity.Command>(), default))
            .ReturnsAsync(Result<CreateFosteringActivity.CreateFosteringActivityResult>.Success(commandResult, 201));

        _mapperMock
            .Setup(m => m.Map<ResActivityFosteringDto>(commandResult))
            .Returns(new ResActivityFosteringDto
            {
                ActivitySlotId = "slot-001",
                ActivityId = "activity-001",
                StartDateTime = startTime,
                EndDateTime = endTime,
                Animal = new AnimalVisitInfoDto(),
                Shelter = new ShelterVisitInfoDto(),
                Message = "Visit scheduled successfully"
            });

        // Act
        await _controller.ScheduleActivityFostering(dto);

        // Assert
        _mapperMock.Verify(m => m.Map<ResActivityFosteringDto>(commandResult), Times.Once);
    }
}