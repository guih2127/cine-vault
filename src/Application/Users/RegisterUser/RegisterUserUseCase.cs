using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Domain.Users;

namespace CineVault.Application.Users.RegisterUser;

public class RegisterUserUseCase
{
    private readonly IUserRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(IUserRepository usersRepository, IPasswordHasher passwordHasher)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResult>> Execute(RegisterUserCommand command)
    {
        if (await _usersRepository.ExistsByEmailAsync(command.Email))
        {
            return Result<RegisterUserResult>.Failure(
                new Error(ErrorType.Conflict, "user.email_taken", "Email is already registered."));
        }

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Create(command.Name, command.Email, passwordHash);

        await _usersRepository.AddAsync(user);

        return Result<RegisterUserResult>.Success(new RegisterUserResult(user.Id));
    }
}
