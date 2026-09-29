using CineVault.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace CineVault.Infrastructure.Security;

public class PasswordHasherAdapter : IPasswordHasher
{
    private static readonly object HashingContext = new();
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
        => _passwordHasher.HashPassword(HashingContext, password);

    public bool Verify(string password, string hash)
        => _passwordHasher.VerifyHashedPassword(HashingContext, hash, password) != PasswordVerificationResult.Failed;
}
