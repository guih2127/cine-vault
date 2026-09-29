namespace CineVault.Application.Movies.ListMovies;

public record MovieResponse(Guid Id, string Title, int Year, string? PosterUrl);
