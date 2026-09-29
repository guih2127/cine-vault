using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Domain.UserMovies;

namespace CineVault.Application.UserMovies.CreateUserMovie;

public class CreateUserMovieUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUserMovieRepository _userMovieRepository;

    public CreateUserMovieUseCase(IMovieRepository movieRepository, IUserMovieRepository userMovieRepository)
    {
        _movieRepository = movieRepository;
        _userMovieRepository = userMovieRepository;
    }

    public async Task<Result<CreateUserMovieResult>> Execute(CreateUserMovieCommand command)
    {
        if (!await _movieRepository.ExistsByIdAsync(command.MovieId))
        {
            return Result<CreateUserMovieResult>.Failure(
                new Error(ErrorType.NotFound, "movie.not_found", "Movie not found."));
        }

        if (await _userMovieRepository.ExistsByUserAndMovieAsync(command.UserId, command.MovieId))
        {
            return Result<CreateUserMovieResult>.Failure(
                new Error(ErrorType.Conflict, "user_movie.already_reviewed", "You already reviewed this movie."));
        }

        var userMovie = UserMovie.Create(command.UserId, command.MovieId, command.Rating);

        await _userMovieRepository.AddAsync(userMovie);

        return Result<CreateUserMovieResult>.Success(new CreateUserMovieResult(userMovie.Id));
    }
}
