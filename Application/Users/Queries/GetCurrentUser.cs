using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Queries;

/// <summary>
/// Retrieves complete information about the currently authenticated user,
/// including role and shelter ID (if AdminCAA).
/// </summary>
public class GetCurrentUser
{
    /// <summary>
    /// Query request to retrieve the currently authenticated user's information.
    /// Does not require any parameters as the user is identified from the authentication context.
    /// </summary>
    public class Query : IRequest<Result<UserInfo>>
    {
    }

    /// <summary>
    /// Contains complete information about the authenticated user.
    /// This class is used as the result type for the <see cref="GetCurrentUser"/> query.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The full name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The role assigned to the user.
        /// Possible values: "User", "AdminCAA", "PlatformAdmin".
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The identifier of the shelter managed by the user.
        /// Only populated when the user has the AdminCAA role; otherwise null.
        /// </summary>
        public string? ShelterId { get; set; }

        /// <summary>
        /// The user's date of birth.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// The street address of the user.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// The city where the user resides.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The postal code of the user's address.
        /// Format: 0000-000 (Portuguese postal code format).
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// The user's phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handles the retrieval of the currently authenticated user's information.
    /// </summary>
    public class Handler : IRequestHandler<Query, Result<UserInfo>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="userAccessor">Service to access the currently authenticated user from the HTTP context.</param>
        /// <param name="userManager">ASP.NET Core Identity user manager for role retrieval.</param>
        public Handler(IUserAccessor userAccessor, UserManager<User> userManager)
        {
            _userAccessor = userAccessor;
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves complete information about the currently authenticated user.
        /// </summary>
        /// <param name="request">The query request (contains no parameters).</param>
        /// <param name="ct">Cancellation token for async operation control.</param>
        /// <returns>
        /// A <see cref="Result{UserInfo}"/> containing the user's complete information if successful,
        /// or a failure result with status code 401 if the user is not authenticated.
        /// </returns>
        public async Task<Result<UserInfo>> Handle(Query request, CancellationToken ct)
        {
            var user = await _userAccessor.GetUserAsync();

            if (user == null)
                return Result<UserInfo>.Failure("User not authenticated", 401);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var userInfo = new UserInfo
            {
                UserId = user.Id,
                Email = user.Email!,
                Name = user.Name,
                Role = role,
                ShelterId = user.ShelterId,
                BirthDate = user.BirthDate,
                Street = user.Street,
                City = user.City,
                PostalCode = user.PostalCode,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return Result<UserInfo>.Success(userInfo, 200);
        }
    }
}
