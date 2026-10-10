using CineVault.Application.Abstractions;
using CineVault.Domain.Movies;
using Microsoft.EntityFrameworkCore;

namespace CineVault.Infrastructure.Persistence.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly CineVaultDbContext _dbContext;

    public MovieRepository(CineVaultDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByIdAsync(Guid id)
        => _dbContext.Movies.AnyAsync(movie => movie.Id == id);

    public Task<Movie?> GetByIdAsync(Guid id)
        => _dbContext.Movies.FirstOrDefaultAsync(movie => movie.Id == id);

    public async Task<IReadOnlyList<Movie>> GetAllAsync()
        => await _dbContext.Movies.AsNoTracking().ToListAsync();

    public async Task UpdateAsync(Movie movie)
    {
        _dbContext.Movies.Update(movie);
        await _dbContext.SaveChangesAsync();
    }
}
