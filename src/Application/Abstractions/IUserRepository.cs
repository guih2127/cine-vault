using CineVault.Domain.Users;

namespace CineVault.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email);

    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);
}
