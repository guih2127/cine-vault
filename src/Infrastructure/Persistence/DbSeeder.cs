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
                Movie.Create("The Matrix", 1999, "https://image.tmdb.org/t/p/w500/lDqMDI3xpbB9UQRyeXfei0MXhqb.jpg"),
                Movie.Create("Inception", 2010, "https://image.tmdb.org/t/p/w500/9e3Dz7aCANy5aRUQF745IlNloJ1.jpg"),
                Movie.Create("The Godfather", 1972, "https://image.tmdb.org/t/p/w500/wOMxE93W6KcZTuCeNUByNTSaLLt.jpg"),
                Movie.Create("Pulp Fiction", 1994, "https://image.tmdb.org/t/p/w500/tptjnB2LDbuUWya9Cx5sQtv5hqb.jpg"),
                Movie.Create("Interstellar", 2014, "https://image.tmdb.org/t/p/w500/tR1XVa5bxgdh2bRw2u0DzrgkO2l.jpg"));
        }

        if (!await dbContext.Users.AnyAsync())
        {
            dbContext.Users.Add(User.Create(DemoName, DemoEmail, passwordHasher.Hash(DemoPassword)));
        }

        await dbContext.SaveChangesAsync();
    }
}
