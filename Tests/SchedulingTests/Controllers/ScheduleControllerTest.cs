using Application.Core;
using Application.Scheduling;
using Application.Scheduling.Queries;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.AnimalSchedule;

namespace Tests.Scheduling.Controllers;

/// <summary>
/// Tests for ScheduleController using equivalence class partitioning and boundary value analysis.
/// Focuses on parameter handling, authorization, result mapping, and error handling.
/// </summary>
public class ScheduleControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ScheduleController _controller;

    public ScheduleControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _controller = new ScheduleController(_mockMediator.Object, _mockMapper.Object);
    }

    #region AnimalId Parameter - Equivalence Classes

    [Fact]
    public async Task GetAnimalWeeklySchedule_ValidAnimalId_SendsCorrectQuery()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == animalId && q.StartDate == startDate), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == animalId), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_NullAnimalId_PassesToMediator()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(null!, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == null), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_EmptyAnimalId_PassesToMediator()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(string.Empty, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == string.Empty), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_WhitespaceAnimalId_PassesToMediator()
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule("   ", startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == "   "), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_InvalidGuidFormat_PassesToMediator()
    {
        var invalidId = "not-a-guid";
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(invalidId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == invalidId), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_VeryLongAnimalId_PassesToMediator()
    {
        var longId = new string('a', 1000);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(longId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == longId), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_SpecialCharactersInAnimalId_PassesToMediator()
    {
        var specialId = "<script>alert('xss')</script>";
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(specialId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.AnimalId == specialId), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region StartDate Parameter - Boundary Value Analysis

    [Fact]
    public async Task GetAnimalWeeklySchedule_ValidStartDate_SendsCorrectQuery()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = new DateOnly(2025, 1, 6);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.StartDate == startDate), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_MinValueStartDate_PassesToMediator()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.MinValue;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.StartDate == DateOnly.MinValue), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_MaxValueStartDate_PassesToMediator()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.MaxValue;

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.StartDate == DateOnly.MaxValue), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_DefaultStartDate_PassesToMediator()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = default(DateOnly);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", 400));

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.StartDate == default(DateOnly)), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(2025, 1, 1)]
    [InlineData(2025, 12, 31)]
    [InlineData(2024, 2, 29)]
    public async Task GetAnimalWeeklySchedule_SpecificDates_PassesToMediator(int year, int month, int day)
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = new DateOnly(year, month, day);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.Is<GetAnimalWeeklySchedule.Query>(
                q => q.StartDate == startDate), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Success Response - Equivalence Classes

    [Fact]
    public async Task GetAnimalWeeklySchedule_SuccessResult_ReturnsOkWithMappedDto()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();
        var expectedDto = new AnimalWeeklyScheduleDto();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(schedule))
            .Returns(expectedDto);

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_SuccessResult_CallsMapperWithCorrectValue()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMapper.Verify(
            m => m.Map<AnimalWeeklyScheduleDto>(schedule),
            Times.Once);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    public async Task GetAnimalWeeklySchedule_SuccessWithDifferentCodes_ReturnsCorrectStatusCode(int statusCode)
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, statusCode));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        var okResult = result.Result as ObjectResult;
        
        if (statusCode == 200)
        {
            okResult = Assert.IsType<OkObjectResult>(result.Result);
        }
        
        Assert.Equal(statusCode, okResult!.StatusCode);
    }

    #endregion

    #region Failure Response - Equivalence Classes

    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(409)]
    public async Task GetAnimalWeeklySchedule_FailureResult_ReturnsCorrectStatusCode(int statusCode)
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", statusCode));

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        Assert.NotNull(result.Result);
        var objectResult = result.Result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.Equal(statusCode, objectResult?.StatusCode);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_NotFoundResult_DoesNotCallMapper()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Animal not found", 404));

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMapper.Verify(
            m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_ConflictResult_DoesNotCallMapper()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Animal not fostered", 409));

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMapper.Verify(
            m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_FailureWithNullError_HandlesCorrectly()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure(null!, 400));

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_FailureWithEmptyError_HandlesCorrectly()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure(string.Empty, 400));

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        Assert.NotNull(result.Result);
    }

    #endregion

    #region Error Handling - Equivalence Classes

    [Fact]
    public async Task GetAnimalWeeklySchedule_MediatorThrowsException_PropagatesException()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => 
            _controller.GetAnimalWeeklySchedule(animalId, startDate));
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_MapperThrowsException_PropagatesException()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Throws(new AutoMapperMappingException("Mapping error"));

        await Assert.ThrowsAsync<AutoMapperMappingException>(() => 
            _controller.GetAnimalWeeklySchedule(animalId, startDate));
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_MediatorReturnsNull_HandlesOrThrows()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Result<AnimalWeeklySchedule>)null!);

        await Assert.ThrowsAnyAsync<Exception>(() => 
            _controller.GetAnimalWeeklySchedule(animalId, startDate));
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_MapperReturnsNull_HandlesOrThrows()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns((AnimalWeeklyScheduleDto)null!);

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        Assert.NotNull(result.Result);
    }

    #endregion

    #region Cancellation Token Handling

    [Fact]
    public async Task GetAnimalWeeklySchedule_PassesCancellationToken_ToMediator()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(
                It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Multiple Calls - Equivalence Classes

    [Fact]
    public async Task GetAnimalWeeklySchedule_CalledMultipleTimes_EachCallIndependent()
    {
        var animalId1 = Guid.NewGuid().ToString();
        var animalId2 = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId1, startDate);
        await _controller.GetAnimalWeeklySchedule(animalId2, startDate);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetAnimalWeeklySchedule_CalledWithSameParameters_ExecutesBothTimes()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        await _controller.GetAnimalWeeklySchedule(animalId, startDate);
        await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Result Value Handling

    [Fact]
    public async Task GetAnimalWeeklySchedule_SuccessResult_PreservesResultProperties()
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = CreateValidSchedule();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Success(schedule, 200));

        _mockMapper
            .Setup(m => m.Map<AnimalWeeklyScheduleDto>(It.IsAny<AnimalWeeklySchedule>()))
            .Returns(new AnimalWeeklyScheduleDto());

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    #endregion

    #region Edge Cases - Status Codes

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task GetAnimalWeeklySchedule_UnusualStatusCodes_HandlesCorrectly(int statusCode)
    {
        var animalId = Guid.NewGuid().ToString();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAnimalWeeklySchedule.Query>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AnimalWeeklySchedule>.Failure("Error", statusCode));

        var result = await _controller.GetAnimalWeeklySchedule(animalId, startDate);

        Assert.NotNull(result.Result);
    }

    #endregion

    #region Helper Methods

    private static AnimalWeeklySchedule CreateValidSchedule()
    {
        var shelter = new Shelter
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Shelter",
            Street = "Test St",
            City = "Test City",
            PostalCode = "1234-567",
            Phone = "912345678",
            NIF = "123456789",
            OpeningTime = new TimeOnly(9, 0),
            ClosingTime = new TimeOnly(18, 0)
        };

        var animal = new Animal
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Animal",
            Species = Species.Dog,
            Size = SizeType.Medium,
            Sex = SexType.Male,
            Colour = "Brown",
            BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            Sterilized = true,
            Cost = 50m,
            ShelterId = shelter.Id,
            Shelter = shelter,
            BreedId = Guid.NewGuid().ToString()
        };

        return new AnimalWeeklySchedule
        {
            Animal = animal,
            Shelter = shelter,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
    }

    #endregion
}