using CineVault.Infrastructure.Persistence;
using CineVault.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Tests.Persistence;

public class DbSeederTests : RepositoryTestBase
{
    [Fact]
    public async Task SeedAsync_PopulatesMoviesAndDemoUser()
    {
        await using (var context = CreateContext())
        {
            await DbSeeder.SeedAsync(context, new PasswordHasherAdapter());
        }

        await using var read = CreateContext();
        (await read.Movies.CountAsync()).Should().BeGreaterThan(0);
        var demo = await read.Users.FirstOrDefaultAsync(user => user.Email == DbSeeder.DemoEmail);
        demo.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        await using (var context = CreateContext())
        {
            await DbSeeder.SeedAsync(context, new PasswordHasherAdapter());
            await DbSeeder.SeedAsync(context, new PasswordHasherAdapter());
        }

        await using var read = CreateContext();
        (await read.Users.CountAsync(user => user.Email == DbSeeder.DemoEmail)).Should().Be(1);
    }
}
