using Application.Core;
using Domain;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Animals.Commands;

public class CreateAnimal
{
    public class Command : IRequest<Result<string>>
    {
        public required Animal Animal { get; set; }
        public required string ShelterId { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate that the shelter exists
            var shelterExists = await context.Shelters
                .AnyAsync(s => s.Id == request.ShelterId, cancellationToken);

            if (!shelterExists)
                return Result<string>.Failure("Shelter not found", 404);

            request.Animal.ShelterId = request.ShelterId;


            //Persist the entity
            context.Animals.Add(request.Animal);
            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<string>.Failure("Failed to create animal", 400);

            return Result<string>.Success(request.Animal.Id);
        }
    }
}
