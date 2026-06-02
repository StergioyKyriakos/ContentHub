using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Posts;

public sealed class PostFlowTests : IntegrationTestBase
{
    public PostFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task CreatePost_PublishPost_GetPublicPost_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();

        var categoryId = await CreateCategoryAsync(faker);
        var authorId = await CreateAuthorAsync(faker);

        var title = $"Post {faker.Random.Guid()}";
        var slug = title.ToLowerInvariant().Replace(" ", "-");

        var createPostResponse = await Client.PostAsJsonAsync("/api/posts", new
        {
            title,
            slug,
            summary = faker.Lorem.Sentence(),
            content = faker.Lorem.Paragraphs(3),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "integration", "test" }
        });

        createPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await LogResponseAsync(createPostResponse, "POST /api/posts response:");

        var createBody = await createPostResponse.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var postId = createBody.Data!.Post.Id;

        var publishResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/publish", new
        {
            id = postId
        });

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(publishResponse, $"POST /api/posts/{postId}/publish response:");

        Auth.ClearBearerToken();

        var publicResponse = await GetAsJsonAsync($"/api/public/posts/{slug}", new
        {
            slug
        });

        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicBody = await LogResponseAsync(publicResponse, $"GET /api/public/posts/{slug} response:");

        publicBody.Should().Contain(title);
    }

    [Fact]
    public async Task Post_Management_Endpoints_Should_Work()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();
        var categoryId = await CreateCategoryAsync(faker);
        var authorId = await CreateAuthorAsync(faker);

        var title = $"Managed Post {faker.Random.Guid()}";
        var slug = title.ToLowerInvariant().Replace(" ", "-");

        var createPostResponse = await Client.PostAsJsonAsync("/api/posts", new
        {
            title,
            slug,
            summary = faker.Lorem.Sentence(),
            content = faker.Lorem.Paragraphs(3),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "management", "coverage" }
        });

        createPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await LogResponseAsync(createPostResponse, "POST /api/posts response:");

        var createBody = await createPostResponse.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();
        var postId = createBody!.Data!.Post.Id;

        var getPostsResponse = await GetAsJsonAsync("/api/posts", new
        {
        });

        getPostsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(getPostsResponse, "GET /api/posts response:");

        var getByIdResponse = await GetAsJsonAsync($"/api/posts/{postId}", new
        {
            id = postId
        });

        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(getByIdResponse, $"GET /api/posts/{postId} response:");

        var draftsResponse = await GetAsJsonAsync("/api/posts/drafts", new
        {
        });

        draftsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(draftsResponse, "GET /api/posts/drafts response:");

        var updatedTitle = $"Updated {title}";
        var updatedSlug = updatedTitle.ToLowerInvariant().Replace(" ", "-");

        var updateResponse = await Client.PutAsJsonAsync($"/api/posts/{postId}", new
        {
            title = updatedTitle,
            slug = updatedSlug,
            summary = faker.Lorem.Sentence(),
            content = faker.Lorem.Paragraphs(2),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "updated", "coverage" }
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateBody = await LogResponseAsync(updateResponse, $"PUT /api/posts/{postId} response:");

        updateBody.Should().Contain(updatedTitle);

        var scheduleResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/schedule", new
        {
            id = postId,
            scheduledForUtc = DateTime.UtcNow.AddDays(1)
        });

        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(scheduleResponse, $"POST /api/posts/{postId}/schedule response:");

        var publishResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/publish", new
        {
            id = postId
        });

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(publishResponse, $"POST /api/posts/{postId}/publish response:");

        var setFeaturedResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/feature", new
        {
            id = postId
        });

        setFeaturedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(setFeaturedResponse, $"POST /api/posts/{postId}/feature response:");

        Auth.ClearBearerToken();

        var publicPostsResponse = await GetAsJsonAsync("/api/public/posts", new
        {
        });

        publicPostsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(publicPostsResponse, "GET /api/public/posts response:");

        var featuredResponse = await GetAsJsonAsync("/api/public/featured-posts", new
        {
        });

        featuredResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var featuredBody = await LogResponseAsync(featuredResponse, "GET /api/public/featured-posts response:");

        featuredBody.Should().Contain(updatedTitle);

        Auth.UseBearerToken(token);

        var removeFeaturedResponse = await DeleteAsJsonAsync($"/api/posts/{postId}/feature", new
        {
            id = postId
        });

        removeFeaturedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(removeFeaturedResponse, $"DELETE /api/posts/{postId}/feature response:");

        var unpublishResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/unpublish", new
        {
            id = postId
        });

        unpublishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(unpublishResponse, $"POST /api/posts/{postId}/unpublish response:");

        var archiveResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/archive", new
        {
            id = postId
        });

        archiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(archiveResponse, $"POST /api/posts/{postId}/archive response:");

        var deleteResponse = await DeleteAsJsonAsync($"/api/posts/{postId}", new
        {
            id = postId
        });

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(deleteResponse, $"DELETE /api/posts/{postId} response:");
    }

    private async Task<Guid> CreateCategoryAsync(Faker faker)
    {
        var name = $"Category {faker.Random.Guid()}";
        var slug = name.ToLowerInvariant().Replace(" ", "-");

        var response = await Client.PostAsJsonAsync("/api/categories", new
        {
            name,
            slug,
            description = faker.Lorem.Sentence(),
            parentCategoryId = (Guid?)null,
            displayOrder = 1,
            isVisible = true
        });

        response.EnsureSuccessStatusCode();

        await LogResponseAsync(response, "POST /api/categories response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreateCategoryData>>();

        return body!.Data!.Category.Id;
    }

    private Task<HttpResponseMessage> DeleteAsJsonAsync(string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(body)
        };

        return Client.SendAsync(request);
    }

    private async Task<Guid> CreateAuthorAsync(Faker faker)
    {
        var displayName = faker.Name.FullName();
        var slug = $"{displayName}-{faker.Random.Int(1000, 9999)}"
            .ToLowerInvariant()
            .Replace(" ", "-");

        var response = await Client.PostAsJsonAsync("/api/authors", new
        {
            displayName,
            slug,
            bio = faker.Lorem.Paragraph(),
            avatarAssetId = (Guid?)null,
            isActive = true
        });

        response.EnsureSuccessStatusCode();

        await LogResponseAsync(response, "POST /api/authors response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreateAuthorData>>();

        return body!.Data!.Author.Id;
    }

    private sealed class CreatePostData
    {
        public PostData Post { get; init; } = default!;
    }

    private sealed class PostData
    {
        public Guid Id { get; init; }
    }

    private sealed class CreateCategoryData
    {
        public CategoryData Category { get; init; } = default!;
    }

    private sealed class CategoryData
    {
        public Guid Id { get; init; }
    }

    private sealed class CreateAuthorData
    {
        public AuthorData Author { get; init; } = default!;
    }

    private sealed class AuthorData
    {
        public Guid Id { get; init; }
    }
}
