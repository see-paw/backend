using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Commands
{
    /// <summary>
    /// Handles the update of an existing <see cref="User"/> profile.
    /// The command allows authenticated users to modify their personal and contact information.
    /// </summary>
    public class EditUserProfile
    {
        /// <summary>
        /// Command that carries the updated user profile information.
        /// </summary>
        public class Command : IRequest<Result<User>>
        {
            /// <summary>
            /// The unique identifier of the user being updated (retrieved from authentication context).
            /// </summary>
            public required string UserId { get; set; }

            /// <summary>
            /// The <see cref="User"/> entity containing the updated profile information.
            /// </summary>
            public required User UpdatedUser { get; set; }
        }

        /// <summary>
        /// Handles the execution of the <see cref="Command"/> to update a user profile.
        /// Validates user existence, applies updates, and persists the changes.
        /// </summary>
        public class Handler : IRequestHandler<Command, Result<User>>
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
            /// Updates an existing user's profile with new personal and contact information.
            /// </summary>
            /// <param name="request">The command containing the user's ID and updated data.</param>
            /// <param name="cancellationToken">A token to cancel the operation if needed.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing the updated <see cref="User"/> if successful,  
            /// or an error message with the appropriate status code otherwise.
            /// </returns>
            /// <exception cref="Exception">Thrown if a database operation fails unexpectedly.</exception>
            public async Task<Result<User>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Retrieve the user from the database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

                if (user == null)
                    return Result<User>.Failure("User not found", 404);

                // Apply updates (only editable fields)
                user.Name = request.UpdatedUser.Name;
                user.BirthDate = request.UpdatedUser.BirthDate;
                user.Street = request.UpdatedUser.Street;
                user.City = request.UpdatedUser.City;
                user.PostalCode = request.UpdatedUser.PostalCode;
                user.PhoneNumber = request.UpdatedUser.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                // Save changes
                var success = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!success)
                    return Result<User>.Failure("Failed to update user profile", 400);

                return Result<User>.Success(user, 200);
            }
        }
    }
}
