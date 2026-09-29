namespace CineVault.Application.UserMovies.CreateUserMovie;

public record CreateUserMovieCommand(Guid UserId, Guid MovieId, int? Rating);
