using CineVault.Domain.Movies;
using CineVault.Domain.Shared;
using FluentAssertions;

namespace CineVault.Domain.Tests;

public class MovieTests
{
    [Fact]
    public void Create_WithValidData_ReturnsMovie()
    {
        var movie = Movie.Create("The Matrix", "https://posters/matrix.jpg");

        movie.Id.Should().NotBeEmpty();
        movie.Title.Should().Be("The Matrix");
        movie.PosterUrl.Should().Be("https://posters/matrix.jpg");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ThrowsDomainException(string? invalidTitle)
    {
        var act = () => Movie.Create(invalidTitle!, "https://posters/matrix.jpg");

        act.Should().Throw<DomainException>()
            .WithMessage("*title*");
    }
}
