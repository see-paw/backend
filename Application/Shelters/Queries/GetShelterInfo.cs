using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shelters.Queries;

public class GetShelterInfo
{
    /// <summary>
    /// Query to retrieve shelter information by a given ShelterId.
    /// </summary>
    public class Query(string shelterId) : IRequest<Result<Shelter>>
    {
        public string ShelterId { get; } = shelterId;
    }

    /// <summary>
    /// Handler that retrieves the shelter information.
    /// </summary>
    public class Handler(AppDbContext context)
        : IRequestHandler<Query, Result<Shelter>>
    {
        public async Task<Result<Shelter>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ShelterId))
                return Result<Shelter>.Failure("Invalid shelter ID", 400);

            var shelter = await context.Shelters
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.ShelterId, cancellationToken);

            if (shelter == null)
                return Result<Shelter>.Failure("Shelter not found", 404);

            return Result<Shelter>.Success(shelter, 200);
        }
    }
}
