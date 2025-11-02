using Application.Core;
using Application.Scheduling.Queries;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.AnimalSchedule;

namespace WebAPI.Controllers;

public class ScheduleController(IMediator mediator, IMapper mapper) : BaseApiController
{
    [Authorize(Roles = "User")]
    [HttpGet("animals/{animalId}/schedule/weekly")]
    public async Task<ActionResult<ResWeeklyAnimalScheduleDto>> GetAnimalWeeklySchedule(
        [FromRoute] string animalId, 
        [FromQuery] DateOnly startDate)
    {
        var result =  await mediator.Send(new GetAnimalWeeklySchedule.Query
        {
            AnimalId = animalId,
            StartDate = startDate
        });

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var resDto = mapper.Map<ResWeeklyAnimalScheduleDto>(result.Value);

        return HandleResult(new Result<ResWeeklyAnimalScheduleDto>
        {
            Value = resDto,
            Code = result.Code
        });
    }
}