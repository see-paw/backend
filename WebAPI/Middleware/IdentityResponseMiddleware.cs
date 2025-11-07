using System.Text.Json;

namespace WebAPI.Middleware;

/// <summary>
/// Middleware that intercepts HTTP 401 (Unauthorized) responses produced by ASP.NET Identity 
/// or authentication components and replaces them with standardized JSON responses.
/// 
/// This ensures consistent API behavior by returning clear and structured error messages 
/// for all authentication-related failures.
/// </summary>
public class IdentityResponseMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityResponseMiddleware"/> class.
    /// </summary>
    /// <param name="next">
    /// The next delegate/middleware in the HTTP request pipeline.
    /// </param>
    public IdentityResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Intercepts unauthorized responses (HTTP 401) and replaces the response body 
    /// with a custom JSON message based on the Identity error type.
    /// </summary>
    /// <param name="context">The current HTTP request and response context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Intercepta apenas respostas 401 (Unauthorized)
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            newBody.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(newBody).ReadToEndAsync();

            string customJson;

            if (responseBody.Contains("InvalidLoginAttempt", StringComparison.OrdinalIgnoreCase))
            {
                customJson = JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Invalid credentials. Please check your email or password.",
                    details = (string?)null
                });
            }
            else if (responseBody.Contains("LockedOut", StringComparison.OrdinalIgnoreCase))
            {
                customJson = JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Account temporarily blocked: maximum login attempts reached.",
                    details = "LockedOut"
                });
            }
            else if (responseBody.Contains("NotAllowed", StringComparison.OrdinalIgnoreCase))
            {
                customJson = JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Account not confirmed. Please verify your email.",
                    details = "NotAllowed"
                });
            }
            else
            {
                customJson = JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Authentication required. Please provide valid credentials.",
                    details = "Unauthorized"
                });
            }

            context.Response.ContentType = "application/json";
            context.Response.ContentLength = customJson.Length;
            context.Response.Body = originalBody;
            await context.Response.WriteAsync(customJson);
        }
        else
        {
            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
        }
    }
}
