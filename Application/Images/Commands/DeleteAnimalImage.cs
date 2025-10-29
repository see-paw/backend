using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;

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
    public class Handler(IImageManager<Animal> imageManager) : IRequestHandler<Command, Result<Unit>>
    {
        /// <summary>
        /// Executes the command to delete an image from the specified animal.
        /// </summary>
        /// <param name="request">The command containing the animal and image identifiers.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> with <see cref="Unit"/> if successful,  
        /// or an error message otherwise.
        /// </returns>
        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            return await imageManager.DeleteImageAsync(request.AnimalId, request.ImageId, ct);
        }
    }
}