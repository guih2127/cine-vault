using CineVault.Application.Abstractions;
using CineVault.Application.Shared;
using CineVault.Application.UserMovies.CreateUserMovie;
using CineVault.Domain.UserMovies;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.UserMovies.CreateUserMovie;

public class CreateUserMovieUseCaseTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IUserMovieRepository> _userMovieRepository = new();
    private readonly CreateUserMovieUseCase _useCase;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    public CreateUserMovieUseCaseTests()
        => _useCase = new CreateUserMovieUseCase(_movieRepository.Object, _userMovieRepository.Object);

    [Fact]
    public async Task Execute_WithUnknownMovie_ReturnsNotFoundAndDoesNotPersist()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(false);

        var result = await _useCase.Execute(new CreateUserMovieCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _userMovieRepository.Verify(r => r.AddAsync(It.IsAny<UserMovie>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithDuplicateReview_ReturnsConflictAndDoesNotPersist()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(true);
        _userMovieRepository.Setup(r => r.ExistsByUserAndMovieAsync(UserId, MovieId)).ReturnsAsync(true);

        var result = await _useCase.Execute(new CreateUserMovieCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _userMovieRepository.Verify(r => r.AddAsync(It.IsAny<UserMovie>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidData_CreatesAndPersistsUserMovie()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(true);
        _userMovieRepository.Setup(r => r.ExistsByUserAndMovieAsync(UserId, MovieId)).ReturnsAsync(false);

        var result = await _useCase.Execute(new CreateUserMovieCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        _userMovieRepository.Verify(r => r.AddAsync(
            It.Is<UserMovie>(um =>
                um.UserId == UserId &&
                um.MovieId == MovieId &&
                um.Rating == 8)), Times.Once);
    }
}
