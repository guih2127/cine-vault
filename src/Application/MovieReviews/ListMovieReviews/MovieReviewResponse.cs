namespace CineVault.Application.MovieReviews.ListMovieReviews;

public record MovieReviewResponse(Guid Id, Guid MovieId, int? Rating);
