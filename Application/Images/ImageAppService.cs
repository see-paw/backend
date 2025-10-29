using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Images;

/// <summary>
/// Provides application-level services for managing images of an entity.
/// </summary>
public class ImageAppService<T>(
    IImageService imageService,
    IImageOwnerLoader<T> loader,
    IImageOwnerLinker<T> linker,
    IPrincipalImageEnforcer principalImageEnforcer
) : IImageAppService<T> where T : class, IHasImages
{
    /// <summary>
    /// Adds an image to the specified entity.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="description">A short description of the image.</param>
    /// <param name="isPrincipal">Whether the image should be set as the main image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> with the created <see cref="Image"/> if successful,  
    /// or an error message otherwise.
    /// </returns>
    /// <exception cref="Exception">Thrown if upload or database operations fail unexpectedly.</exception>
    public async Task<Result<Image>> AddImageAsync(
        AppDbContext dbContext,
        string entityId,
        IFormFile file,
        string description,
        bool isPrincipal,
        CancellationToken ct)
    {
        var entity = await loader.LoadAsync(dbContext, entityId, ct);
        
        var folder = $"SeePaw/{typeof(T).Name}/{entityId}";
        var uploadResult = await imageService.UploadImage(file, folder);
        if (uploadResult == null)
            return Result<Image>.Failure("Failed to upload photo.", 400);

        var img = new Image
        {
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            Description = description,
            IsPrincipal = isPrincipal
        };

        linker.Link(entity, img, entityId);
        principalImageEnforcer.EnforceSinglePrincipal(entity.Images, img);

        var saved = await dbContext.SaveChangesAsync(ct) > 0;
        
        return saved
            ? Result<Image>.Success(img, 201)
            : Result<Image>.Failure("Failed to save image.", 500);
    }

    /// <summary>
    /// Deletes an image from the specified entity.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="publicId">The public Cloudinary ID of the image to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success or failure of the deletion.
    /// </returns>
    /// <exception cref="Exception">Thrown if Cloudinary deletion or database save fails unexpectedly.</exception>
    public async Task<Result<Unit>> DeleteImageAsync(
        AppDbContext dbContext,
        string entityId,
        string publicId,
        CancellationToken ct)
    {
        var entity = await loader.LoadAsync(dbContext, entityId, ct);

        var image = entity.Images.FirstOrDefault(i => i.PublicId == publicId);
        
        if (image == null)
            return Result<Unit>.Failure("Image not found.", 404);

        var deleteResult = await imageService.DeleteImage(publicId);
        
        if (!string.Equals(deleteResult, "ok", StringComparison.OrdinalIgnoreCase))
            return Result<Unit>.Failure($"Cloudinary deletion failed ({deleteResult}).", 502);

        dbContext.Images.Remove(image);
        var saved = await dbContext.SaveChangesAsync(ct) > 0;

        return saved
            ? Result<Unit>.Success(Unit.Value, 204)
            : Result<Unit>.Failure("Failed to delete image from database.", 500);
    }
}