using Application.Images.Services;
using Domain;

namespace Application.Images;

public class PrincipalImageEnforcer : IPrincipalImageEnforcer
{
    public void EnforceSinglePrincipal(ICollection<Image> images, Image newOne)
    {
        foreach (var img in images)
            img.IsPrincipal = false;
        
        newOne.IsPrincipal = true;
    }
}