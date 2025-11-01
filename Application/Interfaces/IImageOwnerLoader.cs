using Domain.Interfaces;
using Persistence;

namespace Application.Interfaces;

/// <summary>
/// Loads an entity that owns images from the database.
/// </summary>
public interface IImageOwnerLoader<T> where T : class, IHasImages
{
    /// <summary>
    /// Loads the specified entity and its images.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="entityId">The ID of the entity to load.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The loaded entity.</returns>
    Task<T> LoadAsync(AppDbContext db, string entityId, CancellationToken ct);
}