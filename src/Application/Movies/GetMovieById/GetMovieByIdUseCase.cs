using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using CineVault.Application.Shared;

namespace CineVault.Application.Movies.GetMovieById;

public class GetMovieByIdUseCase
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieByIdUseCase(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<Result<MovieResponse>> Execute(GetMovieByIdQuery query)
    {
        var movie = await _movieRepository.GetByIdAsync(query.MovieId);

        if (movie is null)
        {
            return Result<MovieResponse>.Failure(
                new Error(ErrorType.NotFound, "movie.not_found", "Movie not found."));
        }

        return Result<MovieResponse>.Success(
            new MovieResponse(movie.Id, movie.Title, movie.Year, movie.PosterUrl));
    }
}
