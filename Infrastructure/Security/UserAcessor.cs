using System.Security.Claims;
using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Persistence;

namespace Infrastructure.Security;

/// <summary>
/// Provides access to information about the currently authenticated user.
///
/// This class acts as a bridge between the HTTP context and the application layer,
/// allowing services to retrieve the active user's identity and corresponding domain entity
/// from the database.
/// </summary>
public class UserAccessor(IHttpContextAccessor httpContextAccessor,
    AppDbContext dbContext): IUserAccessor
{
    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user
    /// from the active <see cref="HttpContext"/>.
    /// </summary>
    /// <returns>
    /// A string containing the user ID (usually a GUID or equivalent unique identifier).
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when no user is found in the current context.
    /// </exception>
    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new Exception("User not found");
    }

    /// <summary>
    /// Asynchronously retrieves the <see cref="User"/> entity corresponding
    /// to the currently authenticated user from the database context.
    /// </summary>
    /// <returns>
    /// The <see cref="User"/> entity representing the currently logged-in user.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when no authenticated user exists or the user record cannot be found.
    /// </exception>
    public async Task<User> GetUserAsync()
    {
        var userId = GetUserId();
        return await dbContext.Users
            .Include(u => u.Shelter)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("No user logged in");
    }
}
