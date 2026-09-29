using CineVault.Domain.Movies;
using CineVault.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace CineVault.Infrastructure.Tests.Persistence;

public class MovieRepositoryTests : RepositoryTestBase
{
    private readonly MovieRepository _repository;

    public MovieRepositoryTests() => _repository = new MovieRepository(CreateContext());

    [Fact]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        var arrange = CreateContext();
        arrange.Movies.AddRange(Movie.Create("The Matrix", 1999, null), Movie.Create("Inception", 2010, null));
        await arrange.SaveChangesAsync();

        var movies = await _repository.GetAllAsync();

        movies.Should().HaveCount(2);
        movies.Select(movie => movie.Title).Should().Contain(new[] { "The Matrix", "Inception" });
    }

    [Fact]
    public async Task ExistsByIdAsync_WhenMovieExists_ReturnsTrue()
    {
        var movie = Movie.Create("The Matrix", 1999, null);
        var arrange = CreateContext();
        arrange.Movies.Add(movie);
        await arrange.SaveChangesAsync();

        var exists = await _repository.ExistsByIdAsync(movie.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByIdAsync_WhenMovieMissing_ReturnsFalse()
    {
        var exists = await _repository.ExistsByIdAsync(Guid.NewGuid());

        exists.Should().BeFalse();
    }
}
