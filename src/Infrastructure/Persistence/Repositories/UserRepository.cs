using CineVault.Application.Abstractions;
using CineVault.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CineVaultDbContext _dbContext;

    public UserRepository(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByEmailAsync(string email)
        => _dbContext.Users.AnyAsync(user => user.Email == email);

    public Task<User?> GetByEmailAsync(string email)
        => _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);

    public async Task AddAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }
}
