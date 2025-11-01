using Application.Core;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for managing images associated with entities.
/// </summary>
/// <typeparam name="T">The type of entity that can have images.</typeparam>
public interface IImageManager<T> where T : class
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
    Task<Result<Image>> AddImageAsync(
        string entityId,
        IFormFile file,
        string description,
        bool isPrincipal,
        CancellationToken ct);

    /// <summary>
    /// Deletes an image from the specified entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="publicId">The public Cloudinary ID of the image to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success or failure of the deletion.
    /// </returns>
    /// <remarks>
    /// </remarks>
    Task<Result<Unit>> DeleteImageAsync(
        string entityId,
        string publicId,
        CancellationToken ct);
}