using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WebAPI.Middleware;

namespace Tests.Middleware;

public class IdentityResponseMiddlewareTest
{
    private readonly DefaultHttpContext _context;
    private readonly RequestDelegate _next;
    private readonly IdentityResponseMiddleware _middleware;

    public IdentityResponseMiddlewareTest()
    {
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream(); 
        _next = new RequestDelegate(async ctx =>
        {
            // simula a escrita de uma resposta 401 com um corpo JSON específico
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var responseBody = ctx.Items["SimulatedBody"] as string ?? string.Empty;
            await ctx.Response.WriteAsync(responseBody);
        });
        _middleware = new IdentityResponseMiddleware(_next);
    }

    [Theory]
    [InlineData("InvalidLoginAttempt", "Invalid credentials. Please check your email or password.", null)]
    [InlineData("LockedOut", "Account temporarily blocked: maximum login attempts reached.", "LockedOut")]
    [InlineData("NotAllowed", "Account not confirmed. Please verify your email.", "NotAllowed")]
    [InlineData("OtherText", "Authentication required. Please provide valid credentials.", "Unauthorized")]
    public async Task InvokeAsync_ReturnsExpectedCustomJson(string inputBody, string expectedMessage, string? expectedDetails)
    {
        // Arrange
        _context.Items["SimulatedBody"] = inputBody;

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_context.Response.Body);
        var body = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.Equal(401, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedMessage, root.GetProperty("message").GetString());

        var detailsProp = root.GetProperty("details").GetString();
        Assert.Equal(expectedDetails, detailsProp);
    }
}