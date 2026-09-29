using CineVault.Domain.UserMovies;

namespace CineVault.Application.Abstractions;

public interface IUserMovieRepository
{
    Task<bool> ExistsByUserAndMovieAsync(Guid userId, Guid movieId);

    Task AddAsync(UserMovie userMovie);
}
