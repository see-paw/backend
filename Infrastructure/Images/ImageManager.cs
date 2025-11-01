using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Images;

/// <summary>
/// Provides application-level services for managing images of an entity.
/// </summary>
public class ImageManager<T>(
    AppDbContext dbContext,
    ICloudinaryService cloudinaryService,
    IImageOwnerLoader<T> loader,
    IImageOwnerLinker<T> linker
) : IImageManager<T> where T : class, IHasImages
{
    /// <summary>
    /// Adds an image to the specified entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="description">A short description of the image.</param>
    /// <param name="isPrincipal">Whether the image should be set as the main image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> with the created <see cref="Image"/> if successful,  
    /// or an error message otherwise.
    /// </returns>
    public async Task<Result<Image>> AddImageAsync(
        string entityId,
        IFormFile file,
        string description,
        bool isPrincipal,
        CancellationToken ct)
    {
        var entity = await loader.LoadAsync(dbContext, entityId, ct);
        
        var folder = $"{typeof(T).Name}/{entityId}";
        var uploadResult = await cloudinaryService.UploadImage(file, folder);
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

        return Result<Image>.Success(img, 201);
    }
    
    /// <summary>
    /// Deletes an image from the specified entity, removing it from both the database and Cloudinary.
    /// </summary>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="imageId">The ID of the image to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> with <see cref="Unit"/> if successful,  
    /// or an error message otherwise.
    /// </returns>
    public async Task<Result<Unit>> DeleteImageAsync(
        string entityId,
        string imageId,
        CancellationToken ct)
    {
        var entity = await loader.LoadAsync(dbContext, entityId, ct);
        
        if  (entity == null)
            return Result<Unit>.Failure($"{typeof(T).Name} not found", 404);

        var image = await dbContext.Images
            .FirstOrDefaultAsync(i => i.Id == imageId, ct);
        
        if (image == null)
            return Result<Unit>.Failure("Image not found.", 404);

        if (image.IsPrincipal)
            return Result<Unit>.Failure("Cannot delete the principal image.", 400);

        var entityIdProperty = typeof(T).Name + "Id";
        var imageEntityId = image.GetType()
            .GetProperty(entityIdProperty)
            ?.GetValue(image)
            ?.ToString();

        if (imageEntityId != entityId)
            return Result<Unit>.Failure("Image does not belong to the specified entity.", 403);

        var deleteResult = await cloudinaryService.DeleteImage(image.PublicId);
        
        if (!string.Equals(deleteResult, "ok", StringComparison.OrdinalIgnoreCase))
            return Result<Unit>.Failure($"Cloudinary deletion failed ({deleteResult}).", 502);

        dbContext.Images.Remove(image);
        var saved = await dbContext.SaveChangesAsync(ct) > 0;

        return saved
            ? Result<Unit>.Success(Unit.Value, 204)
            : Result<Unit>.Failure("Failed to delete image from database.", 500);
    } 
}