using Application.Images.Services;
using Domain.Interfaces;
using Persistence;

namespace Application.Images;

public class ImageOwnerLoader<T> : IImageOwnerLoader<T> where T : class, IHasPhotos
{
    public async Task<T> LoadAsync(AppDbContext db, string entityId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId is required.", nameof(entityId));

        var entity = await db.Set<T>().FindAsync([entityId], ct);
        return entity ?? throw new KeyNotFoundException($"Entity {typeof(T).Name}({entityId}) not found.");
    }
}