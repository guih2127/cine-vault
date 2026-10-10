using CineVault.Application.Movies.ListMovies;

namespace CineVault.Application.Abstractions;

public interface IMovieCache
{
    Task<MovieResponse?> GetAsync(Guid movieId);

    Task SetAsync(MovieResponse movie);

    Task RemoveAsync(Guid movieId);
}
