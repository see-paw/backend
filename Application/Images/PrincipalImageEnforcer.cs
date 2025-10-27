using Application.Images.Services;
using Domain;

namespace Application.Images;

public class PrincipalImageEnforcer : IPrincipalImageEnforcer
{
    public void EnforceSinglePrincipal(ICollection<Image> images, Image newOne)
    {
        if (!newOne.IsPrincipal) return;
        foreach (var img in images.Where(i => i.IsPrincipal && !ReferenceEquals(i, newOne)))
            img.IsPrincipal = false;
    }
}