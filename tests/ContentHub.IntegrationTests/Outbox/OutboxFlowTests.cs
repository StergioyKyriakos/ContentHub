using System.Net;
using System.Net.Http.Json;
using ContentHub.Data.Entities.Outbox;
using ContentHub.Data.Persistence;
using ContentHub.IntegrationTests.Infrastructure;
using ContentHub.Infrastructure.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Outbox;

public sealed class OutboxFlowTests : IntegrationTestBase
{
    public OutboxFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task Publishing_Post_Should_Create_And_Process_Outbox_Message()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var post = await Cms.CreateDraftPostAsync();

        var publishResponse = await Client.PostAsJsonAsync($"/api/posts/{post.Id}/publish", new
        {
            id = post.Id
        });

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(publishResponse, $"POST /api/posts/{post.Id}/publish response:");

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

            var pendingMessage = await db.OutboxMessages
                .SingleAsync(message => message.Type == OutboxMessageTypes.PostPublished);

            pendingMessage.ProcessedAtUtc.Should().BeNull();
        }

        await ProcessOutboxAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

            var processedMessage = await db.OutboxMessages
                .SingleAsync(message => message.Type == OutboxMessageTypes.PostPublished);

            processedMessage.ProcessedAtUtc.Should().NotBeNull();

            var auditLogExists = await db.AuditLogs.AnyAsync(log =>
                log.EntityName == "Post" &&
                log.EntityId == post.Id.ToString() &&
                log.Action == ContentHub.Data.Enums.AuditAction.PostPublished);

            var notificationExists = await db.Notifications.AnyAsync();

            auditLogExists.Should().BeTrue();
            notificationExists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Failed_Outbox_Message_Should_Be_Retried()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

            db.OutboxMessages.Add(new OutboxMessage(
                "unsupported.test",
                "{}"));

            await db.SaveChangesAsync();
        }

        await ProcessOutboxAsync();
        await Task.Delay(TimeSpan.FromSeconds(2));
        await ProcessOutboxAsync();

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

            var message = await db.OutboxMessages.SingleAsync();

            message.ProcessedAtUtc.Should().BeNull();
            message.RetryCount.Should().Be(2);
            message.FailedAtUtc.Should().NotBeNull();
            message.Error.Should().Contain("Unsupported outbox message type");
        }
    }

    private async Task ProcessOutboxAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var processor = scope.ServiceProvider.GetRequiredService<OutboxMessageProcessor>();

        await processor.ProcessPendingAsync();
    }
}
