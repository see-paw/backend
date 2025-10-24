using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Base controller that provides common functionality for all API controllers.
/// </summary>
/// <remarks>
/// Implements a shared <see cref="IMediator"/> instance and a standardized method
/// for handling <see cref="Result{T}"/> objects returned from the application layer.  
/// Ensures consistent API responses across all controllers.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()
                                                  ?? throw new InvalidOperationException("IMediator service is unavailable");

    /// <summary>
    /// Handles a <see cref="Result{T}"/> object and converts it into a corresponding <see cref="ActionResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result object returned from the application layer.</param>
    /// <param name="actionName">Optional name of the action for <c>CreatedAtAction</c> responses.</param>
    /// <param name="routeValues">Optional route values for <c>CreatedAtAction</c> responses.</param>
    /// <returns>
    /// An <see cref="ActionResult"/> representing the appropriate HTTP response,
    /// including status codes and response content.
    /// </returns>
    /// <remarks>
    /// This method standardizes how API responses are returned across controllers,
    /// ensuring consistent status codes and messages for success and error results.
    /// </remarks>
    protected ActionResult HandleResult<T>(Result<T> result,
        string? actionName = null,
        object? routeValues = null)
    {
        if (result == null)
        {
            return StatusCode(500, "Unexpected null result.");
        }

        if (!result.IsSuccess)
        {
            return result.Code switch
            {
                400 => BadRequest(result.Error),
                401 => Unauthorized(result.Error),
                403 => StatusCode(403, result.Error),
                404 => NotFound(result.Error),
                409 => Conflict(result.Error),
                500 => StatusCode(500, result.Error ?? "Internal Server Error"),
                _ => StatusCode(result.Code, result.Error ?? "Unexpected error")
            };
        }


       
        return result.Code switch
        {
            200 => Ok(result.Value),
            201 => actionName != null
                ? CreatedAtAction(actionName, routeValues, result.Value)
                : StatusCode(201, result.Value),
            204 => NoContent(),
            _ => StatusCode(result.Code, result.Value)
        };
    }

}

