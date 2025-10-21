using Domain;

namespace Application.Interfaces;

public interface IUserAcessor
{
    string GetUserId();
    Task<User> GetUserAsync();
}