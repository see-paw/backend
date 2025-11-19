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
/// Handles retrieval of the current user's ID.
/// 
/// This query extracts the user ID from the authenticated JWT token context.
/// </summary>
public class GetUserId
{
    /// <summary>
    /// Query to get the current user's ID.
    /// </summary>
    public class Query : IRequest<Result<string>>
    {
    }

    /// <summary>
    /// Handles the retrieval of the current authenticated user's ID.
    /// </summary>
    public class Handler(
        AppDbContext context,
        IUserAccessor userAccessor) : IRequestHandler<Query, Result<string>>
    {
        /// <summary>
        /// Retrieves the ID of the currently authenticated user.
        /// 
        /// This method performs the following operations:
        /// - Gets the current user's ID from the authentication context
        /// - Validates that the user exists in the database
        /// - Returns the user ID
        /// </summary>
        /// <param name="request">The query request (no parameters needed).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the user's ID as a string if successful,
        /// or an error message with appropriate status code if the user is not found.
        /// </returns>
        public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = userAccessor.GetUserId();

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return Result<string>.Failure("User not found", 404);

            return Result<string>.Success(userId, 200);
        }
    }
}
