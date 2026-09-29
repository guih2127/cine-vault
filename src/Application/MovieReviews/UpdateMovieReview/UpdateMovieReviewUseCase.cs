using CineVault.Application.Abstractions;
using CineVault.Application.Shared;

namespace CineVault.Application.MovieReviews.UpdateMovieReview;

public class UpdateMovieReviewUseCase
{
    private readonly IMovieReviewRepository _movieReviewRepository;

    public UpdateMovieReviewUseCase(IMovieReviewRepository movieReviewRepository)
    {
        _movieReviewRepository = movieReviewRepository;
    }

    public async Task<Result<None>> Execute(UpdateMovieReviewCommand command)
    {
        var review = await _movieReviewRepository.GetByIdAsync(command.ReviewId);

        if (review is null)
        {
            return Result<None>.Failure(
                new Error(ErrorType.NotFound, "movie_review.not_found", "Review not found."));
        }

        if (review.UserId != command.UserId)
        {
            return Result<None>.Failure(
                new Error(ErrorType.Forbidden, "movie_review.forbidden", "You can only edit your own review."));
        }

        review.Rate(command.Rating);

        await _movieReviewRepository.UpdateAsync(review);

        return Result<None>.Success(None.Value);
    }
}
