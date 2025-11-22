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
    public class Query : IRequest<Result<UserInfo>>
    {
    }

    public class UserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ShelterId { get; set; }
        public DateTime BirthDate { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Query, Result<UserInfo>>
    {
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<User> _userManager;

        public Handler(IUserAccessor userAccessor, UserManager<User> userManager)
        {
            _userAccessor = userAccessor;
            _userManager = userManager;
        }

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
