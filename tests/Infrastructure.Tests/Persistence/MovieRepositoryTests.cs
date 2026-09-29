using CineVault.Domain.Movies;
using CineVault.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace CineVault.Infrastructure.Tests.Persistence;

public class MovieRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        await using (var context = CreateContext())
        {
            context.Movies.AddRange(Movie.Create("The Matrix", null), Movie.Create("Inception", null));
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var movies = await new MovieRepository(read).GetAllAsync();

        movies.Should().HaveCount(2);
        movies.Select(movie => movie.Title).Should().Contain(new[] { "The Matrix", "Inception" });
    }

    [Fact]
    public async Task ExistsByIdAsync_WhenMovieExists_ReturnsTrue()
    {
        var movie = Movie.Create("The Matrix", null);
        await using (var context = CreateContext())
        {
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        await using var read = CreateContext();
        var exists = await new MovieRepository(read).ExistsByIdAsync(movie.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByIdAsync_WhenMovieMissing_ReturnsFalse()
    {
        await using var context = CreateContext();

        var exists = await new MovieRepository(context).ExistsByIdAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }
}
