using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Application.Users.Login;
using CineVault.Domain.Users;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Users.Login;

public class LoginUseCaseTests
{
    private readonly Mock<IUserRepository> _usersRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
        => _useCase = new LoginUseCase(_usersRepository.Object, _passwordHasher.Object, _tokenService.Object);

    [Fact]
    public async Task Execute_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("Jane Doe", "jane@email.com", "stored-hash");
        _usersRepository.Setup(r => r.GetByEmailAsync("jane@email.com"))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("plain-password", "stored-hash")).Returns(true);
        _tokenService.Setup(t => t.Generate(user)).Returns("jwt-token");

        var result = await _useCase.Execute(new LoginCommand("jane@email.com", "plain-password"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Execute_WithUnknownEmail_ReturnsUnauthorized()
    {
        _usersRepository.Setup(r => r.GetByEmailAsync("ghost@email.com"))
            .ReturnsAsync((User?)null);

        var result = await _useCase.Execute(new LoginCommand("ghost@email.com", "plain-password"));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        _tokenService.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = User.Create("Jane Doe", "jane@email.com", "stored-hash");
        _usersRepository.Setup(r => r.GetByEmailAsync("jane@email.com"))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong-password", "stored-hash")).Returns(false);

        var result = await _useCase.Execute(new LoginCommand("jane@email.com", "wrong-password"));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        _tokenService.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }
}
