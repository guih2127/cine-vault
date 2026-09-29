using CineVault.Domain.MovieReviews;
using CineVault.Domain.Shared;
using FluentAssertions;

namespace CineVault.Domain.Tests;

public class MovieReviewTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsReviewWithoutRating()
    {
        var review = MovieReview.Create(UserId, MovieId);

        review.Id.Should().NotBeEmpty();
        review.UserId.Should().Be(UserId);
        review.MovieId.Should().Be(MovieId);
        review.Rating.Should().BeNull();
    }

    [Fact]
    public void Create_WithRating_SetsRating()
    {
        var review = MovieReview.Create(UserId, MovieId, 8);

        review.Rating.Should().Be(8);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Create_WithRatingOutOfRange_ThrowsDomainException(int invalidRating)
    {
        var act = () => MovieReview.Create(UserId, MovieId, invalidRating);

        act.Should().Throw<DomainException>().WithMessage("*rating*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => MovieReview.Create(Guid.Empty, MovieId);

        act.Should().Throw<DomainException>().WithMessage("*user*");
    }

    [Fact]
    public void Create_WithEmptyMovieId_ThrowsDomainException()
    {
        var act = () => MovieReview.Create(UserId, Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*movie*");
    }

    [Fact]
    public void Rate_WithValidValue_UpdatesRating()
    {
        var review = MovieReview.Create(UserId, MovieId);

        review.Rate(9);

        review.Rating.Should().Be(9);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Rate_WithOutOfRange_ThrowsDomainException(int invalidRating)
    {
        var review = MovieReview.Create(UserId, MovieId);

        var act = () => review.Rate(invalidRating);

        act.Should().Throw<DomainException>().WithMessage("*rating*");
    }
}
