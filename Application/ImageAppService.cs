using Application.Core;
using Application.Images.Services;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application;
public class ImageAppService<T>(
    IImageService imageService,
    IImageOwnerLoader<T> loader,
    IImageOwnerLinker<T> linker,
    IPrincipalImageEnforcer principalImageEnforcer
) : IImageAppService<T> where T : class, IHasPhotos
{
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