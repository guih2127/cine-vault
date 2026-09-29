using CineVault.Application.Abstractions;
using CineVault.Application.MovieReviews.CreateMovieReview;
using CineVault.Application.Shared;
using CineVault.Domain.MovieReviews;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.MovieReviews.CreateMovieReview;

public class CreateMovieReviewUseCaseTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IMovieReviewRepository> _movieReviewRepository = new();
    private readonly CreateMovieReviewUseCase _useCase;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    public CreateMovieReviewUseCaseTests()
        => _useCase = new CreateMovieReviewUseCase(_movieRepository.Object, _movieReviewRepository.Object);

    [Fact]
    public async Task Execute_WithUnknownMovie_ReturnsNotFoundAndDoesNotPersist()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(false);

        var result = await _useCase.Execute(new CreateMovieReviewCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _movieReviewRepository.Verify(r => r.AddAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithDuplicateReview_ReturnsConflictAndDoesNotPersist()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(true);
        _movieReviewRepository.Setup(r => r.ExistsByUserAndMovieAsync(UserId, MovieId)).ReturnsAsync(true);

        var result = await _useCase.Execute(new CreateMovieReviewCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _movieReviewRepository.Verify(r => r.AddAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidData_CreatesAndPersistsMovieReview()
    {
        _movieRepository.Setup(r => r.ExistsByIdAsync(MovieId)).ReturnsAsync(true);
        _movieReviewRepository.Setup(r => r.ExistsByUserAndMovieAsync(UserId, MovieId)).ReturnsAsync(false);

        var result = await _useCase.Execute(new CreateMovieReviewCommand(UserId, MovieId, 8));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        _movieReviewRepository.Verify(r => r.AddAsync(
            It.Is<MovieReview>(mr =>
                mr.UserId == UserId &&
                mr.MovieId == MovieId &&
                mr.Rating == 8)), Times.Once);
    }
}
