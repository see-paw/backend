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
            var shelter = await context.Shelters.FindAsync([request.Animal.ShelterId], cancellationToken);

            if (shelter == null)
            {
                return Result<string>.Failure("Shelter not found", 404);
            }

            var breed = await context.Breeds.FindAsync([request.Animal.BreedId], cancellationToken);

            if (breed == null)
            {
                return Result<string>.Failure("Breed not found", 404);
            }

            request.Animal.Breed = breed;
            request.Animal.Shelter = shelter;

            context.Animals.Add(request.Animal); //não é necessária a versão assíncrona

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            return result ? Result<string>.Success(request.Animal.Id) 
                : Result<string>.Failure("Failed to add the animal", 400);
        }
    }
}