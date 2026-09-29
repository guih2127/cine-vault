using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CineVault.Api.Tests;

public class ReviewsEndpointsTests : ApiTestBase
{
    [Fact]
    public async Task ReviewLifecycle_CreateListUpdateDelete()
    {
        var client = await CreateAuthenticatedClientAsync();
        var movieId = await FirstMovieIdAsync(client);

        var create = await client.PostAsJsonAsync("/api/reviews", new { movieId, rating = 8 });
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewId = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var afterCreate = await client.GetFromJsonAsync<List<JsonElement>>("/api/reviews");
        afterCreate!.Should().ContainSingle();

        var update = await client.PutAsJsonAsync($"/api/reviews/{reviewId}", new { rating = 10 });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var delete = await client.DeleteAsync($"/api/reviews/{reviewId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await client.GetFromJsonAsync<List<JsonElement>>("/api/reviews");
        afterDelete!.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateReview_Duplicate_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        var movieId = await FirstMovieIdAsync(client);
        await client.PostAsJsonAsync("/api/reviews", new { movieId, rating = 8 });

        var duplicate = await client.PostAsJsonAsync("/api/reviews", new { movieId, rating = 8 });

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReview_UnknownMovie_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/reviews",
            new { movieId = Guid.NewGuid(), rating = 8 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReviews_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/reviews");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task<Guid> FirstMovieIdAsync(HttpClient client)
    {
        var movies = await client.GetFromJsonAsync<List<MovieDto>>("/api/movies");
        return movies!.First().Id;
    }

    private sealed record MovieDto(Guid Id, string Title, string? PosterUrl);
}
