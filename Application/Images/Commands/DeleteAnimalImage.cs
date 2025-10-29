using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Persistence;

namespace Application.Images.Commands;

/// <summary>
/// Command and handler for deleting an image from an animal.
/// </summary>
public class DeleteAnimalImage
{
    /// <summary>
    /// Request to delete a specific image from an animal.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// The ID of the animal that owns the image.
        /// </summary>
        public required string AnimalId { get; init; }

        /// <summary>
        /// The ID of the image to delete.
        /// </summary>
        public required string ImageId { get; set; }
    }

    /// <summary>
    /// Handles the image deletion process for a specific animal.
    /// </summary>
    public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<Unit>>
    {
        /// <summary>
        /// Deletes an image from the given animal.
        /// </summary>
        /// <param name="request">The command containing the animal ID and image ID.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> indicating success or failure of the deletion.
        /// </returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs during deletion.</exception>
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