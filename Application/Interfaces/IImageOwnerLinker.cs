using Domain;
using Domain.Interfaces;

namespace Application.Interfaces;

/// <summary>
/// Links an image to its owning entity.
/// </summary>
public interface IImageOwnerLinker<T> where T: class, IHasImages
{
    /// <summary>
    /// Associates an image with the given owner entity.
    /// </summary>
    /// <param name="owner">The entity that owns the image.</param>
    /// <param name="img">The image to link.</param>
    /// <param name="entityId">The ID of the owner entity.</param>
    void Link(T owner, Image img, string entityId);
}