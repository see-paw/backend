using Domain;
using Domain.Interfaces;

namespace Application.Images.Services;

public interface IImageOwnerLinker<T> where T: class, IHasPhotos
{
    void Link(T owner, Image img, string entityId);
}