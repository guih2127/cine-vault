using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Domain.Users;

namespace CineVault.Application.Users.RegisterUser;

public class RegisterUserUseCase
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public RegisterUserUseCase(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<Result<RegisterUserResult>> Execute(
        RegisterUserCommand command, CancellationToken ct = default)
    {
        if (await _users.ExistsByEmailAsync(command.Email, ct))
        {
            return Result<RegisterUserResult>.Failure(
                new Error(ErrorType.Conflict, "user.email_taken", "Email is already registered."));
        }

        var passwordHash = _hasher.Hash(command.Password);
        var user = User.Create(command.Name, command.Email, passwordHash);

        await _users.AddAsync(user, ct);

        return Result<RegisterUserResult>.Success(new RegisterUserResult(user.Id));
    }
}
