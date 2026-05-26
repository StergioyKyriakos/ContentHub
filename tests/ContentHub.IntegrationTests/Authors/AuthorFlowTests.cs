using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Authors;

public sealed class AuthorFlowTests : IntegrationTestBase
{
    public AuthorFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task CreateAuthor_GetAuthors_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();

        var displayName = faker.Name.FullName();
        var slug = displayName.ToLowerInvariant().Replace(" ", "-");

        var createResponse = await Client.PostAsJsonAsync("/api/authors", new
        {
            displayName,
            slug,
            bio = faker.Lorem.Paragraph(),
            avatarAssetId = (Guid?)null,
            isActive = true
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await Client.GetAsync("/api/authors");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResponse.Content.ReadAsStringAsync();

        body.Should().Contain(displayName);
    }
}