using CineVault.Domain.Users;
using CineVault.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Tests.Persistence;

public class UserRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        var user = User.Create("Jane Doe", "jane@email.com", "hash");

        await using (var context = CreateContext())
        {
            await new UserRepository(context).AddAsync(user);
        }

        await using var read = CreateContext();
        var found = await read.Users.FindAsync(user.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Jane Doe");
        found.Email.Should().Be("jane@email.com");
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserExists_ReturnsTrue()
    {
        await using (var context = CreateContext())
        {
            await new UserRepository(context).AddAsync(User.Create("Jane", "jane@email.com", "hash"));
        }

        await using var read = CreateContext();
        var exists = await new UserRepository(read).ExistsByEmailAsync("jane@email.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenNoUser_ReturnsFalse()
    {
        await using var context = CreateContext();

        var exists = await new UserRepository(context).ExistsByEmailAsync("ghost@email.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        await using (var context = CreateContext())
        {
            await new UserRepository(context).AddAsync(User.Create("Jane", "jane@email.com", "hash"));
        }

        await using var read = CreateContext();
        var user = await new UserRepository(read).GetByEmailAsync("jane@email.com");

        user.Should().NotBeNull();
        user!.Name.Should().Be("Jane");
    }
}
