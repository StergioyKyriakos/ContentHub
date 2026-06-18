using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Categories;

public sealed class CategoryFlowTests : IntegrationTestBase
{
    public CategoryFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
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

        var getResponse = await GetAsJsonAsync("/api/categories", new
        {
        });

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await LogResponseAsync(getResponse, "GET /api/categories response:");

        body.Should().Contain(name);
    }

    [Fact]
    public async Task Category_GetById_Update_Delete_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();
        var name = $"Category {faker.Random.Guid()}";
        var slug = name.ToLowerInvariant().Replace(" ", "-");

        var createResponse = await Client.PostAsJsonAsync("/api/categories", new
        {
            name,
            slug,
            description = faker.Lorem.Sentence(),
            parentCategoryId = (Guid?)null,
            displayOrder = 1,
            isVisible = true
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await LogResponseAsync(createResponse, "POST /api/categories response:");

        var createBody = await createResponse.Content.ReadFromJsonAsync<TestApiResponse<CreateCategoryData>>();
        var categoryId = createBody!.Data!.Category.Id;

        var getByIdResponse = await GetAsJsonAsync($"/api/categories/{categoryId}", new
        {
            id = categoryId
        });

        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(getByIdResponse, $"GET /api/categories/{categoryId} response:");

        var updatedName = $"Updated {name}";

        var updateResponse = await Client.PutAsJsonAsync($"/api/categories/{categoryId}", new
        {
            name = updatedName,
            slug = updatedName.ToLowerInvariant().Replace(" ", "-"),
            description = faker.Lorem.Sentence(),
            parentCategoryId = (Guid?)null,
            displayOrder = 2,
            isVisible = false
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateBody = await LogResponseAsync(updateResponse, $"PUT /api/categories/{categoryId} response:");

        updateBody.Should().Contain(updatedName);

        var deleteResponse = await DeleteAsJsonAsync($"/api/categories/{categoryId}", new
        {
            id = categoryId
        });

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(deleteResponse, $"DELETE /api/categories/{categoryId} response:");
    }

    private sealed class CreateCategoryData
    {
        public CategoryData Category { get; init; } = default!;
    }

    private sealed class CategoryData
    {
        public Guid Id { get; init; }
    }
}
