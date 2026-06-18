using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Authors;

public sealed class AuthorFlowTests : IntegrationTestBase
{
    public AuthorFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
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

        var getResponse = await GetAsJsonAsync("/api/authors", new
        {
        });

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await LogResponseAsync(getResponse, "GET /api/authors response:");

        body.Should().Contain(displayName);
    }

    [Fact]
    public async Task Author_GetById_Update_GetPosts_Delete_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();
        var displayName = faker.Name.FullName();
        var slug = $"{displayName}-{faker.Random.Int(1000, 9999)}"
            .ToLowerInvariant()
            .Replace(" ", "-");

        var createResponse = await Client.PostAsJsonAsync("/api/authors", new
        {
            displayName,
            slug,
            bio = faker.Lorem.Paragraph(),
            avatarAssetId = (Guid?)null,
            isActive = true
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await LogResponseAsync(createResponse, "POST /api/authors response:");

        var createBody = await createResponse.Content.ReadFromJsonAsync<TestApiResponse<CreateAuthorData>>();
        var authorId = createBody!.Data!.Author.Id;

        var getByIdResponse = await GetAsJsonAsync($"/api/authors/{authorId}", new
        {
            id = authorId
        });

        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(getByIdResponse, $"GET /api/authors/{authorId} response:");

        var updatedName = $"Updated {displayName}";

        var updateResponse = await Client.PutAsJsonAsync($"/api/authors/{authorId}", new
        {
            displayName = updatedName,
            slug = updatedName.ToLowerInvariant().Replace(" ", "-"),
            bio = faker.Lorem.Paragraph(),
            avatarAssetId = (Guid?)null,
            isActive = true
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateBody = await LogResponseAsync(updateResponse, $"PUT /api/authors/{authorId} response:");

        updateBody.Should().Contain(updatedName);

        var categoryId = await Cms.CreateCategoryAsync();
        var title = $"Author Post {faker.Random.Guid()}";
        var postSlug = title.ToLowerInvariant().Replace(" ", "-");

        var createPostResponse = await Client.PostAsJsonAsync("/api/posts", new
        {
            title,
            slug = postSlug,
            summary = faker.Lorem.Sentence(),
            content = faker.Lorem.Paragraphs(3),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "author", "coverage" }
        });

        createPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await LogResponseAsync(createPostResponse, "POST /api/posts response:");

        var createPostBody = await createPostResponse.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();
        var postId = createPostBody!.Data!.Post.Id;

        var publishResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/publish", new
        {
            id = postId
        });

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(publishResponse, $"POST /api/posts/{postId}/publish response:");

        var postsResponse = await GetAsJsonAsync($"/api/authors/{authorId}/posts", new
        {
            id = authorId
        });

        postsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var postsBody = await LogResponseAsync(postsResponse, $"GET /api/authors/{authorId}/posts response:");

        postsBody.Should().Contain(title);

        var deleteResponse = await DeleteAsJsonAsync($"/api/authors/{authorId}", new
        {
            id = authorId
        });

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(deleteResponse, $"DELETE /api/authors/{authorId} response:");
    }

    private sealed class CreateAuthorData
    {
        public AuthorData Author { get; init; } = default!;
    }

    private sealed class AuthorData
    {
        public Guid Id { get; init; }
    }

    private sealed class CreatePostData
    {
        public PostData Post { get; init; } = default!;
    }

    private sealed class PostData
    {
        public Guid Id { get; init; }
    }
}
