using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Queries
{
    /// <summary>
    /// Handles the retrieval of the currently authenticated <see cref="User"/> profile.
    /// Provides basic personal and contact information for display in the profile page.
    /// </summary>
    public class GetUserProfile
    {
        /// <summary>
        /// Query that requests the authenticated user's profile data.
        /// </summary>
        public class Query : IRequest<Result<User>>
        {
            /// <summary>
            /// The unique identifier of the authenticated user.
            /// </summary>
            public required string UserId { get; set; }
        }

        /// <summary>
        /// Handles the execution of the <see cref="Query"/> to fetch a user profile.
        /// </summary>
        public class Handler : IRequestHandler<Query, Result<User>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result<User>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Retrieve the user from the database along with their shelter (if any)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                // Validate user existence
                if (user == null)
                    return Result<User>.Failure("User not found", 404);

                // Return the full user entity (to be mapped into ResUserProfileDto)
                return Result<User>.Success(user, 200);
            }
        }
    }
}