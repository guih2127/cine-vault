using CineVault.Domain.MovieReviews;

namespace CineVault.Application.Abstractions;

public interface IMovieReviewRepository
{
    Task<bool> ExistsByUserAndMovieAsync(Guid userId, Guid movieId);

    Task<MovieReview?> GetByIdAsync(Guid id);

    Task AddAsync(MovieReview movieReview);

    Task UpdateAsync(MovieReview movieReview);
}
