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
        await DbSeeder.SeedAsync(CreateContext(), new PasswordHasherAdapter());

        var read = CreateContext();
        (await read.Movies.CountAsync()).Should().BeGreaterThan(0);
        var demo = await read.Users.FirstOrDefaultAsync(user => user.Email == DbSeeder.DemoEmail);
        demo.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        await DbSeeder.SeedAsync(CreateContext(), new PasswordHasherAdapter());
        await DbSeeder.SeedAsync(CreateContext(), new PasswordHasherAdapter());

        var count = await CreateContext().Users.CountAsync(user => user.Email == DbSeeder.DemoEmail);
        count.Should().Be(1);
    }
}
