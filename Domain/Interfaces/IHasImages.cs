namespace Domain.Interfaces;

/// <summary>
/// Defines an entity that can have associated images.
/// </summary>
public interface IHasImages
{
    /// <summary>
    /// The collection of images linked to the entity.
    /// </summary>
    ICollection<Image> Images { get; set; }
}
    

