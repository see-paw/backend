using Application.Animals.DTOs;
using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Animals.Commands;
public class CreateAnimal
{
    public class Command : IRequest<Result<string>>
    {
        public required AnimalDto AnimalDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {

            var animal = mapper.Map<Animal>(request.AnimalDto);

            context.Animals.Add(animal); //não é necessária a versão assíncrona

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<string>.Failure("Failed to delete the animal", 400);
            }

            return Result<string>.Success(animal.AnimalId);
        }
    }
}