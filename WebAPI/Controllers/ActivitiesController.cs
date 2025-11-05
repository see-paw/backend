using Application.Activities.Commands;
using Application.Activities.Queries;
using Application.Core;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.Activities;

namespace WebAPI.Controllers;

/// <summary>
/// API controller responsible for handling activity operations.
/// Provides endpoints for creating and managing ownership activities with animals.
/// </summary>
public class ActivitiesController(IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Gets all ownership activities for the authenticated user with pagination and optional status filter.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="status">Optional status filter (Active, Completed, Canceled, All). If not provided, returns all activities.</param>
    /// <returns>Paginated list of ownership activities.</returns>
    [Authorize(Roles = "User")]
    [HttpGet("ownership")]
    public async Task<ActionResult> GetOwnershipActivities(
        [FromQuery] int pageNumber = 1,
        [FromQuery] string? status = null)
    {
        var result = await Mediator.Send(new GetOwnershipActivitiesByUser.Query
        {
            PageNumber = pageNumber,
            Status = status
        });

        if (!result.IsSuccess || result.Value == null)
        {
            return HandleResult(result);
        }

        var dtoList = mapper.Map<List<ResActivityDto>>(result.Value);

        // Create a new paginated list with the DTOs
        var dtoPagedList = new PagedList<ResActivityDto>(
            dtoList,
            result.Value.TotalCount,
            result.Value.CurrentPage,
            result.Value.PageSize
        );

        // Return the successful paginated result
        return HandleResult(Result<PagedList<ResActivityDto>>.Success(dtoPagedList, 200));
    }

    /// <summary>
    /// Creates a new ownership activity for an animal.
    /// </summary>
    /// <param name="dto">The request DTO containing animal ID, start date, and end date.</param>
    /// <returns>The created activity object.</returns>
    /// <remarks>
    /// This endpoint allows animal owners to schedule visits/interactions with their animals
    /// at the shelter. The activity must be scheduled at least 24 hours in advance and must
    /// fall within the shelter's operating hours.
    /// </remarks>
    [Authorize(Roles = "User")]
    [HttpPost("ownership")]
    public async Task<ActionResult<ResActivityDto>> CreateOwnershipActivity([FromBody] ReqCreateActivityDto dto)
    {
        var command = new CreateOwnershipActivity.Command
        {
            AnimalId = dto.AnimalId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResActivityDto>(result.Value);

        return HandleResult(Result<ResActivityDto>.Success(responseDto, 201));
    }

    /// <summary>
    /// Cancels an active ownership activity.
    /// </summary>
    /// <param name="id">The unique identifier of the activity to cancel.</param>
    /// <returns>The cancelled activity object.</returns>
    /// <remarks>
    /// This endpoint allows animal owners to cancel their scheduled visits/interactions.
    /// Only activities with Active status can be cancelled.
    /// </remarks>
    [Authorize(Roles = "User")]
    [HttpPatch("ownership/{id}/cancel")]
    public async Task<ActionResult<ResActivityDto>> CancelOwnershipActivity(string id)
    {
        var command = new CancelOwnershipActivity.Command
        {
            ActivityId = id
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResActivityDto>(result.Value);

        return HandleResult(Result<ResActivityDto>.Success(responseDto, 200));
    }
    
    /// <summary>
    /// Schedules a visit slot for a fostered animal.
    /// </summary>
    /// <param name="dto">The request containing animal ID and visit time details.</param>
    /// <returns>
    /// A response containing the created activity and slot details, along with animal and shelter information.
    /// </returns>
    /// <remarks>
    /// This endpoint allows a foster user to schedule a visit with an animal they are fostering.
    /// The visit must be scheduled at least 24 hours in advance, last between 1-3 hours,
    /// and occur within the shelter's operating hours without conflicting with existing activities.
    /// </remarks>
    /// <response code="201">Visit successfully scheduled</response>
    /// <response code="400">Invalid request data or business rule violation</response>
    /// <response code="404">Animal not found or user is not fostering the animal</response>
    /// <response code="409">Time slot conflict or shelter unavailable</response>
    [HttpPost("foster-activity")]
    [ProducesResponseType(typeof(ResActivityFosteringDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResActivityFosteringDto>> ScheduleVisit(
        [FromBody] ReqCreateActivityFosteringDto dto)
    {
        var command = new CreateFosteringActivity.Command
        {
            AnimalId = dto.AnimalId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result);

        var responseDto = mapper.Map<ResActivityFosteringDto>(result.Value);

        return HandleResult(Result<ResActivityFosteringDto>.Success(responseDto, 201));
    }
    
}