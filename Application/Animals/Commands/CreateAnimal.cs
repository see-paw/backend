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

        public List<Image>? Images { get; set; }
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


            // Validate that the breed exists 
            if (!string.IsNullOrEmpty(request.Animal.BreedId))
            {
                var breedExists = await context.Breeds
                    .AnyAsync(b => b.Id == request.Animal.BreedId, cancellationToken);

                if (!breedExists)
                    return Result<string>.Failure("Breed not found", 404);
            }

            request.Animal.ShelterId = request.ShelterId;

            // Associate images with the animal
            if (request.Images != null && request.Images.Any())
            {
                request.Animal.Images = request.Images;
            }

            //Persist the entity
            context.Animals.Add(request.Animal);
            var success = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!success)
                return Result<string>.Failure("Failed to create animal", 400);

            return Result<string>.Success(request.Animal.Id);
        }
    }
}
