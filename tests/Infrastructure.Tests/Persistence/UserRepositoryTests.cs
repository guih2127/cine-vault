using CineVault.Domain.Users;
using CineVault.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace CineVault.Infrastructure.Tests.Persistence;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests() => _repository = new UserRepository(CreateContext());

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        var user = User.Create("Jane Doe", "jane@email.com", "hash");
        await _repository.AddAsync(user);

        var found = await CreateContext().Users.FindAsync(user.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Jane Doe");
        found.Email.Should().Be("jane@email.com");
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenUserExists_ReturnsTrue()
    {
        await _repository.AddAsync(User.Create("Jane", "jane@email.com", "hash"));

        var exists = await _repository.ExistsByEmailAsync("jane@email.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenNoUser_ReturnsFalse()
    {
        var exists = await _repository.ExistsByEmailAsync("ghost@email.com");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsMatchingUser()
    {
        await _repository.AddAsync(User.Create("Jane", "jane@email.com", "hash"));

        var user = await _repository.GetByEmailAsync("jane@email.com");

        user.Should().NotBeNull();
        user!.Name.Should().Be("Jane");
    }
}
