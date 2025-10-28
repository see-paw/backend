using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Persistence;

namespace Application.Images.Commands;

public class DeleteAnimalImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string AnimalId { get; init; }

        public required string ImageId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var animal = await dbContext.Animals.FindAsync([request.AnimalId], ct);
            
            if  (animal == null)
                return Result<Unit>.Failure("Animal not found", 404);
            
            var image = await dbContext.Images.FindAsync([request.ImageId], ct);
            
            if (image == null)
                return Result<Unit>.Failure("Image not found", 404);

            if (image.IsPrincipal)
            {
                return Result<Unit>.Failure("Cannot delete Animal's main image", 404);
            }
            
            if (image.AnimalId != animal.Id)
                return Result<Unit>.Failure("Image does not belong to the specified animal.", 403);
            
            var result = await imageAppService.DeleteImageAsync(dbContext, animal.Id, image.PublicId, ct);

            return result;
        }
    }
}