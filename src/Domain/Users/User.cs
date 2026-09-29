using CineVault.Domain.Shared;

namespace CineVault.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    private User(Guid id, string name, string email, string passwordHash)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("User email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("User password is required.");
        }

        return new User(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant(), passwordHash);
    }
}
