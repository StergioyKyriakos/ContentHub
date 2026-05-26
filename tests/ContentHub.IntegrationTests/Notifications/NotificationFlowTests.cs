using System.Net;
using ContentHub.Data.Persistence;
using FluentAssertions;
using ContentHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.IntegrationTests.Notifications;

public sealed class NotificationFlowTests : IntegrationTestBase
{
    public NotificationFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task User_Can_Get_Notifications()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await Client.GetAsync("/api/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task User_Can_Mark_All_Notifications_As_Read()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await Client.PatchAsync("/api/notifications/read-all", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
}