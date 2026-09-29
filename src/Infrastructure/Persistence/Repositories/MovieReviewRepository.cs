using CineVault.Application.Abstractions;
using CineVault.Domain.MovieReviews;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Persistence.Repositories;

public class MovieReviewRepository : IMovieReviewRepository
{
    private readonly CineVaultDbContext _dbContext;

    public MovieReviewRepository(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByUserAndMovieAsync(Guid userId, Guid movieId)
        => _dbContext.MovieReviews.AnyAsync(review => review.UserId == userId && review.MovieId == movieId);

    public Task<MovieReview?> GetByIdAsync(Guid id)
        => _dbContext.MovieReviews.FirstOrDefaultAsync(review => review.Id == id);

    public async Task<IReadOnlyList<MovieReview>> GetByUserAsync(Guid userId)
        => await _dbContext.MovieReviews.AsNoTracking().Where(review => review.UserId == userId).ToListAsync();

    public async Task AddAsync(MovieReview movieReview)
    {
        _dbContext.MovieReviews.Add(movieReview);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(MovieReview movieReview)
    {
        _dbContext.MovieReviews.Update(movieReview);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(MovieReview movieReview)
    {
        _dbContext.MovieReviews.Remove(movieReview);
        await _dbContext.SaveChangesAsync();
    }
}
