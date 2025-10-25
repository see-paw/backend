using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OwnershipRequests.Queries;

public class GetOwnershipRequestsByShelter
{
    public class Query: IRequest<Result<PagedList<OwnershipRequest>>>
    {
        /// <summary>
        /// The page number to retrieve (default is 1).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// The number of items per page (default is 20).
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    public class Handler(AppDbContext context, IUserAcessor userAcessor) : IRequestHandler<Query, Result<PagedList<OwnershipRequest>>>
    {
        public async Task<Result<PagedList<OwnershipRequest>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var currentUser = await userAcessor.GetUserAsync();
            if (currentUser.ShelterId == null)
            {
                return Result<PagedList<OwnershipRequest>>.Failure(
                    "Non Authorized Access: Only shelter adiministrators can view ownership requests", 403);
            }

            // Shelter referenced by the User has to exist
            var shelterExists = await context.Shelters.AnyAsync(s => s.Id == currentUser.ShelterId, cancellationToken);
            if (!shelterExists)
            {
                return Result<PagedList<OwnershipRequest>>.Failure(
                    "Shelter not found", 404);
            }

            var query = context.OwnershipRequests
                .Include(or => or.Animal)
                .Include(or => or.User)
                .Where(or => or.Animal.ShelterId == currentUser.ShelterId)
                .OrderByDescending(or => or.RequestedAt);

            var pagedList = await PagedList<OwnershipRequest>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<OwnershipRequest>>.Success(pagedList, 200);
        }
    }
    
        
}