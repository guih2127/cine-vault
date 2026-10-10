using CineVault.Application.Abstractions;
using CineVault.Application.Shared;

namespace CineVault.Application.Movies.RenameMovie;

public class RenameMovieUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieCache _movieCache;

    public RenameMovieUseCase(IMovieRepository movieRepository, IMovieCache movieCache)
    {
        _movieRepository = movieRepository;
        _movieCache = movieCache;
    }

    public async Task<Result<None>> Execute(RenameMovieCommand command)
    {
        var movie = await _movieRepository.GetByIdAsync(command.MovieId);

        if (movie is null)
        {
            return Result<None>.Failure(
                new Error(ErrorType.NotFound, "movie.not_found", "Movie not found."));
        }

        movie.Rename(command.Title);

        await _movieRepository.UpdateAsync(movie);
        await _movieCache.RemoveAsync(movie.Id);

        return Result<None>.Success(None.Value);
    }
}
