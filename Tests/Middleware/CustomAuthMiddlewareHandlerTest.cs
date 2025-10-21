using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using WebAPI.Middleware;

namespace Tests.Middleware;

public class CustomAuthMiddlewareHandlerTest
{
    private readonly DefaultHttpContext _httpContext;
    private readonly RequestDelegate _next;
    private readonly AuthorizationPolicy _policy;
    private readonly IAuthorizationMiddlewareResultHandler _handler;
        
    public CustomAuthMiddlewareHandlerTest()
    {
        this._httpContext = new DefaultHttpContext();
        this._policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        this._next = new RequestDelegate(_ => Task.CompletedTask);
        this._handler = new CustomAuthMiddlewareHandler();
    }

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