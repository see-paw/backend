using Application.Core;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

/// <summary>
/// Service that orchestrates the upload of multiple images for any entity.
/// </summary>
/// <typeparam name="T">The type of entity that can have images.</typeparam>
public interface IImagesUploader<T> where T : class

{
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
    Task<Result<Unit>> UploadImagesAsync(
        string entityId,
        List<IFormFile> files,
        List<Image> metadata,
        CancellationToken ct);
}