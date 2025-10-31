using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Ownerships.Queries;

public class GetOwnershipRequestsByUser
{
    public class Query : IRequest<Result<List<OwnershipRequest>>>
    {
        
    }

    public class Handler(AppDbContext context, IUserAccessor userAcessor) : IRequestHandler<Query, Result<List<OwnershipRequest>>>
    {
        public async Task<Result<List<OwnershipRequest>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAcessor.GetUserAsync();
            
            if (currentUser == null)
                return Result<List<OwnershipRequest>>.Failure("User not found", 404);
            
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            
            var ownershipRequests = await context.OwnershipRequests
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Breed)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Shelter)
                .Include(or => or.Animal)
                    .ThenInclude(a => a.Images)
                .Where(or => or.UserId == currentUser.Id && 
                             ((or.Status != OwnershipStatus.Rejected && or.Status != OwnershipStatus.Approved) ||
                              (or.Status == OwnershipStatus.Rejected && or.UpdatedAt >= oneMonthAgo)))
                .OrderBy(or => or.Status == OwnershipStatus.Rejected ? 1 : 0) // Non rejected first
                .ThenByDescending(or => or.RequestedAt) // then by decreasing order
                .ToListAsync(cancellationToken);
            
            return Result<List<OwnershipRequest>>.Success(ownershipRequests, 200);

        }
    }
}