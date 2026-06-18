using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Posts;

public sealed class ScheduledPostFlowTests : IntegrationTestBase
{
    public ScheduledPostFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(
            databaseFixture,
            output,
            new ContentHubApiFactoryOptions
            {
                BackgroundJobsEnabled = true,
                ScheduledPostPublisherIntervalSeconds = 1,
                NotificationDeliveryIntervalSeconds = 1
            })
    {
    }

    [Fact]
    public async Task Scheduled_Post_Should_Be_Published_By_Background_Job()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var faker = new Faker();
        var categoryId = await Cms.CreateCategoryAsync();
        var authorId = await Cms.CreateAuthorAsync();

        var title = $"Scheduled Post {faker.Random.Guid()}";
        var slug = title.ToLowerInvariant().Replace(" ", "-");

        var createResponse = await Client.PostAsJsonAsync("/api/posts", new
        {
            title,
            slug,
            summary = faker.Lorem.Sentence(),
            content = faker.Lorem.Paragraphs(3),
            coverAssetId = (Guid?)null,
            categoryIds = new[] { categoryId },
            authorIds = new[] { authorId },
            tags = new[] { "scheduled", "background-job" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        await LogResponseAsync(createResponse, "POST /api/posts response:");

        var createBody = await createResponse.Content.ReadFromJsonAsync<TestApiResponse<CreatePostData>>();
        createBody.Should().NotBeNull();
        createBody!.Data.Should().NotBeNull();

        var postId = createBody.Data!.Post.Id;
        var scheduledForUtc = DateTime.UtcNow.AddSeconds(2);

        var scheduleResponse = await Client.PostAsJsonAsync($"/api/posts/{postId}/schedule", new
        {
            id = postId,
            scheduledForUtc
        });

        scheduleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(
            scheduleResponse,
            $"POST /api/posts/{postId}/schedule response:");

        var scheduleApiResponse = await scheduleResponse.Content.ReadFromJsonAsync<TestApiResponse<SchedulePostData>>();
        scheduleApiResponse.Should().NotBeNull();
        scheduleApiResponse!.Data.Should().NotBeNull();
        scheduleApiResponse.Data!.Status.Should().Be(3);

        Auth.ClearBearerToken();

        var beforePublishResponse = await GetAsJsonAsync($"/api/public/posts/{slug}", new
        {
            slug
        });

        beforePublishResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await LogResponseAsync(beforePublishResponse, $"GET /api/public/posts/{slug} before publish response:");

        await WaitUntilAsync(async () =>
        {
            var response = await GetAsJsonAsync($"/api/public/posts/{slug}", new
            {
                slug
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return body.Contains(title, StringComparison.Ordinal);
        });

        var publicResponse = await GetAsJsonAsync($"/api/public/posts/{slug}", new
        {
            slug
        });

        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicBody = await LogResponseAsync(
            publicResponse,
            $"GET /api/public/posts/{slug} after publish response:");

        publicBody.Should().Contain(title);

        var searchResponse = await GetAsJsonAsync("/api/search", new
        {
            q = slug,
            page = 1,
            pageSize = 10
        });

        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchBody = await LogResponseAsync(searchResponse, "GET /api/search scheduled publish response:");

        searchBody.Should().Contain(title);

        Auth.UseBearerToken(token);

        await WaitUntilAsync(async () =>
        {
            var response = await GetAsJsonAsync("/api/admin/audit-logs", new
            {
                action = 303,
                entityName = "Post",
                entityId = postId.ToString(),
                page = 1,
                pageSize = 20
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return body.Contains(postId.ToString(), StringComparison.Ordinal) &&
                   body.Contains("PostPublished", StringComparison.Ordinal);
        });

        var auditResponse = await GetAsJsonAsync("/api/admin/audit-logs", new
        {
            action = 303,
            entityName = "Post",
            entityId = postId.ToString(),
            page = 1,
            pageSize = 20
        });

        auditResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var auditBody = await LogResponseAsync(auditResponse, "GET /api/admin/audit-logs scheduled publish response:");

        auditBody.Should().Contain("PostPublished");
        auditBody.Should().Contain(postId.ToString());

        await WaitUntilAsync(async () =>
        {
            var response = await GetAsJsonAsync("/api/notifications", new
            {
                type = 2,
                page = 1,
                pageSize = 20
            });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            return body.Contains(title, StringComparison.Ordinal);
        });

        var notificationsResponse = await GetAsJsonAsync("/api/notifications", new
        {
            type = 2,
            page = 1,
            pageSize = 20
        });

        notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var notificationsBody = await LogResponseAsync(
            notificationsResponse,
            "GET /api/notifications scheduled publish response:");

        notificationsBody.Should().Contain(title);
    }

    private async Task WaitUntilAsync(
        Func<Task<bool>> condition,
        int timeoutSeconds = 15,
        int pollMilliseconds = 500)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < TimeSpan.FromSeconds(timeoutSeconds))
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(pollMilliseconds);
        }

        throw new TimeoutException("Scheduled publish did not complete within the expected time.");
    }

    private sealed class CreatePostData
    {
        public PostData Post { get; init; } = default!;
    }

    private sealed class SchedulePostData
    {
        public Guid Id { get; init; }

        public int Status { get; init; }

        public DateTime? ScheduledForUtc { get; init; }
    }

    private sealed class PostData
    {
        public Guid Id { get; init; }
    }
}
