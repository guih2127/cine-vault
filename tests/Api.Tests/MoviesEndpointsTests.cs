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
}
