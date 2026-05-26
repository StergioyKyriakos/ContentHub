using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class AuthTestHelper
{
    private readonly HttpClient _client;

    public AuthTestHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> RegisterAndLoginAsync(
        string email,
        string username,
        string password = TestConstants.DefaultPassword)
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            username,
            password
        });

        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed: {body}");
        }

        return await LoginAsync(email, password);
    }

    public async Task<string> LoginAsync(
        string emailOrUsername,
        string password = TestConstants.DefaultPassword)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUsername,
            password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<LoginData>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();

        return body.Data.AccessToken;
    }

    public void UseBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearBearerToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private sealed class LoginData
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }
    }
}