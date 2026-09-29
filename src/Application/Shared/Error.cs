namespace CineVault.Application.Shared;

public record Error(ErrorType Type, string Code, string Message);
