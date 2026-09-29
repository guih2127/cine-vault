using CineVault.Application.Abstractions;
using CineVault.Application.MovieReviews.DeleteMovieReview;
using CineVault.Application.Shared;
using CineVault.Domain.MovieReviews;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.MovieReviews.DeleteMovieReview;

public class DeleteMovieReviewUseCaseTests
{
    private readonly Mock<IMovieReviewRepository> _movieReviewRepository = new();
    private readonly DeleteMovieReviewUseCase _useCase;

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    public DeleteMovieReviewUseCaseTests()
        => _useCase = new DeleteMovieReviewUseCase(_movieReviewRepository.Object);

    [Fact]
    public async Task Execute_WithUnknownReview_ReturnsNotFoundAndDoesNotDelete()
    {
        var reviewId = Guid.NewGuid();
        _movieReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((MovieReview?)null);

        var result = await _useCase.Execute(new DeleteMovieReviewCommand(reviewId, UserId));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        _movieReviewRepository.Verify(r => r.DeleteAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenNotOwner_ReturnsForbiddenAndDoesNotDelete()
    {
        var otherUsersReview = MovieReview.Create(Guid.NewGuid(), MovieId, 5);
        _movieReviewRepository.Setup(r => r.GetByIdAsync(otherUsersReview.Id)).ReturnsAsync(otherUsersReview);

        var result = await _useCase.Execute(new DeleteMovieReviewCommand(otherUsersReview.Id, UserId));

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        _movieReviewRepository.Verify(r => r.DeleteAsync(It.IsAny<MovieReview>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithOwnedReview_DeletesAndReturnsSuccess()
    {
        var review = MovieReview.Create(UserId, MovieId, 5);
        _movieReviewRepository.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);

        var result = await _useCase.Execute(new DeleteMovieReviewCommand(review.Id, UserId));

        result.IsSuccess.Should().BeTrue();
        _movieReviewRepository.Verify(r => r.DeleteAsync(
            It.Is<MovieReview>(mr => mr.Id == review.Id)), Times.Once);
    }
}
