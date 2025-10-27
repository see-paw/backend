using Application.Images.Services;
using Domain;

namespace Application.Animals;

public class AnimalImageLinker : IImageOwnerLinker<Animal>
{
    public void Link(Animal owner, Image img, string entityId)
    {
        img.AnimalId = entityId;
        owner.Images.Add(img);
    }
}