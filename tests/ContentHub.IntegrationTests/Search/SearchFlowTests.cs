using System.Net;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Search;

public sealed class SearchFlowTests : IntegrationTestBase
{
    public SearchFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task Public_SearchPosts_Should_Work()
    {
        var response = await GetAsJsonAsync("/api/search/posts", new
        {
            q = "test"
        });

        await LogResponseAsync(response, "GET /api/search/posts?q=test response:");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Authenticated_SearchAssets_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var response = await GetAsJsonAsync("/api/search/assets", new
        {
            q = "test"
        });

        await LogResponseAsync(response, "GET /api/search/assets?q=test response:");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchEverything_Should_Work()
    {
        var response = await GetAsJsonAsync("/api/search", new
        {
            q = "test"
        });

        var body = await LogResponseAsync(response, "GET /api/search?q=test response:");

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new Xunit.Sdk.XunitException($"API rejected request with: {body}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
