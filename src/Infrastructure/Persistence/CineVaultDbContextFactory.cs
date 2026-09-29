using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CineVault.Infrastructure.Persistence;

public class CineVaultDbContextFactory : IDesignTimeDbContextFactory<CineVaultDbContext>
{
    public CineVaultDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CineVaultDbContext>()
            .UseSqlite("Data Source=cinevault.db")
            .Options;

        return new CineVaultDbContext(options);
    }
}
