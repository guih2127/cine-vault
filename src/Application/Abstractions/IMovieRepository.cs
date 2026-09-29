namespace CineVault.Application.Abstractions;

public interface IMovieRepository
{
    Task<bool> ExistsByIdAsync(Guid id);
}
