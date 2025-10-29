using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    public class Handler(AppDbContext dbContext
        , IImagesUploader<Animal> uploadService) : IRequestHandler<Command, Result<List<Image>>>
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
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            var animal = await dbContext.Animals
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == request.AnimalId, ct);

            if (animal == null)
                return Result<List<Image>>.Failure("Animal not found", 404);

            var imageCountBefore = animal.Images.Count;

            var uploadResult = await uploadService.UploadImagesAsync(
                request.AnimalId,
                request.Files,
                request.Images,
                ct
            );

            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync(ct);
                return Result<List<Image>>.Failure(uploadResult.Error, uploadResult.Code);
            }

            var success = await dbContext.SaveChangesAsync(ct) > 0;
            if (!success)
            {
                await transaction.RollbackAsync(ct);
                return Result<List<Image>>.Failure("Failed to save images", 500);
            }

            await transaction.CommitAsync(ct);

            var addedImages = animal.Images.Skip(imageCountBefore).ToList();
            return Result<List<Image>>.Success(addedImages, 201);
        }
    }
}