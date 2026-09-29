using CineVault.Application.Abstractions;
using CineVault.Application.MovieReviews.ListMovieReviews;
using CineVault.Domain.MovieReviews;
using FluentAssertions;
using Moq;

namespace CineVault.Application.Tests.MovieReviews.ListMovieReviews;

public class ListMovieReviewsUseCaseTests
{
    private readonly Mock<IMovieReviewRepository> _movieReviewRepository = new();
    private readonly ListMovieReviewsUseCase _useCase;

    private static readonly Guid UserId = Guid.NewGuid();

    public ListMovieReviewsUseCaseTests()
        => _useCase = new ListMovieReviewsUseCase(_movieReviewRepository.Object);

    [Fact]
    public async Task Execute_ReturnsUsersReviewsMappedToResponse()
    {
        var first = MovieReview.Create(UserId, Guid.NewGuid(), 8);
        var second = MovieReview.Create(UserId, Guid.NewGuid(), null);
        _movieReviewRepository.Setup(r => r.GetByUserAsync(UserId))
            .ReturnsAsync(new List<MovieReview> { first, second });

        var result = await _useCase.Execute(new ListMovieReviewsQuery(UserId));

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new MovieReviewResponse(first.Id, first.MovieId, 8));
        result.Should().ContainEquivalentOf(new MovieReviewResponse(second.Id, second.MovieId, null));
    }

    [Fact]
    public async Task Execute_WhenNoReviews_ReturnsEmptyList()
    {
        _movieReviewRepository.Setup(r => r.GetByUserAsync(UserId))
            .ReturnsAsync(new List<MovieReview>());

        var result = await _useCase.Execute(new ListMovieReviewsQuery(UserId));

        result.Should().BeEmpty();
    }
}
