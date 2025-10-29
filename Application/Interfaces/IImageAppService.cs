using Application.Core;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Application.Interfaces;

/// <summary>
/// Defines application-level operations for managing images of an entity.
/// </summary>
public interface IImageAppService<T> where T : class, IHasImages
{
    /// <summary>
    /// Adds an image to the specified entity.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="description">A short description of the image.</param>
    /// <param name="isPrincipal">Whether the image should be the main image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> with the created <see cref="Image"/> if successful,  
    /// or an error message otherwise.
    /// </returns>
    /// <exception cref="Exception">Thrown if upload or save fails unexpectedly.</exception>
    Task<Result<Image>> AddImageAsync(
        AppDbContext dbContext,
        string entityId,
        IFormFile file,
        string description,
        bool isPrincipal,
        CancellationToken ct
    );

    /// <summary>
    /// Deletes an image from the specified entity.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="entityId">The ID of the entity that owns the image.</param>
    /// <param name="publicId">The cloud identifier of the image to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success or failure.
    /// </returns>
    /// <exception cref="Exception">Thrown if deletion or save fails unexpectedly.</exception>
    Task<Result<Unit>> DeleteImageAsync(
        AppDbContext dbContext,
        string entityId,
        string publicId,
        CancellationToken ct
    );
}