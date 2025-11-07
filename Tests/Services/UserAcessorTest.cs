using System.Security.Claims;
using Domain;
using Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Services;

/// <summary>
/// Unit tests for the <see cref="UserAccessor"/> class.
/// 
/// This test suite verifies that the <see cref="UserAccessor"/> behaves correctly 
/// when retrieving user identifiers and user entities from the current HTTP context 
/// and the application database.
/// </summary>
public class UserAccessorTest
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserAccessorTest"/> class.
    /// 
    /// Sets up an in-memory database and a mocked <see cref="IHttpContextAccessor"/> 
    /// to simulate various authentication states in a controlled test environment.
    /// </summary>
    public UserAccessorTest()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    /// <summary>
    /// Verifies that <see cref="UserAccessor.GetUserId"/> returns the expected user ID
    /// when a valid authenticated user exists in the HTTP context.
    /// </summary>
    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenUserIsAuthenticated()
    {
        //Arrange
        var claims = new List<Claim>{new (ClaimTypes.NameIdentifier, "user123")};
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        
        _httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);
        using var context = new AppDbContext(_options);
        var userAccessor = new UserAccessor(_httpContextAccessor.Object, context);
        
        // Act
        var result = userAccessor.GetUserId();

        // Assert
        Assert.Equal("user123", result);
    }
    
    /// <summary>
    /// Verifies that <see cref="UserAccessor.GetUserId"/> throws an <see cref="Exception"/>
    /// when there is no authenticated user present in the HTTP context.
    /// </summary>
    [Fact]
    public void GetUserId_ShouldThrowException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var httpContext = new DefaultHttpContext(); // no authenticated user
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        using var context = new AppDbContext(_options);
        var userAccessor = new UserAccessor(_httpContextAccessor.Object, context);

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => userAccessor.GetUserId());
        Assert.Equal("User not found", ex.Message);
    }
    
    /// <summary>
    /// Verifies that <see cref="UserAccessor.GetUserAsync"/> returns the correct <see cref="User"/> entity
    /// when the authenticated user's ID exists in the database.
    /// </summary>
    [Fact]
    public async Task GetUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        const string userId = "user-abc";
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        await using var context = new AppDbContext(_options);
        var user = new User { Id = userId, UserName = "TestUser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userAccessor = new UserAccessor(_httpContextAccessor.Object, context);

        // Act
        var result = await userAccessor.GetUserAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestUser", result.UserName);
        Assert.Equal(userId, result.Id);
    }

    /// <summary>
    /// Verifies that <see cref="UserAccessor.GetUserAsync"/> throws an <see cref="UnauthorizedAccessException"/>
    /// when the authenticated user's ID does not exist in the database.
    /// </summary>
    [Fact]
    public async Task GetUserAsync_ShouldThrowUnauthorizedAccess_WhenUserDoesNotExist()
    {
        // Arrange
        const string userId = "nonexistent-user";
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        await using var context = new AppDbContext(_options);
        var userAccessor = new UserAccessor(_httpContextAccessor.Object, context);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => userAccessor.GetUserAsync());
        Assert.Equal("No user logged in", ex.Message);
    }
}