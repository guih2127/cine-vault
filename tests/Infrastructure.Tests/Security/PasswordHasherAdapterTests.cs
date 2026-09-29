using CineVault.Infrastructure.Security;
using FluentAssertions;

namespace CineVault.Infrastructure.Tests.Security;

public class PasswordHasherAdapterTests
{
    private readonly PasswordHasherAdapter _passwordHasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _passwordHasher.Hash("my-password");

        _passwordHasher.Verify("my-password", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _passwordHasher.Hash("my-password");

        _passwordHasher.Verify("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        var hash = _passwordHasher.Hash("my-password");

        hash.Should().NotBe("my-password");
    }
}
