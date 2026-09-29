using CineVault.Domain.Users;

namespace CineVault.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);
}
