using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace CineVault.Api.Tests;

public class MoviesEndpointsTests : ApiTestBase
{
    [Fact]
    public async Task GetMovies_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/movies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMovies_WithToken_ReturnsSeededMovies()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/movies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var movies = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        movies!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetMovieById_WithExistingMovie_ReturnsMovie()
    {
        var client = await CreateAuthenticatedClientAsync();
        var movie = await GetFirstMovieAsync(client);

        var response = await client.GetAsync($"/api/movies/{movie.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var found = await response.Content.ReadFromJsonAsync<MovieDto>();
        found.Should().Be(movie);
    }

    [Fact]
    public async Task GetMovieById_WithUnknownMovie_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/movies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RenameMovie_WithValidTitle_ReturnsNoContentAndPersists()
    {
        var client = await CreateAuthenticatedClientAsync();
        var movie = await GetFirstMovieAsync(client);

        var response = await client.PutAsJsonAsync($"/api/movies/{movie.Id}/title", new { title = "Renamed Movie" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var renamed = await client.GetFromJsonAsync<MovieDto>($"/api/movies/{movie.Id}");
        renamed!.Title.Should().Be("Renamed Movie");
    }

    [Fact]
    public async Task RenameMovie_WithEmptyTitle_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var movie = await GetFirstMovieAsync(client);

        var response = await client.PutAsJsonAsync($"/api/movies/{movie.Id}/title", new { title = "   " });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RenameMovie_WithUnknownMovie_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync($"/api/movies/{Guid.NewGuid()}/title", new { title = "Renamed Movie" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<MovieDto> GetFirstMovieAsync(HttpClient client)
    {
        var movies = await client.GetFromJsonAsync<List<MovieDto>>("/api/movies");
        return movies!.First();
    }

    private sealed record MovieDto(Guid Id, string Title, int Year, string? PosterUrl);
}
