using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace WebAPI.Middleware;

/// <summary>
/// Custom implementation of <see cref="IAuthorizationMiddlewareResultHandler"/> 
/// that provides standardized JSON responses for authorization failures.
/// 
/// This middleware enhances the default ASP.NET Core authorization handling by 
/// intercepting forbidden and unauthorized results, returning structured JSON error messages 
/// instead of plain HTTP responses. This improves API consistency and client-side integration.
/// </summary>
public class CustomAuthMiddlewareHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    /// <summary>
    /// Handles authorization results by setting custom HTTP responses for unauthorized or forbidden requests.
    /// 
    /// When the user lacks permissions, it returns a <c>403 Forbidden</c> JSON response.
    /// When the user is unauthenticated, it returns a <c>401 Unauthorized</c> JSON response.
    /// Otherwise, it delegates the handling to the default authorization middleware handler.
    /// </summary>
    /// <param name="next">The next middleware delegate in the HTTP pipeline.</param>
    /// <param name="context">The current <see cref="HttpContext"/> of the request.</param>
    /// <param name="policy">The <see cref="AuthorizationPolicy"/> being evaluated.</param>
    /// <param name="authorizeResult">The result of the authorization process.</param>
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Access denied. Not enough permissions\"}");
            return;
        }

        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Not authenticated.\"}");
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
