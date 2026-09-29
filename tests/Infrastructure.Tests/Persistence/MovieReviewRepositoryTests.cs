using CineVault.Domain.Movies;
using CineVault.Domain.MovieReviews;
using CineVault.Domain.Users;
using CineVault.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace CineVault.Infrastructure.Tests.Persistence;

public class MovieReviewRepositoryTests : RepositoryTestBase
{
    private async Task<(Guid UserId, Guid MovieId)> SeedUserAndMovieAsync(string email = "jane@email.com")
    {
        var user = User.Create("Jane", email, "hash");
        var movie = Movie.Create("The Matrix", null);

        var context = CreateContext();
        context.Users.Add(user);
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        return (user.Id, movie.Id);
    }

    [Fact]
    public async Task AddAsync_PersistsReview()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync();
        var review = MovieReview.Create(userId, movieId, 8);

        await new MovieReviewRepository(CreateContext()).AddAsync(review);

        var found = await CreateContext().MovieReviews.FindAsync(review.Id);
        found.Should().NotBeNull();
        found!.Rating.Should().Be(8);
    }

    [Fact]
    public async Task ExistsByUserAndMovieAsync_WhenExists_ReturnsTrue()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync();
        await new MovieReviewRepository(CreateContext()).AddAsync(MovieReview.Create(userId, movieId, 8));

        var exists = await new MovieReviewRepository(CreateContext()).ExistsByUserAndMovieAsync(userId, movieId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsReview()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync();
        var review = MovieReview.Create(userId, movieId, 8);
        await new MovieReviewRepository(CreateContext()).AddAsync(review);

        var found = await new MovieReviewRepository(CreateContext()).GetByIdAsync(review.Id);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(userId);
        found.MovieId.Should().Be(movieId);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyThatUsersReviews()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync("jane@email.com");
        var (otherUserId, otherMovieId) = await SeedUserAndMovieAsync("john@email.com");

        var repository = new MovieReviewRepository(CreateContext());
        await repository.AddAsync(MovieReview.Create(userId, movieId, 8));
        await repository.AddAsync(MovieReview.Create(otherUserId, otherMovieId, 5));

        var reviews = await new MovieReviewRepository(CreateContext()).GetByUserAsync(userId);

        reviews.Should().ContainSingle();
        reviews[0].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_PersistsRatingChange()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync();
        var review = MovieReview.Create(userId, movieId, 5);
        await new MovieReviewRepository(CreateContext()).AddAsync(review);

        var updateRepository = new MovieReviewRepository(CreateContext());
        var loaded = await updateRepository.GetByIdAsync(review.Id);
        loaded!.Rate(9);
        await updateRepository.UpdateAsync(loaded);

        var updated = await CreateContext().MovieReviews.FindAsync(review.Id);
        updated!.Rating.Should().Be(9);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReview()
    {
        var (userId, movieId) = await SeedUserAndMovieAsync();
        var review = MovieReview.Create(userId, movieId, 5);
        await new MovieReviewRepository(CreateContext()).AddAsync(review);

        var deleteRepository = new MovieReviewRepository(CreateContext());
        var loaded = await deleteRepository.GetByIdAsync(review.Id);
        await deleteRepository.DeleteAsync(loaded!);

        var found = await CreateContext().MovieReviews.FindAsync(review.Id);
        found.Should().BeNull();
    }
}
