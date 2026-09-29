using CineVault.Domain.MovieReviews;

namespace CineVault.Application.Abstractions;

public interface IMovieReviewRepository
{
    Task<bool> ExistsByUserAndMovieAsync(Guid userId, Guid movieId);

    Task AddAsync(MovieReview movieReview);
}
