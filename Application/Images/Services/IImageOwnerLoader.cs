using Domain.Interfaces;
using Persistence;

namespace Application.Images.Services;

public interface IImageOwnerLoader<T> where T : class, IHasPhotos
{
    Task<T> LoadAsync(AppDbContext db, string entityId, CancellationToken ct);
}