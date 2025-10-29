using Application.Interfaces;
using Domain.Interfaces;
using Persistence;

namespace Application.Images;

/// <summary>
/// Loads an entity that owns images from the database.
/// </summary>
public class ImageOwnerLoader<T> : IImageOwnerLoader<T> where T : class, IHasImages
{
    /// <summary>
    /// Loads an entity by its ID, including its images.
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="entityId">The ID of the entity to load.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The loaded entity.</returns>
    /// <exception cref="ArgumentException">Thrown when the entity ID is missing or invalid.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the entity is not found in the database.</exception>
    public async Task<T> LoadAsync(AppDbContext db, string entityId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId is required.", nameof(entityId));

        var entity = await db.Set<T>().FindAsync([entityId], ct);
        return entity ?? throw new KeyNotFoundException($"Entity {typeof(T).Name}({entityId}) not found.");
    }
}