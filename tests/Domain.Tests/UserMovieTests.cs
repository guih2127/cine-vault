using CineVault.Domain.Shared;
using CineVault.Domain.UserMovies;
using FluentAssertions;

namespace CineVault.Domain.Tests;

public class UserMovieTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MovieId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsEntryWithoutRating()
    {
        var userMovie = UserMovie.Create(UserId, MovieId);

        userMovie.Id.Should().NotBeEmpty();
        userMovie.UserId.Should().Be(UserId);
        userMovie.MovieId.Should().Be(MovieId);
        userMovie.Rating.Should().BeNull();
    }

    [Fact]
    public void Create_WithRating_SetsRating()
    {
        var userMovie = UserMovie.Create(UserId, MovieId, 8);

        userMovie.Rating.Should().Be(8);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Create_WithRatingOutOfRange_ThrowsDomainException(int invalidRating)
    {
        var act = () => UserMovie.Create(UserId, MovieId, invalidRating);

        act.Should().Throw<DomainException>().WithMessage("*rating*");
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => UserMovie.Create(Guid.Empty, MovieId);

        act.Should().Throw<DomainException>().WithMessage("*user*");
    }

    [Fact]
    public void Create_WithEmptyMovieId_ThrowsDomainException()
    {
        var act = () => UserMovie.Create(UserId, Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*movie*");
    }

    [Fact]
    public void Rate_WithValidValue_UpdatesRating()
    {
        var userMovie = UserMovie.Create(UserId, MovieId);

        userMovie.Rate(9);

        userMovie.Rating.Should().Be(9);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Rate_WithOutOfRange_ThrowsDomainException(int invalidRating)
    {
        var userMovie = UserMovie.Create(UserId, MovieId);

        var act = () => userMovie.Rate(invalidRating);

        act.Should().Throw<DomainException>().WithMessage("*rating*");
    }
}
