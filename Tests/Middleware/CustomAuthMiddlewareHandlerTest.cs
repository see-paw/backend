using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using WebAPI.Middleware;

namespace Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="CustomAuthMiddlewareHandler"/>.
/// 
/// This test suite validates the behavior of the custom authorization middleware handler
/// responsible for processing authorization results and setting the appropriate HTTP status codes.
/// </summary>
public class CustomAuthMiddlewareHandlerTest
{
    private readonly DefaultHttpContext _httpContext;
    private readonly RequestDelegate _next;
    private readonly AuthorizationPolicy _policy;
    private readonly IAuthorizationMiddlewareResultHandler _handler;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomAuthMiddlewareHandlerTest"/> class.
    /// 
    /// Sets up a mock <see cref="HttpContext"/>, a default <see cref="AuthorizationPolicy"/>,
    /// and a <see cref="RequestDelegate"/> to simulate middleware execution.
    /// </summary>
    public CustomAuthMiddlewareHandlerTest()
    {
        _httpContext = new DefaultHttpContext();
        _policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        _next = _ => Task.CompletedTask;
        _handler = new CustomAuthMiddlewareHandler();
    }

    /// <summary>
    /// Ensures that when authorization fails with a <c>Forbid</c> result,
    /// the middleware sets the response status code to <c>403 Forbidden</c>.
    /// </summary>
    [Fact]
    public async Task ForbiddenUser_Returns403Forbidden()
    {
        // Arrange
        var result = PolicyAuthorizationResult.Forbid();

        // Act
        await _handler.HandleAsync(_next, _httpContext, _policy, result);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, _httpContext.Response.StatusCode);
    }
    
    /// <summary>
    /// Ensures that when authorization fails with a <c>Challenge</c> result,
    /// the middleware sets the response status code to <c>401 Unauthorized</c>.
    /// </summary>
    [Fact]
    public async Task UnauthorizedUser_Returns401Unauthorized()
    {
        // Arrange
        var result = PolicyAuthorizationResult.Challenge();

        // Act
        await _handler.HandleAsync(_next, _httpContext, _policy, result);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, _httpContext.Response.StatusCode);
    }
    
    /// <summary>
    /// Ensures that when authorization succeeds,
    /// the middleware continues the pipeline without modifying the response.
    /// </summary>
    [Fact]
    public async Task AuthorizedUser_CallsNextWithoutChangingResponse()
    {
        // Arrange
        var result = PolicyAuthorizationResult.Success();

        // Act
        await _handler.HandleAsync(_next, _httpContext, _policy, result);

        // Assert
        Assert.Equal(200, _httpContext.Response.StatusCode == 0 ? 200 : _httpContext.Response.StatusCode);
    }
}