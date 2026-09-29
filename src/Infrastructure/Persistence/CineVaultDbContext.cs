using CineVault.Domain.Movies;
using CineVault.Domain.MovieReviews;
using CineVault.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Persistence;

public class CineVaultDbContext : DbContext
{
    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieReview> MovieReviews => Set<MovieReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CineVaultDbContext).Assembly);
    }
}
