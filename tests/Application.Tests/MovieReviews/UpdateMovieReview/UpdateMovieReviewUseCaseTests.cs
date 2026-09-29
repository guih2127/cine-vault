using CineVault.Application.Abstractions;
using CineVault.Application.MovieReviews.UpdateMovieReview;
using CineVault.Application.Shared;
using CineVault.Domain.MovieReviews;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.MovieReviews.UpdateMovieReview;

public class UpdateMovieReviewUseCaseTests
{
    private readonly Mock<IMovieReviewRepository> _movieReviewRepository = new();
    private readonly UpdateMovieReviewUseCase _useCase;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    public UpdateMovieReviewUseCaseTests()
        => _useCase = new UpdateMovieReviewUseCase(_movieReviewRepository.Object);

    [Fact]
    public async Task Execute_WithUnknownReview_ReturnsNotFoundAndDoesNotPersist()
    {
        var reviewId = Guid.NewGuid();
        _movieReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((MovieReview?)null);

        var result = await _useCase.Execute(new UpdateMovieReviewCommand(reviewId, UserId, 9));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _movieReviewRepository.Verify(r => r.UpdateAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenNotOwner_ReturnsForbiddenAndDoesNotPersist()
    {
        var otherUsersReview = MovieReview.Create(Guid.NewGuid(), MovieId, 5);
        _movieReviewRepository.Setup(r => r.GetByIdAsync(otherUsersReview.Id)).ReturnsAsync(otherUsersReview);

        var result = await _useCase.Execute(new UpdateMovieReviewCommand(otherUsersReview.Id, UserId, 9));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        _movieReviewRepository.Verify(r => r.UpdateAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidData_UpdatesRatingAndPersists()
    {
        var review = MovieReview.Create(UserId, MovieId, 5);
        _movieReviewRepository.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);

        var result = await _useCase.Execute(new UpdateMovieReviewCommand(review.Id, UserId, 9));

        result.IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(9);
        _movieReviewRepository.Verify(r => r.UpdateAsync(
            It.Is<MovieReview>(mr => mr.Id == review.Id && mr.Rating == 9)), Times.Once);
    }
}
