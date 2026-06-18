using ContentHub.Data.Entities.Users;
using ContentHub.Data.Persistence;
using ContentHub.Infrastructure.BackgroundJobs;
using ContentHub.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace ContentHub.IntegrationTests.Auth;

public sealed class ExpiredTokenCleanupFlowTests : IntegrationTestBase
{
    public ExpiredTokenCleanupFlowTests(
        DatabaseFixture databaseFixture,
        ITestOutputHelper output)
        : base(databaseFixture, output)
    {
    }

    [Fact]
    public async Task ExpiredTokenCleanup_Should_Remove_Only_Old_Inactive_Tokens_And_Sessions()
    {
        var now = DateTime.UtcNow;
        var oldExpiredHash = $"old-expired-{Guid.CreateVersion7()}";
        var oldRevokedHash = $"old-revoked-{Guid.CreateVersion7()}";
        var recentExpiredHash = $"recent-expired-{Guid.CreateVersion7()}";
        var activeHash = $"active-{Guid.CreateVersion7()}";

        await SeedTokensAndSessionsAsync(
            now,
            oldExpiredHash,
            oldRevokedHash,
            recentExpiredHash,
            activeHash);

        ExpiredTokenCleanupResult result;

        using (var cleanupScope = Factory.Services.CreateScope())
        {
            var cleanupService = cleanupScope.ServiceProvider.GetRequiredService<ExpiredTokenCleanupService>();

            result = await cleanupService.CleanupAsync(new ExpiredTokenCleanupOptions
            {
                RetentionDays = 7,
                BatchSize = 20
            });
        }

        result.SessionsDeleted.Should().Be(2);
        result.RefreshTokensDeleted.Should().Be(2);

        using var assertionScope = Factory.Services.CreateScope();
        var db = assertionScope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var sessionHashes = await db.UserSessions
            .AsNoTracking()
            .Select(session => session.RefreshTokenHash)
            .ToListAsync();

        var refreshTokenHashes = await db.RefreshTokens
            .AsNoTracking()
            .Select(token => token.TokenHash)
            .ToListAsync();

        sessionHashes.Should().NotContain(oldExpiredHash);
        sessionHashes.Should().NotContain(oldRevokedHash);
        sessionHashes.Should().Contain(recentExpiredHash);
        sessionHashes.Should().Contain(activeHash);

        refreshTokenHashes.Should().NotContain(oldExpiredHash);
        refreshTokenHashes.Should().NotContain(oldRevokedHash);
        refreshTokenHashes.Should().Contain(recentExpiredHash);
        refreshTokenHashes.Should().Contain(activeHash);
    }

    private async Task SeedTokensAndSessionsAsync(
        DateTime now,
        string oldExpiredHash,
        string oldRevokedHash,
        string recentExpiredHash,
        string activeHash)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

        var user = await db.Users
            .FirstAsync(user => user.NormalizedEmail == TestConstants.AdminEmail.ToUpperInvariant());

        var oldExpiredRefreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: oldExpiredHash,
            expiresAtUtc: now.AddDays(-10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        var oldRevokedRefreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: oldRevokedHash,
            expiresAtUtc: now.AddDays(10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1")
        {
            RevokedAtUtc = now.AddDays(-10)
        };

        var recentExpiredRefreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: recentExpiredHash,
            expiresAtUtc: now.AddDays(-1),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        var activeRefreshToken = new RefreshToken(
            userId: user.Id,
            tokenHash: activeHash,
            expiresAtUtc: now.AddDays(10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        var oldExpiredSession = new UserSession(
            userId: user.Id,
            refreshTokenHash: oldExpiredHash,
            expiresAtUtc: now.AddDays(-10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        var oldRevokedSession = new UserSession(
            userId: user.Id,
            refreshTokenHash: oldRevokedHash,
            expiresAtUtc: now.AddDays(10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1")
        {
            RevokedAtUtc = now.AddDays(-10)
        };

        var recentExpiredSession = new UserSession(
            userId: user.Id,
            refreshTokenHash: recentExpiredHash,
            expiresAtUtc: now.AddDays(-1),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        var activeSession = new UserSession(
            userId: user.Id,
            refreshTokenHash: activeHash,
            expiresAtUtc: now.AddDays(10),
            userAgent: "integration-test",
            ipAddress: "127.0.0.1");

        db.RefreshTokens.AddRange(
            oldExpiredRefreshToken,
            oldRevokedRefreshToken,
            recentExpiredRefreshToken,
            activeRefreshToken);

        db.UserSessions.AddRange(
            oldExpiredSession,
            oldRevokedSession,
            recentExpiredSession,
            activeSession);

        await db.SaveChangesAsync();
    }
}
