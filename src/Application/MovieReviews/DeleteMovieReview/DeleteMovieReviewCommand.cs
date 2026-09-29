namespace CineVault.Application.MovieReviews.DeleteMovieReview;

public record DeleteMovieReviewCommand(Guid ReviewId, Guid UserId);
