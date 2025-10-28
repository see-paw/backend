using Application.Core;
using Application.Images.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Images.Commands;

public class SetAnimalPrincipalImage
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string AnimalId { get; set; }

        public required string ImageId { get; set; }
    }

    public class Handler(AppDbContext dbContext, IPrincipalImageEnforcer enforcer) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var animal = await dbContext.Animals
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

            if (animal == null)
            {
                return Result<Unit>.Failure("Animal not found", 404);
            }
            
            var image = animal.Images.FirstOrDefault(i => i.Id == request.ImageId);

            if (image == null)
            {
                return Result<Unit>.Failure("Image not found", 404);
            }
            
            if (image.AnimalId != animal.Id)
                return Result<Unit>.Failure("Image does not belong to the specified animal.", 403);

            if (image.IsPrincipal)
            {
                return Result<Unit>.Failure("Image already is the anima\'s main image", 400);
            }

            var principalImage = animal.Images.FirstOrDefault(i => i.IsPrincipal);
            
            if (principalImage != null)
            {
                principalImage.IsPrincipal = false;
                await dbContext.SaveChangesAsync(ct); 
            }
            
            image.IsPrincipal = true;

            var saved = await dbContext.SaveChangesAsync(ct) > 0;

            return !saved ? Result<Unit>.Failure("Failed to change main image", 500) : Result<Unit>.Success(Unit.Value, 204);
        }
    }
}