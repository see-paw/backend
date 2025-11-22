using Application.Interfaces;
using Application.Users.Queries;

using Domain;

using Microsoft.AspNetCore.Identity;

using Moq;

namespace Tests.Users.Queries;

public class GetCurrentUserTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly GetCurrentUser.Handler _handler;

    public GetCurrentUserTests()
    {
        _userAccessorMock = new Mock<IUserAccessor>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _handler = new GetCurrentUser.Handler(_userAccessorMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserInfo_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "Test User",
            BirthDate = new DateTime(1990, 5, 15),
            Street = "Test Street 123",
            City = "Porto",
            PostalCode = "4000-100",
            PhoneNumber = "912345678",
            ShelterId = null
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        var result = await _handler.Handle(new GetCurrentUser.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("user@example.com", result.Value.Email);
        Assert.Equal("Test User", result.Value.Name);
        Assert.Equal("User", result.Value.Role);
        Assert.Null(result.Value.ShelterId);
    }

    [Fact]
    public async Task Handle_ShouldIncludeShelterId_WhenUserIsAdminCAA()
    {
        var userId = Guid.NewGuid().ToString();
        var shelterId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "admin@shelter.com",
            Name = "Admin CAA",
            BirthDate = new DateTime(1985, 3, 20),
            Street = "Admin Street 456",
            City = "Lisboa",
            PostalCode = "1000-200",
            PhoneNumber = "923456789",
            ShelterId = shelterId
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "AdminCAA" });

        var result = await _handler.Handle(new GetCurrentUser.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("AdminCAA", result.Value.Role);
        Assert.Equal(shelterId, result.Value.ShelterId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthenticated()
    {
        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(new GetCurrentUser.Query(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(401, result.Code);
        Assert.Equal("User not authenticated", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldDefaultToUserRole_WhenNoRolesAssigned()
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "norole@example.com",
            Name = "No Role User",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            Street = "Street",
            City = "City",
            PostalCode = "1234-567"
        };

        _userAccessorMock.Setup(x => x.GetUserAsync())
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var result = await _handler.Handle(new GetCurrentUser.Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("User", result.Value!.Role);
    }
}
