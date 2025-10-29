using Application.Core;
using Domain;
using MediatR;

namespace Application.Fosterings.Commands;

public class AddFostering
{
    public class Command : IRequest<Result<Fostering>>
    {
        public required string AnimalId { get; set; }
        
        public required string UserId { get; set; }
    }
    
    public class Handler : IRequestHandler<Command, Result<Fostering>>
    {
        public async Task<Result<Fostering>> Handle(Command request, CancellationToken ct)
        {
            
        }
    }
}