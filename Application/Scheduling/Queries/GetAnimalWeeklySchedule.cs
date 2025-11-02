using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Scheduling.Queries;

public class GetAnimalWeeklySchedule
{
    public class Query : IRequest<Result<AnimalWeeklySchedule>>
    {
        public string AnimalId { get; set; }

        public DateOnly StartDate { get; set; }
    }

    public class Handler(
        AppDbContext dbContext,
        IUserAccessor userAccessor
        ) : IRequestHandler<Query, Result<AnimalWeeklySchedule>>
    {
        public async Task<Result<AnimalWeeklySchedule>> Handle(Query request, CancellationToken ct)
        {
            
        }
    }
}