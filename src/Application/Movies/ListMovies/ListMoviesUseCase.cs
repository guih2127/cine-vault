using CineVault.Application.Abstractions;

namespace CineVault.Application.Movies.ListMovies;

public class ListMoviesUseCase
{
    private readonly IMovieRepository _movieRepository;

    public ListMoviesUseCase(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IReadOnlyList<MovieResponse>> Execute()
    {
        var movies = await _movieRepository.GetAllAsync();

        return movies
            .Select(movie => new MovieResponse(movie.Id, movie.Title, movie.Year, movie.PosterUrl))
            .ToList();
    }
}
