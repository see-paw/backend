using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Images;

/// <summary>
/// Service that orchestrates the upload of multiple images for any entity.
/// </summary>
/// <typeparam name="T">The type of entity that can have images.</typeparam>
public class ImagesUploader<T> : IImagesUploader<T> where T : class, IHasImages

{
    private readonly IImageManager<T> _imageManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImagesUploader{T}"/> class.
    /// </summary>
    /// <param name="imageManager">The service used to handle individual image uploads.</param>
    public ImagesUploader(IImageManager<T> imageManager)
    {
        _imageManager = imageManager;
    }

    /// <summary>
    /// Uploads multiple images for a specific entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity to associate images with.</param>
    /// <param name="files">The image files to upload.</param>
    /// <param name="metadata">The metadata for each image (description, isPrincipal).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A Result indicating success or failure. Does NOT persist changes - caller must call SaveChangesAsync.
    /// </returns>
    public async Task<Result<Unit>> UploadImagesAsync(
        string entityId,
        List<IFormFile> files,
        List<Image> metadata,
        CancellationToken ct)

    {
        for (var i = 0; i < files.Count; i++)
        {
            var file = files[i];
            var meta = metadata[i];
            
            var result = await _imageManager.AddImageAsync(
                entityId,
                file,
                meta.Description ?? string.Empty,
                meta.IsPrincipal,
                ct
            );
            
            if (!result.IsSuccess)

                return Result<Unit>.Failure($"Image upload failed: {result.Error}", 400);
        }
        
        return Result<Unit>.Success(Unit.Value, 201);
    }
}