using CineVault.Domain.MovieReviews;

namespace CineVault.Application.Abstractions;

public interface IMovieReviewRepository
{
    Task<bool> ExistsByUserAndMovieAsync(Guid userId, Guid movieId);

    Task<MovieReview?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<MovieReview>> GetByUserAsync(Guid userId);

    Task AddAsync(MovieReview movieReview);

    Task UpdateAsync(MovieReview movieReview);

    Task DeleteAsync(MovieReview movieReview);
}
