using CineVault.Application.Abstractions;
using CineVault.Application.Shared;

namespace CineVault.Application.Movies.RenameMovie;

public class RenameMovieUseCase
{
    private readonly IMovieRepository _movieRepository;

    public RenameMovieUseCase(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
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

        return Result<None>.Success(None.Value);
    }
}
