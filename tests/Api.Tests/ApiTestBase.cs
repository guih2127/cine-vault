using System.Net.Http.Headers;
using System.Net.Http.Json;
using CineVault.Infrastructure.Persistence;

namespace CineVault.Api.Tests;

public abstract class ApiTestBase : IDisposable
{
    protected readonly CineVaultWebApplicationFactory Factory = new();

    protected HttpClient CreateClient() => Factory.CreateClient();

    protected async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = DbSeeder.DemoEmail, password = DbSeeder.DemoPassword });

        var login = await response.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);

        return client;
    }

    public void Dispose() => Factory.Dispose();

    private sealed record TokenResponse(string Token);
}
