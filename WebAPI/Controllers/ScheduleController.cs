using Application.Core;
using Application.Scheduling.Queries;

using AutoMapper;

using Domain.Common;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebAPI.DTOs.AnimalSchedule;

namespace WebAPI.Controllers;

/// <summary>
/// Provides endpoints for retrieving animal scheduling information.
/// </summary>
public class ScheduleController(IMediator mediator, IMapper mapper) : BaseApiController
{
    /// <summary>
    /// Retrieves the complete weekly schedule of a specific animal.
    /// </summary>
    /// <param name="animalId">The unique identifier of the animal whose schedule is being requested.</param>
    /// <param name="startDate">The starting date of the week to retrieve (typically a Monday).</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing an <see cref="AnimalWeeklyScheduleDto"/> if successful,  
    /// or an appropriate error response if the request fails validation or authorization.
    /// </returns>
    [Authorize(Roles = AppRoles.User)]
    [HttpGet("animals/{animalId}/schedule/weekly")]
    public async Task<ActionResult<AnimalWeeklyScheduleDto>> GetAnimalWeeklySchedule(
        [FromRoute] string animalId,
        [FromQuery] DateOnly startDate)
    {
        var result = await mediator.Send(new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animalId,
            StartDate = startDate
        });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var resDto = mapper.Map<AnimalWeeklyScheduleDto>(result.Value);

        return HandleResult(new Result<AnimalWeeklyScheduleDto>
        {
            Value = resDto,
            Code = result.Code,
            IsSuccess = result.IsSuccess,
        });
    }
}
