using CineVault.Application.Abstractions;
using CineVault.Domain.Movies;
using CineVault.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Persistence;

public static class DbSeeder
{
    public const string DemoName = "Demo User";
    public const string DemoEmail = "demo@cinevault.com";
    public const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(CineVaultDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (!await dbContext.Movies.AnyAsync())
        {
            dbContext.Movies.AddRange(
                Movie.Create("The Matrix", "https://placehold.co/300x450?text=The+Matrix"),
                Movie.Create("Inception", "https://placehold.co/300x450?text=Inception"),
                Movie.Create("The Godfather", "https://placehold.co/300x450?text=The+Godfather"),
                Movie.Create("Pulp Fiction", "https://placehold.co/300x450?text=Pulp+Fiction"),
                Movie.Create("Interstellar", "https://placehold.co/300x450?text=Interstellar"));
        }

        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.Add(User.Create(DemoName, DemoEmail, passwordHasher.Hash(DemoPassword)));
        }

        await dbContext.SaveChangesAsync();
    }
}
