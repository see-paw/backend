using Application.Core;
using Application.Interfaces;

using Domain.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Persistence;

namespace Application.Fosterings.Queries
{
    /// <summary>
    /// Returns only the IDs of active fostering records for the authenticated user.
    /// </summary>
    public class GetActiveFosteringIds
    {
        /// <summary>
        /// Query to retrieve fostering and animal IDs for active fosterings.
        /// </summary>
        public class Query : IRequest<Result<List<FosteringIdResult>>> { }

        /// <summary>
        /// Lightweight result containing only IDs.
        /// </summary>
        public class FosteringIdResult
        {
            public string Id { get; set; } = string.Empty;
            public string AnimalId { get; set; } = string.Empty;
        }

        /// <summary>
        /// Handles the retrieval of active fostering IDs.
        /// </summary>
        public class Handler(AppDbContext context, IUserAccessor userAccessor)
            : IRequestHandler<Query, Result<List<FosteringIdResult>>>
        {
            public async Task<Result<List<FosteringIdResult>>> Handle(Query request, CancellationToken ct)
            {
                var currentUser = await userAccessor.GetUserAsync();

                if (currentUser == null)
                    return Result<List<FosteringIdResult>>.Failure("User not found", 404);

                var ids = await context.Fosterings
                    .Where(f => f.UserId == currentUser.Id && f.Status == FosteringStatus.Active)
                    .Select(f => new FosteringIdResult
                    {
                        Id = f.Id,
                        AnimalId = f.AnimalId
                    })
                    .ToListAsync(ct);

                return Result<List<FosteringIdResult>>.Success(ids, 200);
            }
        }
    }
}
