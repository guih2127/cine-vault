using CineVault.Domain.Movies;

namespace CineVault.Application.Abstractions;

public interface IMovieRepository
{
    Task<bool> ExistsByIdAsync(Guid id);

    Task<IReadOnlyList<Movie>> GetAllAsync();
}
