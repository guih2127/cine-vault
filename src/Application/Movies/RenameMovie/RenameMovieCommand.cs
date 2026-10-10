namespace CineVault.Application.Movies.RenameMovie;

public record RenameMovieCommand(Guid MovieId, string Title);
