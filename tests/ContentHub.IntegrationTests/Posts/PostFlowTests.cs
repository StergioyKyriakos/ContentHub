using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;

namespace ContentHub.IntegrationTests.Posts;

public sealed class PostFlowTests : IntegrationTestBase
{
    public PostFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
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

        var createBody = await createPostResponse.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();

        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var postId = createBody.Data!.Post.Id;

        var publishResponse = await Client.PostAsync($"/api/posts/{postId}/publish", null);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Auth.ClearBearerToken();

        var publicResponse = await Client.GetAsync($"/api/public/posts/{slug}");

        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicBody = await publicResponse.Content.ReadAsStringAsync();

        publicBody.Should().Contain(title);
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

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreateCategoryData>>();

        return body!.Data!.Category.Id;
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