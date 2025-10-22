using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    private IMapper? _mapper;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()
                                                  ?? throw new InvalidOperationException("IMediator service is unavailable");
    protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetService<IMapper>()
                                           ?? throw new InvalidOperationException("IMapper service is unavailable");
    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess && result.Code == 404)
        {
            return NotFound(result.Error);
        }

        if (result.IsSuccess && result.Value != null)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }
}
