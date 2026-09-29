using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Domain.MovieReviews;

namespace CineVault.Application.MovieReviews.CreateMovieReview;

public class CreateMovieReviewUseCase
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieReviewRepository _movieReviewRepository;

    public CreateMovieReviewUseCase(IMovieRepository movieRepository, IMovieReviewRepository movieReviewRepository)
    {
        _movieRepository = movieRepository;
        _movieReviewRepository = movieReviewRepository;
    }

    public async Task<Result<CreateMovieReviewResult>> Execute(CreateMovieReviewCommand command)
    {
        if (!await _movieRepository.ExistsByIdAsync(command.MovieId))
        {
            return Result<CreateMovieReviewResult>.Failure(
                new Error(ErrorType.NotFound, "movie.not_found", "Movie not found."));
        }

        if (await _movieReviewRepository.ExistsByUserAndMovieAsync(command.UserId, command.MovieId))
        {
            return Result<CreateMovieReviewResult>.Failure(
                new Error(ErrorType.Conflict, "movie_review.already_exists", "You already reviewed this movie."));
        }

        var movieReview = MovieReview.Create(command.UserId, command.MovieId, command.Rating);

        await _movieReviewRepository.AddAsync(movieReview);

        return Result<CreateMovieReviewResult>.Success(new CreateMovieReviewResult(movieReview.Id));
    }
}
