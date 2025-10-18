using Application.Core;
using AutoMapper;
using Domain;
using Domain.Enums;
using MediatR;
using Persistence;

namespace Application.Animals.Commands;
public class CreateAnimal
{
    public class Command : IRequest<Result<string>>
    {
        public required Animal Animal { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {

            context.Animals.Add(request.Animal); //não é necessária a versão assíncrona

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<string>.Failure("Failed to add the animal", 400);
            }

            return Result<string>.Success(request.Animal.AnimalId);
        }
    }
}