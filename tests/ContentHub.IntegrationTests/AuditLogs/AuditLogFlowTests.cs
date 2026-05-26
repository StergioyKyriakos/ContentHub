using System.Net;
using FluentAssertions;
using ContentHub.Data.Persistence;
using ContentHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.IntegrationTests.AuditLogs;

public sealed class AuditLogFlowTests : IntegrationTestBase
{
    public AuditLogFlowTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Admin_Can_Get_AuditLogs()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var response = await Client.GetAsync("/api/admin/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdmin_Cannot_Get_AuditLogs()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await Client.GetAsync("/api/admin/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuditLogs_Table_Should_Be_Reachable()
    {
        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var count = await db.AuditLogs.CountAsync();

        count.Should().BeGreaterThanOrEqualTo(0);
    }
    
    [Fact]
    public async Task Publishing_Post_Should_Create_AuditLog()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var post = await Cms.CreateAndPublishPostAsync();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var exists = await db.AuditLogs.AnyAsync(log =>
            log.EntityName == "Post" &&
            log.EntityId == post.Id.ToString() &&
            log.Action == ContentHub.Data.Enums.AuditAction.PostPublished);

        exists.Should().BeTrue();
    }
}