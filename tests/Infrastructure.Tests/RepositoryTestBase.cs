using CineVault.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Tests;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CineVaultDbContext> _options;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<CineVaultDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    protected CineVaultDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
