using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using API.Core;

namespace API.Middleware
{
    /// <summary>
    /// Middleware responsible for globally handling exceptions that occur during request processing.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts unhandled exceptions and formats a standardized JSON response.
    /// It provides specific handling for <see cref="ValidationException"/> (from FluentValidation)
    /// and general exception handling for other unexpected errors.
    /// </remarks>
    public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IHostEnvironment env) : IMiddleware
    {
        /// <summary>
        /// Invokes the next middleware in the HTTP pipeline and captures any exceptions thrown during execution.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
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
        /// Handles general unhandled exceptions and returns a structured JSON response to the client.
        /// </summary>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <param name="ex">The exception that was thrown.</param>
        private async Task HandleException(HttpContext context, Exception ex)
        {
            logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = env.IsDevelopment()
                ? new AppException(context.Response.StatusCode, ex.Message, ex.StackTrace)
                : new AppException(context.Response.StatusCode, ex.Message, null);

            JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Handles validation-related exceptions thrown by FluentValidation and returns a detailed validation error response.
        /// </summary>
        /// <param name="context">The HTTP context associated with the current request.</param>
        /// <param name="ex">The <see cref="ValidationException"/> containing validation errors.</param>
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
                Detail = "One or more validation errors have occurred."
            };

            await context.Response.WriteAsJsonAsync(validationProblemDetails);
        }
    }
}
