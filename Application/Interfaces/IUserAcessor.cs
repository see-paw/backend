using Domain;

namespace Application.Interfaces;

/// <summary>
/// Defines a contract for accessing the currently authenticated user within the application layer.
/// 
/// This interface abstracts the retrieval of user information from the underlying HTTP context
/// or authentication mechanism, allowing other components to obtain the user's identity or full domain entity
/// without depending directly on the web framework.
/// </summary>
public interface IUserAcessor
{
    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// A string representing the authenticated user's unique ID (commonly a GUID or equivalent).
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the user is not found in the current context or no user is authenticated.
    /// </exception>
    string GetUserId();
    
    /// <summary>
    /// Asynchronously retrieves the <see cref="User"/> entity corresponding
    /// to the currently authenticated user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.  
    /// The task result contains the <see cref="User"/> entity.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when no authenticated user exists or when the user record cannot be retrieved.
    /// </exception>
    Task<User> GetUserAsync();
}