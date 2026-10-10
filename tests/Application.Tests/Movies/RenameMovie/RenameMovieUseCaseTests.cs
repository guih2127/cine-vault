using CineVault.Application.Abstractions;
using CineVault.Application.Movies.RenameMovie;
using CineVault.Application.Shared;
using CineVault.Domain.Movies;
using CineVault.Domain.Shared;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.Movies.RenameMovie;

public class RenameMovieUseCaseTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IMovieCache> _movieCache = new();
    private readonly RenameMovieUseCase _useCase;

    public RenameMovieUseCaseTests()
        => _useCase = new RenameMovieUseCase(_movieRepository.Object, _movieCache.Object);

    [Fact]
    public async Task Execute_WithUnknownMovie_ReturnsNotFoundAndDoesNotPersist()
    {
        var movieId = Guid.NewGuid();
        _movieRepository.Setup(r => r.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

        var result = await _useCase.Execute(new RenameMovieCommand(movieId, "The Matrix Reloaded"));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _movieRepository.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        _movieCache.Verify(c => c.RemoveAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithEmptyTitle_ThrowsDomainExceptionAndDoesNotPersist()
    {
        var movie = Movie.Create("The Matrix", 1999, null);
        _movieRepository.Setup(r => r.GetByIdAsync(movie.Id)).ReturnsAsync(movie);

        var act = () => _useCase.Execute(new RenameMovieCommand(movie.Id, "   "));

        await act.Should().ThrowAsync<DomainException>();
        _movieRepository.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        _movieCache.Verify(c => c.RemoveAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidTitle_RenamesPersistsAndInvalidatesCache()
    {
        var movie = Movie.Create("The Matrix", 1999, null);
        _movieRepository.Setup(r => r.GetByIdAsync(movie.Id)).ReturnsAsync(movie);

        var result = await _useCase.Execute(new RenameMovieCommand(movie.Id, "The Matrix Reloaded"));

        result.IsSuccess.Should().BeTrue();
        _movieRepository.Verify(r => r.UpdateAsync(
            It.Is<Movie>(m => m.Id == movie.Id && m.Title == "The Matrix Reloaded")), Times.Once);
        _movieCache.Verify(c => c.RemoveAsync(movie.Id), Times.Once);
    }
}
