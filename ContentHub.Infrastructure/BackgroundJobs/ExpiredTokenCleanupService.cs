using ContentHub.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Infrastructure.BackgroundJobs;

public sealed class ExpiredTokenCleanupService
{
    private readonly ContentHubDbContext _db;

    public ExpiredTokenCleanupService(ContentHubDbContext db)
    {
        _db = db;
    }

    public async Task<ExpiredTokenCleanupResult> CleanupAsync(
        ExpiredTokenCleanupOptions options,
        CancellationToken ct = default)
    {
        var batchSize = Math.Max(options.BatchSize, 1);
        var retentionDays = Math.Max(options.RetentionDays, 1);
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        var sessions = await _db.UserSessions
            .Where(session =>
                session.ExpiresAtUtc <= cutoff ||
                session.RevokedAtUtc != null && session.RevokedAtUtc <= cutoff)
            .OrderBy(session => session.ExpiresAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);

        if (sessions.Count > 0)
        {
            _db.UserSessions.RemoveRange(sessions);
        }

        var remaining = batchSize - sessions.Count;
        var refreshTokenCount = 0;

        if (remaining > 0)
        {
            var refreshTokens = await _db.RefreshTokens
                .Where(token =>
                    token.ExpiresAtUtc <= cutoff ||
                    token.RevokedAtUtc != null && token.RevokedAtUtc <= cutoff)
                .OrderBy(token => token.ExpiresAtUtc)
                .Take(remaining)
                .ToListAsync(ct);

            refreshTokenCount = refreshTokens.Count;

            if (refreshTokenCount > 0)
            {
                _db.RefreshTokens.RemoveRange(refreshTokens);
            }
        }

        await _db.SaveChangesAsync(ct);

        return new ExpiredTokenCleanupResult(
            SessionsDeleted: sessions.Count,
            RefreshTokensDeleted: refreshTokenCount);
    }
}

public sealed record ExpiredTokenCleanupResult(
    int SessionsDeleted,
    int RefreshTokensDeleted);
