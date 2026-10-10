using CineVault.Application.Abstractions;
using CineVault.Application.Movies.GetMovieById;
using CineVault.Application.Movies.ListMovies;
using CineVault.Application.Shared;
using CineVault.Domain.Movies;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Movies.GetMovieById;

public class GetMovieByIdUseCaseTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly GetMovieByIdUseCase _useCase;

    public GetMovieByIdUseCaseTests()
        => _useCase = new GetMovieByIdUseCase(_movieRepository.Object);

    [Fact]
    public async Task Execute_WithExistingMovie_ReturnsMovieResponse()
    {
        var movie = Movie.Create("The Matrix", 1999, "https://posters/matrix.jpg");
        _movieRepository.Setup(r => r.GetByIdAsync(movie.Id)).ReturnsAsync(movie);

        var result = await _useCase.Execute(new GetMovieByIdQuery(movie.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new MovieResponse(movie.Id, "The Matrix", 1999, "https://posters/matrix.jpg"));
    }

    [Fact]
    public async Task Execute_WithUnknownMovie_ReturnsNotFound()
    {
        var movieId = Guid.NewGuid();
        _movieRepository.Setup(r => r.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

        var result = await _useCase.Execute(new GetMovieByIdQuery(movieId));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
