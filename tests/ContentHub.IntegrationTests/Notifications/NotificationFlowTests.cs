using System.Net;
using System.Net.Http.Json;
using ContentHub.Data.Entities.Notifications;
using ContentHub.Data.Enums;
using ContentHub.Data.Persistence;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Notifications;

public sealed class NotificationFlowTests : IntegrationTestBase
{
    public NotificationFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task User_Can_Get_Notifications()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await GetAsJsonAsync("/api/notifications", new
        {
        });

        await LogResponseAsync(response, "GET /api/notifications response:");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task User_Can_Mark_All_Notifications_As_Read()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await Client.PatchAsync("/api/notifications/read-all", null);

        await LogResponseAsync(response, "PATCH /api/notifications/read-all response:");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task User_Can_Mark_Notification_As_Read()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var notificationId = await CreateNotificationForUserAsync(TestConstants.AuthorEmail);

        var response = await Client.PatchAsJsonAsync($"/api/notifications/{notificationId}/read", new
        {
            id = notificationId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(response, $"PATCH /api/notifications/{notificationId}/read response:");
    }

    [Fact]
    public async Task User_Can_Get_And_Update_Notification_Preferences()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var getResponse = await Client.GetAsync("/api/notifications/preferences");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getBody = await LogResponseAsync(getResponse, "GET /api/notifications/preferences response:");

        getBody.Should().Contain("PostPublished");

        var updateResponse = await Client.PutAsJsonAsync("/api/notifications/preferences", new
        {
            preferences = new[]
            {
                new
                {
                    type = NotificationType.PostPublished,
                    channel = NotificationChannel.InApp,
                    isEnabled = false
                }
            }
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await LogResponseAsync(updateResponse, "PUT /api/notifications/preferences response:");
    }
    
    [Fact]
    public async Task Publishing_Post_Should_Create_Notification()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        await Cms.CreateAndPublishPostAsync();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var exists = await db.Notifications.AnyAsync();

        exists.Should().BeTrue();
    }

    private async Task<Guid> CreateNotificationForUserAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var user = await db.Users.FirstAsync(user => user.NormalizedEmail == email.ToUpperInvariant());

        var notification = new Notification(
            userId: user.Id,
            type: NotificationType.System,
            title: "Integration test notification",
            message: "Notification endpoint coverage.");

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        return notification.Id;
    }
}
