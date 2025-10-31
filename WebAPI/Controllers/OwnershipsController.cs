using Application.Core;
using Application.Interfaces;
using Application.Ownerships.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers;

[Authorize(Roles = "User")]
public class OwnershipsController(IMapper mapper) : BaseApiController
{
    [HttpGet("requests")]
    public async Task<ActionResult> GetUserOwnerships()
    {
        var result = await Mediator.Send(new GetOwnershipRequestsByUser.Query());
        
        if (!result.IsSuccess)
            return HandleResult(result);

        var dtoRequests = mapper.Map<List<ResUserOwnershipsDto>>(result.Value);
        return HandleResult(Result<List<ResUserOwnershipsDto>>.Success(dtoRequests, 200));

    }
    
    [HttpGet("owned-animals")]
    public async Task<IActionResult> GetOwnedAnimals()
    {
        var result = await Mediator.Send(new GetUserOwnedAnimals.Query());
        
        if (!result.IsSuccess)
            return HandleResult(result);

        var dtoOwned = mapper.Map<List<ResUserOwnershipsDto>>(result.Value);
        return HandleResult(Result<List<ResUserOwnershipsDto>>.Success(dtoOwned, 200));
    }
    
}