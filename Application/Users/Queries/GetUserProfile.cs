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

            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">The application's database context.</param>
            public Handler(AppDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Retrieves the profile of the authenticated user from the database.
            /// </summary>
            /// <param name="request">The query containing the user's ID.</param>
            /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the user's profile data if found,  
            /// or an error message with the corresponding status code otherwise.
            /// </returns>
            /// <exception cref="Exception">Thrown if a database query fails unexpectedly.</exception>
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