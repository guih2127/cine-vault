using CineVault.Application.Abstractions;
using CineVault.Application.Shared;

namespace CineVault.Application.MovieReviews.DeleteMovieReview;

public class DeleteMovieReviewUseCase
{
    private readonly IMovieReviewRepository _movieReviewRepository;

    public DeleteMovieReviewUseCase(IMovieReviewRepository movieReviewRepository)
    {
        _movieReviewRepository = movieReviewRepository;
    }

    public async Task<Result<None>> Execute(DeleteMovieReviewCommand command)
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
                new Error(ErrorType.Forbidden, "movie_review.forbidden", "You can only delete your own review."));
        }

        await _movieReviewRepository.DeleteAsync(review);

        return Result<None>.Success(None.Value);
    }
}
