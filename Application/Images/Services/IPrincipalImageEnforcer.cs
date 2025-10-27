using Domain;

namespace Application.Images.Services;

public interface IPrincipalImageEnforcer
{
    void EnforceSinglePrincipal(ICollection<Image> images, Image newOne);
}