using CineVault.Application.Abstractions;
using CineVault.Application.Shared;

namespace CineVault.Application.Users.Login;

public class LoginUseCase
{
    private readonly IUserRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUseCase(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _usersRepository = users;
        _passwordHasher = hasher;
        _tokenService = tokens;
    }

    public async Task<Result<LoginResult>> Execute(LoginCommand command)
    {
        var user = await _usersRepository.GetByEmailAsync(command.Email);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Result<LoginResult>.Failure(
                new Error(ErrorType.Unauthorized, "auth.invalid_credentials", "Invalid email or password."));
        }

        var token = _tokenService.Generate(user);
        return Result<LoginResult>.Success(new LoginResult(token));
    }
}
