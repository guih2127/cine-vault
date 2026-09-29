using CineVault.Domain.Users;

namespace CineVault.Application.Abstractions;

public interface ITokenService
{
    string Generate(User user);
}
