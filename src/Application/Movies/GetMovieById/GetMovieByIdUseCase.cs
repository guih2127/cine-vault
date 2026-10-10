using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using CineVault.Application.Shared;

namespace CineVault.Application.Movies.GetMovieById;

public class GetMovieByIdUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieCache _movieCache;

    public GetMovieByIdUseCase(IMovieRepository movieRepository, IMovieCache movieCache)
    {
        _movieRepository = movieRepository;
        _movieCache = movieCache;
    }

    public async Task<Result<MovieResponse>> Execute(GetMovieByIdQuery query)
    {
        var cached = await _movieCache.GetAsync(query.MovieId);

        if (cached is not null)
        {
            return Result<MovieResponse>.Success(cached);
        }

        var movie = await _movieRepository.GetByIdAsync(query.MovieId);

        if (movie is null)
        {
            return Result<MovieResponse>.Failure(
                new Error(ErrorType.NotFound, "movie.not_found", "Movie not found."));
        }

        var response = new MovieResponse(movie.Id, movie.Title, movie.Year, movie.PosterUrl);
        await _movieCache.SetAsync(response);

        return Result<MovieResponse>.Success(response);
    }
}
