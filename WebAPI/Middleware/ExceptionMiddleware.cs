using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebAPI.Core;

namespace WebAPI.Middleware;

/// <summary>
/// Middleware responsible for handling exceptions globally in the application.
/// </summary>
/// <remarks>
/// Captures and processes both validation and general exceptions, ensuring that
/// all errors are returned as structured JSON responses with appropriate HTTP status codes.
/// </remarks>
public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IHostEnvironment env) : IMiddleware
{
    /// <summary>
    /// Executes the middleware logic for handling exceptions during the HTTP request pipeline.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Invokes the next middleware and captures any exceptions thrown during execution,
    /// delegating them to the appropriate handler for validation or general errors.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    /// <summary>
    /// Handles unexpected exceptions and returns a standardized JSON error response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="ex">The exception that was thrown.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Logs the exception and returns a JSON response containing the error details.
    /// In development mode, the stack trace is included in the response for debugging purposes.
    /// </remarks>
    private async Task HandleException(HttpContext context, Exception ex)
    {
        logger.LogError(ex, ex.Message);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = env.IsDevelopment()
            ? new AppException
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                Details = ex.StackTrace
            }
            : new AppException
            {
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                Details = null
            };

        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Handles validation exceptions and returns a structured JSON response with validation errors.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="ex">The validation exception that was thrown.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Collects all validation errors and returns them as a <see cref="ValidationProblemDetails"/> object
    /// with a <c>400 Bad Request</c> status code.
    /// </remarks>
    private static async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        var validationErrors = new Dictionary<string, string[]>();

        if (ex.Errors is not null)
        {
            foreach (var error in ex.Errors)
            {
                if (validationErrors.TryGetValue(error.PropertyName, out var existingErrors))
                {
                    validationErrors[error.PropertyName] = existingErrors.Append(error.ErrorMessage).ToArray();
                }
                else
                {
                    validationErrors[error.PropertyName] = new[] { error.ErrorMessage };
                }
            }

        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var validationProblemDetails = new ValidationProblemDetails(validationErrors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "ValidationFailure",
            Title = "Validation error",
            Detail = "One or more validation errors has occured"
        };

        await context.Response.WriteAsJsonAsync(validationProblemDetails);
    }
}
