using System.Net.Http.Json;
using Bogus;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Infrastructure;

public sealed class CmsTestHelper
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly Faker _faker = new();

    public CmsTestHelper(
        HttpClient client,
        ITestOutputHelper output)
    {
        _client = client;
        _output = output;
    }

    public async Task<Guid> CreateCategoryAsync()
    {
        var name = $"Category {_faker.Random.Guid()}";
        var slug = name.ToLowerInvariant().Replace(" ", "-");

        var response = await _client.PostAsJsonAsync("/api/categories", new
        {
            name,
            slug,
            description = _faker.Lorem.Sentence(),
            parentCategoryId = (Guid?)null,
            displayOrder = 1,
            isVisible = true
        });

        response.EnsureSuccessStatusCode();

        await LogResponseAsync(response, "POST /api/categories response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreateCategoryData>>();

        return body!.Data!.Category.Id;
    }

    public async Task<Guid> CreateAuthorAsync()
    {
        var displayName = _faker.Name.FullName();
        var slug = $"{displayName}-{_faker.Random.Int(1000, 9999)}"
            .ToLowerInvariant()
            .Replace(" ", "-");

        var response = await _client.PostAsJsonAsync("/api/authors", new
        {
            displayName,
            slug,
            bio = _faker.Lorem.Paragraph(),
            avatarAssetId = (Guid?)null,
            isActive = true
        });

        response.EnsureSuccessStatusCode();

        await LogResponseAsync(response, "POST /api/authors response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreateAuthorData>>();

        return body!.Data!.Author.Id;
    }

    public async Task<(Guid Id, string Slug)> CreateDraftPostAsync()
    {
        var categoryId = await CreateCategoryAsync();
        var authorId = await CreateAuthorAsync();

        var title = $"Post {_faker.Random.Guid()}";
        var slug = title.ToLowerInvariant().Replace(" ", "-");

        var response = await _client.PostAsJsonAsync("/api/posts", new
        {
            title,
            slug,
            summary = _faker.Lorem.Sentence(),
            content = _faker.Lorem.Paragraphs(3),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "integration", "test" }
        });

        response.EnsureSuccessStatusCode();

        await LogResponseAsync(response, "POST /api/posts response:");

        var body = await response.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();

        return (body!.Data!.Post.Id, slug);
    }

    public async Task<(Guid Id, string Slug)> CreateAndPublishPostAsync()
    {
        var post = await CreateDraftPostAsync();

        var publishResponse = await _client.PostAsJsonAsync($"/api/posts/{post.Id}/publish", new
        {
            id = post.Id
        });

        publishResponse.EnsureSuccessStatusCode();

        await LogResponseAsync(publishResponse, $"POST /api/posts/{post.Id}/publish response:");

        return post;
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

    private sealed class CreatePostData
    {
        public PostData Post { get; init; } = default!;
    }

    private sealed class PostData
    {
        public Guid Id { get; init; }
    }
}
