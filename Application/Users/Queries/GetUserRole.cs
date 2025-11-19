using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Core;
using Application.Interfaces;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Persistence;

namespace Application.Users.Queries;

/// <summary>
/// Handles retrieval of the current user's role.
/// 
/// This query determines the user's role based on their ShelterId:
/// - If ShelterId is null, the user is a regular "User"
/// - If ShelterId has a value, the user is an "AdminCAA" (shelter administrator)
/// </summary>
public class GetUserRole
{
    /// <summary>
    /// Query to get the current user's role.
    /// </summary>
    public class Query : IRequest<Result<string>>
    {
    }

    /// <summary>
    /// Handles the retrieval of the current authenticated user's role.
    /// </summary>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Query, Result<string>>
    {
        /// <summary>
        /// Retrieves the role of the currently authenticated user.
        /// 
        /// This method performs the following operations:
        /// - Gets the current user's ID from the authentication context
        /// - Retrieves the user from the database
        /// - Determines the role based on ShelterId value
        /// - Returns "AdminCAA" if user manages a shelter, "User" otherwise
        /// </summary>
        /// <param name="request">The query request (no parameters needed).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the user's role as a string if successful,
        /// or an error message with appropriate status code if the user is not found.
        /// </returns>
        public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return Result<string>.Failure("User not found", 404);

            var role = user.ShelterId != null ? "AdminCAA" : "User";

            return Result<string>.Success(role, 200);
        }
    }
}







