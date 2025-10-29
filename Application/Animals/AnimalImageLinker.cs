using Application.Interfaces;
using Domain;

namespace Application.Animals;

/// <summary>
/// Handles updating an existing animal.
/// </summary>
public class AnimalImageLinker : IImageOwnerLinker<Animal>
{
    /// <summary>
    /// Updates an animal in the database with the provided data.
    /// </summary>
    public void Link(Animal owner, Image img, string entityId)
    {
        img.AnimalId = entityId;
        owner.Images.Add(img);
    }
}