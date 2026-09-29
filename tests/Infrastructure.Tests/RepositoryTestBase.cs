using CineVault.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Tests;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CineVaultDbContext> _options;
    private readonly List<CineVaultDbContext> _contexts = new();

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<CineVaultDbContext>()
            .UseSqlite(_connection)
            .Options;

        CreateContext().Database.EnsureCreated();
    }

    protected CineVaultDbContext CreateContext()
    {
        var context = new CineVaultDbContext(_options);
        _contexts.Add(context);
        return context;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        _connection.Dispose();
    }
}
