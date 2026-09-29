using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using CineVault.Domain.Movies;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Movies.ListMovies;

public class ListMoviesUseCaseTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly ListMoviesUseCase _useCase;

    public ListMoviesUseCaseTests()
        => _useCase = new ListMoviesUseCase(_movieRepository.Object);

    [Fact]
    public async Task Execute_ReturnsAllMoviesMappedToResponse()
    {
        var matrix = Movie.Create("The Matrix", "https://posters/matrix.jpg");
        var inception = Movie.Create("Inception", null);
        _movieRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Movie> { matrix, inception });

        var result = await _useCase.Execute();

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new MovieResponse(matrix.Id, "The Matrix", "https://posters/matrix.jpg"));
        result.Should().ContainEquivalentOf(new MovieResponse(inception.Id, "Inception", null));
    }

    [Fact]
    public async Task Execute_WhenNoMovies_ReturnsEmptyList()
    {
        _movieRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Movie>());

        var result = await _useCase.Execute();

        result.Should().BeEmpty();
    }
}
