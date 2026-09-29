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
    {
        var normalized = Normalize(email);
        return _dbContext.Users.AnyAsync(user => user.Email == normalized);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var normalized = Normalize(email);
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Email == normalized);
    }

    public async Task AddAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
