using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Application.Users.RegisterUser;
using CineVault.Domain.Users;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Users.RegisterUser;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
        => _useCase = new RegisterUserUseCase(_users.Object, _hasher.Object);

    [Fact]
    public async Task Execute_WithNewEmail_HashesPasswordCreatesAndPersistsUser()
    {
        _users.Setup(r => r.ExistsByEmailAsync("jane@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash("plain-password")).Returns("hashed-password");

        var result = await _useCase.Execute(
            new RegisterUserCommand("Jane Doe", "jane@email.com", "plain-password"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        _users.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.Name == "Jane Doe" &&
                u.Email == "jane@email.com" &&
                u.PasswordHash == "hashed-password"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_WithExistingEmail_ReturnsConflictAndDoesNotPersist()
    {
        _users.Setup(r => r.ExistsByEmailAsync("jane@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _useCase.Execute(
            new RegisterUserCommand("Jane Doe", "jane@email.com", "plain-password"));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
