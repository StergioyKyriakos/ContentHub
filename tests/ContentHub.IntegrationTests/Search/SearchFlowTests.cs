using System.Net;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Search;

public sealed class SearchFlowTests : IntegrationTestBase
{
    public SearchFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Public_SearchPosts_Should_Work()
    {
        var response = await Client.GetAsync("/api/search/posts?q=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Authenticated_SearchAssets_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var response = await Client.GetAsync("/api/search/assets?q=test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchEverything_Should_Work()
    {
        var response = await Client.GetAsync("/api/search?q=test");

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"API rejected request with: {errorContent}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}