using System.Net;
using FluentAssertions;
using ContentHub.Data.Persistence;
using ContentHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.AuditLogs;

public sealed class AuditLogFlowTests : IntegrationTestBase
{
    public AuditLogFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task Admin_Can_Get_AuditLogs()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        var response = await GetAsJsonAsync("/api/admin/audit-logs", new
        {
        });

        await LogResponseAsync(response, "GET /api/admin/audit-logs response:");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdmin_Cannot_Get_AuditLogs()
    {
        var token = await Auth.LoginAsync(TestConstants.AuthorEmail);
        Auth.UseBearerToken(token);

        var response = await GetAsJsonAsync("/api/admin/audit-logs", new
        {
        });

        await LogResponseAsync(response, "GET /api/admin/audit-logs forbidden response:");

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

    [Fact]
    public async Task Admin_Can_Get_AuditLog_ById_And_Export()
    {
        var token = await Auth.LoginAsync(TestConstants.AdminEmail);
        Auth.UseBearerToken(token);

        await Cms.CreateCategoryAsync();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var auditLog = await db.AuditLogs
            .OrderByDescending(log => log.CreatedAtUtc)
            .FirstAsync();

        var getByIdResponse = await GetAsJsonAsync($"/api/admin/audit-logs/{auditLog.Id}", new
        {
            id = auditLog.Id
        });

        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getByIdBody = await LogResponseAsync(getByIdResponse, $"GET /api/admin/audit-logs/{auditLog.Id} response:");

        getByIdBody.Should().Contain(auditLog.Id.ToString());

        var exportResponse = await GetAsJsonAsync("/api/admin/audit-logs/export", new
        {
        });

        exportResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var exportBody = await LogResponseAsync(exportResponse, "GET /api/admin/audit-logs/export response:");

        exportBody.Should().Contain(auditLog.Id.ToString());
    }
}
