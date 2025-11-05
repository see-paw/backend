using Application.Core;
using Application.Users.Commands;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;

namespace Tests.UserTests.Handlers;

public class EditUserProfileHandlerTests
{
    //codacy: ignore[complexity]
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"UserDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private User CreateUser(string id)
    {
        return new User
        {
            Id = id,
            Name = "Original ShelterName",
            BirthDate = new DateTime(1995, 5, 20),
            Street = "Old Street 123",
            City = "Lisbon",
            PostalCode = "1000-001",
            PhoneNumber = "910000000",
            CreatedAt = DateTime.UtcNow
        };
    }

    private User CreateUpdatedUser()
    {
        return new User
        {
            Name = "Updated ShelterName",
            BirthDate = new DateTime(2000, 1, 1),
            Street = "New Street 45",
            City = "Porto",
            PostalCode = "4000-222",
            PhoneNumber = "930000000"
        };
    }

    [Fact]
    public async Task Handle_UserExists_ShouldUpdateProfileSuccessfully()
    {
        var ctx = CreateContext();
        var userId = Guid.NewGuid().ToString();
        var existingUser = CreateUser(userId);
        ctx.Users.Add(existingUser);
        await ctx.SaveChangesAsync();

        var updatedUser = CreateUpdatedUser();

        var handler = new EditUserProfile.Handler(ctx);
        var cmd = new EditUserProfile.Command
        {
            UserId = userId,
            UpdatedUser = updatedUser
        };

        var result = await handler.Handle(cmd, default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        var ctx = CreateContext();
        var handler = new EditUserProfile.Handler(ctx);

        var cmd = new EditUserProfile.Command
        {
            UserId = Guid.NewGuid().ToString(),
            UpdatedUser = CreateUpdatedUser()
        };

        var result = await handler.Handle(cmd, default);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldReturnFailure()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"Fail_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var ctx = new FailingAppDbContext(options);
        var userId = Guid.NewGuid().ToString();
        var existingUser = CreateUser(userId);
        ctx.Users.Add(existingUser);
        await ctx.SaveChangesAsync();

        var handler = new EditUserProfile.Handler(ctx);
        var cmd = new EditUserProfile.Command
        {
            UserId = userId,
            UpdatedUser = CreateUpdatedUser()
        };

        var result = await handler.Handle(cmd, default);

        Assert.False(result.IsSuccess);
    }

    private class FailingAppDbContext : AppDbContext
    {
        public FailingAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0); // Force failure
    }
}
