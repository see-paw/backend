using Domain;

namespace Application.Interfaces;

/// <summary>
/// Ensures that only one image is set as the main image.
/// </summary>
public interface IPrincipalImageEnforcer
{
    /// <summary>
    /// Sets one image as the main one and unmarks all others.
    /// </summary>
    /// <param name="images">The collection of images.</param>
    /// <param name="newOne">The image to set as the main image.</param>
    void EnforceSinglePrincipal(ICollection<Image> images, Image newOne);
}