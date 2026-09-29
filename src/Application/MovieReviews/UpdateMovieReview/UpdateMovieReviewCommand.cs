namespace CineVault.Application.MovieReviews.UpdateMovieReview;

public record UpdateMovieReviewCommand(Guid ReviewId, Guid UserId, int Rating);
