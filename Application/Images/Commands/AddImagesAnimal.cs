using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Images.Commands;

/// <summary>
/// Command and handler for adding images to an animal.
/// </summary>
public class AddImagesAnimal
{
    /// <summary>
    /// Request to add one or more images to an animal.
    /// </summary>
    public class Command : IRequest<Result<List<Image>>>
    {
        /// <summary>
        /// The ID of the animal to which images will be added.
        /// </summary>
        public required string AnimalId { get; set; }
        
        /// <summary>
        /// The metadata for each image.
        /// </summary>
        public required List<Image> Images { get; set; }
        
        /// <summary>
        /// The uploaded image files.
        /// </summary>
        public required List<IFormFile> Files { get; set; }
    }

    /// <summary>
    /// Handles the image upload process for a specific animal.
    /// </summary>
    public class Handler(AppDbContext dbContext, IImageAppService<Animal> imageAppService) : IRequestHandler<Command, Result<List<Image>>>
    {
        /// <summary>
        /// Adds images to the specified animal.
        /// </summary>
        /// <param name="request">The command containing animal ID, image metadata, and files.</param>
        /// <param name="ct">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> with the list of uploaded <see cref="Image"/> objects if successful,  
        /// or an error message with a status code otherwise.
        /// </returns>
        /// <exception cref="Exception">Thrown if an unexpected error occurs during upload.</exception>
        public async Task<Result<List<Image>>> Handle(Command request, CancellationToken ct)
        {
            if (request.Files.Count != request.Images.Count)
                return Result<List<Image>>.Failure("Mismatch between files and image metadata.", 400);

            var animal = await dbContext.Animals.FindAsync([request.AnimalId], ct);

            if (animal == null)
                return Result<List<Image>>.Failure("Animal not found", 404);

            var resultImages = new List<Image>();
            
            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];
                var meta = request.Images[i];

                var imgResult = await imageAppService.AddImageAsync(
                    dbContext,
                    animal.Id,
                    file,
                    meta.Description ?? string.Empty,
                    meta.IsPrincipal,
                    ct
                );

                if (!imgResult.IsSuccess)
                {
                    return Result<List<Image>>.Failure($"Image upload failed: {imgResult.Error}", 400);
                }

                if (imgResult.Value != null)
                {
                    resultImages.Add(imgResult.Value);
                }
                else
                {
                    return Result<List<Image>>.Failure("Image upload returned null value.", 500);
                }
            }
            return Result<List<Image>>.Success(resultImages, 201);
        }
    }
}