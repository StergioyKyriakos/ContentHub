using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class AuthTestHelper
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public AuthTestHelper(
        HttpClient client,
        ITestOutputHelper output)
    {
        _client = client;
        _output = output;
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
            displayName = username,
            password
        });

        await LogResponseAsync(registerResponse, "POST /api/auth/register response:");

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
        var data = await LoginWithRefreshAsync(emailOrUsername, password);

        return data.AccessToken;
    }

    public async Task<LoginData> LoginWithRefreshAsync(
        string emailOrUsername,
        string password = TestConstants.DefaultPassword)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            emailOrUsername,
            password
        });

        await LogResponseAsync(response, "POST /api/auth/login response:");

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<LoginData>>();

        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();

        return body.Data;
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

    private async Task<string> LogResponseAsync(
        HttpResponseMessage response,
        string label)
    {
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine(label);
        _output.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
        _output.WriteLine(body);

        return body;
    }

    public sealed class LoginData
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
