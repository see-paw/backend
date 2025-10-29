using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Images.Commands;

/// <summary>
/// Command and handler for setting the main image of an animal.
/// </summary>
public class SetAnimalPrincipalImage
{
    /// <summary>
    /// Request to set a specific image as the animal's main image.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// The ID of the animal whose main image will be updated.
        /// </summary>
        public required string AnimalId { get; set; }

        /// <summary>
        /// The ID of the image to set as the main image.
        /// </summary>
        public required string ImageId { get; set; }
    }
    
    /// <summary>
    /// Handles the process of setting an animal's main image.
    /// </summary>
    public class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        /// <summary>
        /// Sets the main image for the specified animal.
        /// </summary>
        /// <param name="request">The command containing the animal ID and image ID.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> indicating success or failure of the operation.
        /// </returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs while saving changes.</exception>
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var animal = await dbContext.Animals
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

            if (animal == null)
            {
                return Result<Unit>.Failure("Animal not found", 404);
            }
            
            var image = await dbContext.Images
                .FirstOrDefaultAsync(i => i.Id == request.ImageId, ct);

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