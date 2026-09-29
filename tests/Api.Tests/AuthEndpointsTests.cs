using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CineVault.Infrastructure.Persistence;
using FluentAssertions;

namespace CineVault.Api.Tests;

public class AuthEndpointsTests : ApiTestBase
{
    [Fact]
    public async Task Register_WithNewEmail_ReturnsOk()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { name = "New User", email = "new@cinevault.com", password = "Pass123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        var client = CreateClient();
        var payload = new { name = "Dup", email = "dup@cinevault.com", password = "Pass123!" };
        await client.PostAsJsonAsync("/api/auth/register", payload);

        var second = await client.PostAsJsonAsync("/api/auth/register", payload);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = DbSeeder.DemoEmail, password = DbSeeder.DemoPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = DbSeeder.DemoEmail, password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
