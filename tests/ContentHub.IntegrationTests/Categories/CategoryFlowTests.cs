using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Categories;

public sealed class CategoryFlowTests : IntegrationTestBase
{
    public CategoryFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task CreateCategory_GetCategories_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();

        var name = $"Category {faker.Random.Guid()}";

        var createResponse = await Client.PostAsJsonAsync("/api/categories", new
        {
            name,
            slug = name.ToLowerInvariant().Replace(" ", "-"),
            description = faker.Lorem.Sentence(),
            parentCategoryId = (Guid?)null,
            displayOrder = 1,
            isVisible = true
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await Client.GetAsync("/api/categories");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResponse.Content.ReadAsStringAsync();

        body.Should().Contain(name);
    }
}