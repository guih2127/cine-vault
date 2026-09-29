using CineVault.Domain.Shared;
using CineVault.Domain.Users;
using FluentAssertions;

namespace CineVault.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUser()
    {
        var user = User.Create("  Jane Doe  ", "Test@Email.com", "hashed-password");

        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("Jane Doe");
        user.Email.Should().Be("test@email.com");
        user.PasswordHash.Should().Be("hashed-password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ThrowsDomainException(string? invalidName)
    {
        var act = () => User.Create(invalidName!, "test@email.com", "hashed-password");

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ThrowsDomainException(string? invalidEmail)
    {
        var act = () => User.Create("Jane Doe", invalidEmail!, "hashed-password");

        act.Should().Throw<DomainException>().WithMessage("*email*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPasswordHash_ThrowsDomainException(string? invalidHash)
    {
        var act = () => User.Create("Jane Doe", "test@email.com", invalidHash!);

        act.Should().Throw<DomainException>().WithMessage("*password*");
    }
}
