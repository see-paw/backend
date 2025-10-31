using Application.Interfaces;
using Domain;

namespace Application.Images;

/// <summary>
/// Ensures that only one image is marked as the main image.
/// </summary>
public class PrincipalImageEnforcer : IPrincipalImageEnforcer
{
    /// <summary>
    /// Sets the given image as the main one and clears the main flag from others.
    /// </summary>
    /// <param name="images">The collection of images belonging to an entity.</param>
    /// <param name="newOne">The image to set as the main image.</param>
    public void EnforceSinglePrincipal(ICollection<Image> images, Image newOne)
    {
        foreach (var img in images)
            img.IsPrincipal = false;
        
        newOne.IsPrincipal = true;
    }
}