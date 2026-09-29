namespace CineVault.Application.MovieReviews.CreateMovieReview;

public record CreateMovieReviewCommand(Guid UserId, Guid MovieId, int? Rating);
