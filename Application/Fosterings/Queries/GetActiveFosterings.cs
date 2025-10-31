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
        /// <summary>
        /// Represents a query to retrieve all active fostering records for a specific user.
        /// </summary>
        public class Query : IRequest<Result<List<Fostering>>>
        {
            /// <summary>Authenticated user's Id.</summary>
            public required string UserId { get; set; }
        }
        
        /// <summary>
        /// Handles the retrieval of all active fostering records for a specific user.
        /// </summary>
        public class Handler : IRequestHandler<Query, Result<List<Fostering>>>
        {
            private readonly AppDbContext _context;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">The application's database context.</param>
            public Handler(AppDbContext context) => _context = context;

            /// <summary>
            /// Retrieves all active fostering records for the specified user.
            /// </summary>
            /// <param name="request">The query containing the user's ID.</param>
            /// <param name="ct">A token to cancel the operation.</param>
            /// <returns>
            /// A <see cref="Result{T}"/> containing a list of <see cref="Fostering"/> records if found,  
            /// or an error message with the appropriate status code otherwise.
            /// </returns>
            /// <exception cref="Exception">Thrown if a database query fails unexpectedly.</exception>
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