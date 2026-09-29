using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Application.Users.RegisterUser;
using CineVault.Domain.Users;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Users.RegisterUser;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _usersRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
        => _useCase = new RegisterUserUseCase(_usersRepository.Object, _passwordHasher.Object);

    [Fact]
    public async Task Execute_WithNewEmail_HashesPasswordCreatesAndPersistsUser()
    {
        _usersRepository.Setup(r => r.ExistsByEmailAsync("jane@email.com")).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("plain-password")).Returns("hashed-password");

        var result = await _useCase.Execute(
            new RegisterUserCommand("Jane Doe", "jane@email.com", "plain-password"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        _usersRepository.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.Name == "Jane Doe" &&
                u.Email == "jane@email.com" &&
                u.PasswordHash == "hashed-password")), Times.Once);
    }

    [Fact]
    public async Task Execute_WithExistingEmail_ReturnsConflictAndDoesNotPersist()
    {
        _usersRepository.Setup(r => r.ExistsByEmailAsync("jane@email.com")).ReturnsAsync(true);

        var result = await _useCase.Execute(
            new RegisterUserCommand("Jane Doe", "jane@email.com", "plain-password"));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _usersRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
