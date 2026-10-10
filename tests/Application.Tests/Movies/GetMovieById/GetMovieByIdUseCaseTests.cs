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
    private readonly Mock<IMovieCache> _movieCache = new();
    private readonly GetMovieByIdUseCase _useCase;

    public GetMovieByIdUseCaseTests()
        => _useCase = new GetMovieByIdUseCase(_movieRepository.Object, _movieCache.Object);

    [Fact]
    public async Task Execute_WhenCached_ReturnsCachedMovieWithoutQueryingRepository()
    {
        var cached = new MovieResponse(Guid.NewGuid(), "The Matrix", 1999, null);
        _movieCache.Setup(c => c.GetAsync(cached.Id)).ReturnsAsync(cached);

        var result = await _useCase.Execute(new GetMovieByIdQuery(cached.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cached);
        _movieRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenNotCached_ReturnsMovieFromRepositoryAndCachesIt()
    {
        var movie = Movie.Create("The Matrix", 1999, "https://posters/matrix.jpg");
        var expected = new MovieResponse(movie.Id, "The Matrix", 1999, "https://posters/matrix.jpg");
        _movieRepository.Setup(r => r.GetByIdAsync(movie.Id)).ReturnsAsync(movie);

        var result = await _useCase.Execute(new GetMovieByIdQuery(movie.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
        _movieCache.Verify(c => c.SetAsync(expected), Times.Once);
    }

    [Fact]
    public async Task Execute_WithUnknownMovie_ReturnsNotFoundAndDoesNotCache()
    {
        var movieId = Guid.NewGuid();
        _movieRepository.Setup(r => r.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

        var result = await _useCase.Execute(new GetMovieByIdQuery(movieId));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _movieCache.Verify(c => c.SetAsync(It.IsAny<MovieResponse>()), Times.Never);
    }
}
