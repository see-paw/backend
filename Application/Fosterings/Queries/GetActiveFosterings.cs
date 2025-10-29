using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Fosterings.Queries
{
    /// <summary>
    /// Returns all active fostering records for a given user, including the Animal and its Images.
    /// </summary>
    public class GetActiveFosterings
    {
        public class Query : IRequest<Result<List<Fostering>>>
        {
            /// <summary>Authenticated user's Id.</summary>
            public required string UserId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<Fostering>>>
        {
            private readonly AppDbContext _context;
            public Handler(AppDbContext context) => _context = context;

            public async Task<Result<List<Fostering>>> Handle(Query request, CancellationToken ct)
            {
                var fosterings = await _context.Fosterings
                    .Include(f => f.Animal)
                    .ThenInclude(a => a.Images)
                    .Where(f => f.UserId == request.UserId && f.Status == FosteringStatus.Active)
                    .ToListAsync(ct);

                return fosterings.Count == 0 ? Result<List<Fostering>>.Failure("No active fostering records found.", 404) : Result<List<Fostering>>.Success(fosterings, 200);
            }
        }
    }
}